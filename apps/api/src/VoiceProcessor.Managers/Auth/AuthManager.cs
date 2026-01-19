using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Domain.DTOs.Requests.Auth;
using VoiceProcessor.Domain.DTOs.Responses.Auth;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Engines.Contracts;
using VoiceProcessor.Engines.Security;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Managers.Auth;

public class AuthManager : IAuthManager
{
    private readonly IUserAccessor _userAccessor;
    private readonly IRefreshTokenAccessor _refreshTokenAccessor;
    private readonly IApiKeyAccessor _apiKeyAccessor;
    private readonly IExternalLoginAccessor _externalLoginAccessor;
    private readonly IJwtEngine _jwtEngine;
    private readonly IPasswordEngine _passwordEngine;
    private readonly IApiKeyEngine _apiKeyEngine;
    private readonly IEnumerable<IOAuthEngine> _oauthEngines;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<AuthManager> _logger;

    public AuthManager(
        IUserAccessor userAccessor,
        IRefreshTokenAccessor refreshTokenAccessor,
        IApiKeyAccessor apiKeyAccessor,
        IExternalLoginAccessor externalLoginAccessor,
        IJwtEngine jwtEngine,
        IPasswordEngine passwordEngine,
        IApiKeyEngine apiKeyEngine,
        IEnumerable<IOAuthEngine> oauthEngines,
        IOptions<JwtOptions> jwtOptions,
        ILogger<AuthManager> logger)
    {
        _userAccessor = userAccessor;
        _refreshTokenAccessor = refreshTokenAccessor;
        _apiKeyAccessor = apiKeyAccessor;
        _externalLoginAccessor = externalLoginAccessor;
        _jwtEngine = jwtEngine;
        _passwordEngine = passwordEngine;
        _apiKeyEngine = apiKeyEngine;
        _oauthEngines = oauthEngines;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userAccessor.EmailExistsAsync(request.Email.ToLowerInvariant(), cancellationToken))
        {
            throw new InvalidOperationException("Email already registered");
        }

        var user = new Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            Name = request.Name,
            PasswordHash = _passwordEngine.HashPassword(request.Password),
            PasswordChangedAt = DateTime.UtcNow,
            Tier = SubscriptionTier.Free,
            CreditsRemaining = 1000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userAccessor.CreateAsync(user, cancellationToken);
        _logger.LogInformation("User registered: {UserId} ({Email})", user.Id, user.Email);

        return await GenerateAuthResponseAsync(user, null, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var user = await _userAccessor.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);

        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid credentials");
        }

        if (user.LockoutEndsAt.HasValue && user.LockoutEndsAt > DateTime.UtcNow)
        {
            throw new InvalidOperationException($"Account locked until {user.LockoutEndsAt:u}");
        }

        if (!_passwordEngine.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEndsAt = DateTime.UtcNow.AddMinutes(15);
                _logger.LogWarning("User locked out: {UserId}", user.Id);
            }
            await _userAccessor.UpdateAsync(user, cancellationToken);
            throw new InvalidOperationException("Invalid credentials");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("Account is deactivated");
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEndsAt = null;
        user.LastLoginAt = DateTime.UtcNow;
        user.LastActiveAt = DateTime.UtcNow;
        await _userAccessor.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User logged in: {UserId}", user.Id);

        return await GenerateAuthResponseAsync(user, ipAddress, cancellationToken, request.DeviceInfo);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var storedToken = await _refreshTokenAccessor.GetByTokenAsync(refreshToken, cancellationToken);

        if (storedToken is null)
        {
            throw new InvalidOperationException("Invalid refresh token");
        }

        if (!storedToken.IsActive)
        {
            if (storedToken.IsRevoked)
            {
                await _refreshTokenAccessor.RevokeAllUserTokensAsync(storedToken.UserId, cancellationToken);
                _logger.LogWarning("Refresh token reuse detected for user {UserId}", storedToken.UserId);
            }
            throw new InvalidOperationException("Invalid refresh token");
        }

        var user = storedToken.User;
        if (!user.IsActive)
        {
            throw new InvalidOperationException("Account is deactivated");
        }

        var newRefreshToken = _jwtEngine.GenerateRefreshToken();
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByToken = newRefreshToken;
        await _refreshTokenAccessor.UpdateAsync(storedToken, cancellationToken);

        var newToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshToken,
            DeviceInfo = storedToken.DeviceInfo,
            IpAddress = ipAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };
        await _refreshTokenAccessor.CreateAsync(newToken, cancellationToken);

        var accessToken = _jwtEngine.GenerateAccessToken(new JwtGenerationContext
        {
            UserId = user.Id,
            Email = user.Email,
            Tier = user.Tier.ToString(),
            Name = user.Name
        });

        return new AuthResponse
        {
            AccessToken = accessToken.Token,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshTokenExpiresAt = newToken.ExpiresAt,
            User = MapToUserInfo(user)
        };
    }

    public async Task LogoutAsync(Guid userId, string? refreshToken = null, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var token = await _refreshTokenAccessor.GetByTokenAsync(refreshToken, cancellationToken);
            if (token is not null && token.UserId == userId)
            {
                token.RevokedAt = DateTime.UtcNow;
                await _refreshTokenAccessor.UpdateAsync(token, cancellationToken);
            }
        }
        else
        {
            await _refreshTokenAccessor.RevokeAllUserTokensAsync(userId, cancellationToken);
        }

        _logger.LogInformation("User logged out: {UserId}", userId);
    }

    public async Task<ApiKeyCreatedResponse> CreateApiKeyAsync(Guid userId, CreateApiKeyRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userAccessor.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("User not found");
        }

        var keyResult = _apiKeyEngine.GenerateApiKey();

        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            KeyHash = keyResult.Hash,
            KeyPrefix = keyResult.Prefix,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _apiKeyAccessor.CreateAsync(apiKey, cancellationToken);
        _logger.LogInformation("API key created: {KeyPrefix} for user {UserId}", keyResult.Prefix, userId);

        return new ApiKeyCreatedResponse
        {
            Id = apiKey.Id,
            Name = apiKey.Name,
            KeyPrefix = apiKey.KeyPrefix,
            ApiKey = keyResult.FullKey,
            CreatedAt = apiKey.CreatedAt,
            ExpiresAt = apiKey.ExpiresAt,
            LastUsedAt = null,
            IsActive = true
        };
    }

    public async Task<IReadOnlyList<ApiKeyResponse>> GetUserApiKeysAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var keys = await _apiKeyAccessor.GetByUserIdAsync(userId, cancellationToken);
        return keys.Select(MapToApiKeyResponse).ToList();
    }

    public async Task RevokeApiKeyAsync(Guid userId, Guid apiKeyId, CancellationToken cancellationToken = default)
    {
        var apiKey = await _apiKeyAccessor.GetByIdAsync(apiKeyId, cancellationToken);

        if (apiKey is null || apiKey.UserId != userId)
        {
            throw new InvalidOperationException("API key not found");
        }

        apiKey.RevokedAt = DateTime.UtcNow;
        apiKey.IsActive = false;
        await _apiKeyAccessor.UpdateAsync(apiKey, cancellationToken);

        _logger.LogInformation("API key revoked: {KeyPrefix}", apiKey.KeyPrefix);
    }

    public async Task<AuthValidationResult> ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        var prefix = _apiKeyEngine.ExtractPrefix(apiKey);
        var storedKey = await _apiKeyAccessor.GetByPrefixAsync(prefix, cancellationToken);

        if (storedKey is null)
        {
            return new AuthValidationResult { IsValid = false, Error = "Invalid API key" };
        }

        if (!storedKey.IsValid)
        {
            return new AuthValidationResult { IsValid = false, Error = "API key is expired or revoked" };
        }

        if (!_apiKeyEngine.VerifyApiKey(apiKey, storedKey.KeyHash))
        {
            return new AuthValidationResult { IsValid = false, Error = "Invalid API key" };
        }

        var user = storedKey.User;
        if (!user.IsActive)
        {
            return new AuthValidationResult { IsValid = false, Error = "Account is deactivated" };
        }

        storedKey.LastUsedAt = DateTime.UtcNow;
        await _apiKeyAccessor.UpdateAsync(storedKey, cancellationToken);

        return new AuthValidationResult
        {
            IsValid = true,
            UserId = user.Id,
            Email = user.Email,
            Tier = user.Tier.ToString()
        };
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(
        Domain.Entities.User user,
        string? ipAddress,
        CancellationToken cancellationToken,
        string? deviceInfo = null)
    {
        var accessToken = _jwtEngine.GenerateAccessToken(new JwtGenerationContext
        {
            UserId = user.Id,
            Email = user.Email,
            Tier = user.Tier.ToString(),
            Name = user.Name
        });

        var refreshTokenValue = _jwtEngine.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenAccessor.CreateAsync(refreshToken, cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken.Token,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            User = MapToUserInfo(user)
        };
    }

    private static UserInfoResponse MapToUserInfo(Domain.Entities.User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Name = user.Name,
        Tier = user.Tier,
        CreditsRemaining = user.CreditsRemaining
    };

    private static ApiKeyResponse MapToApiKeyResponse(ApiKey key) => new()
    {
        Id = key.Id,
        Name = key.Name,
        KeyPrefix = key.KeyPrefix,
        CreatedAt = key.CreatedAt,
        ExpiresAt = key.ExpiresAt,
        LastUsedAt = key.LastUsedAt,
        IsActive = key.IsValid
    };

    // OAuth Methods

    public OAuthUrlResponse GetOAuthUrl(string provider, string redirectUri)
    {
        var engine = GetOAuthEngine(provider);
        var state = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var authUrl = engine.GetAuthorizationUrl(state, redirectUri);

        return new OAuthUrlResponse
        {
            AuthorizationUrl = authUrl,
            State = state
        };
    }

    public async Task<OAuthLoginResponse> OAuthLoginAsync(
        string provider,
        string code,
        string redirectUri,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var engine = GetOAuthEngine(provider);
        var oauthUserInfo = await engine.ExchangeCodeAsync(code, redirectUri, cancellationToken);

        // Check if external login already exists
        var existingLogin = await _externalLoginAccessor.GetByProviderAsync(
            provider, oauthUserInfo.ProviderUserId, cancellationToken);

        Domain.Entities.User user;
        bool isNewUser = false;

        if (existingLogin is not null)
        {
            // Existing OAuth login - get user
            user = existingLogin.User;
            if (!user.IsActive)
            {
                throw new InvalidOperationException("Account is deactivated");
            }
        }
        else
        {
            // Check if user with same email exists (auto-link)
            var existingUser = await _userAccessor.GetByEmailAsync(
                oauthUserInfo.Email.ToLowerInvariant(), cancellationToken);

            if (existingUser is not null)
            {
                // Auto-link OAuth to existing account
                user = existingUser;
                if (!user.IsActive)
                {
                    throw new InvalidOperationException("Account is deactivated");
                }

                await CreateExternalLoginAsync(user.Id, provider, oauthUserInfo, cancellationToken);
                _logger.LogInformation("OAuth {Provider} linked to existing user {UserId}", provider, user.Id);
            }
            else
            {
                // Create new user without password
                user = new Domain.Entities.User
                {
                    Id = Guid.NewGuid(),
                    Email = oauthUserInfo.Email.ToLowerInvariant(),
                    Name = oauthUserInfo.Name,
                    PasswordHash = null,
                    Tier = SubscriptionTier.Free,
                    CreditsRemaining = 1000,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _userAccessor.CreateAsync(user, cancellationToken);
                await CreateExternalLoginAsync(user.Id, provider, oauthUserInfo, cancellationToken);

                isNewUser = true;
                _logger.LogInformation("New user registered via OAuth {Provider}: {UserId}", provider, user.Id);
            }
        }

        // Update login tracking
        user.LastLoginAt = DateTime.UtcNow;
        user.LastActiveAt = DateTime.UtcNow;
        await _userAccessor.UpdateAsync(user, cancellationToken);

        // Generate tokens
        var authResponse = await GenerateAuthResponseAsync(user, ipAddress, cancellationToken);

        return new OAuthLoginResponse
        {
            AccessToken = authResponse.AccessToken,
            RefreshToken = authResponse.RefreshToken,
            AccessTokenExpiresAt = authResponse.AccessTokenExpiresAt,
            RefreshTokenExpiresAt = authResponse.RefreshTokenExpiresAt,
            User = authResponse.User,
            IsNewUser = isNewUser,
            Provider = provider
        };
    }

    public async Task<LinkedProvidersResponse> GetLinkedProvidersAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var logins = await _externalLoginAccessor.GetByUserIdAsync(userId, cancellationToken);

        return new LinkedProvidersResponse
        {
            Providers = logins.Select(l => new LinkedProviderInfo
            {
                Provider = l.Provider,
                Email = l.ProviderEmail,
                Name = l.ProviderName,
                LinkedAt = l.CreatedAt
            }).ToList()
        };
    }

    public async Task LinkOAuthAsync(
        Guid userId,
        string provider,
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        // Check if already linked
        var existing = await _externalLoginAccessor.GetByUserAndProviderAsync(
            userId, provider, cancellationToken);

        if (existing is not null)
        {
            throw new InvalidOperationException($"{provider} is already linked to this account");
        }

        var engine = GetOAuthEngine(provider);
        var oauthUserInfo = await engine.ExchangeCodeAsync(code, redirectUri, cancellationToken);

        // Check if this OAuth account is linked to another user
        var otherLogin = await _externalLoginAccessor.GetByProviderAsync(
            provider, oauthUserInfo.ProviderUserId, cancellationToken);

        if (otherLogin is not null)
        {
            throw new InvalidOperationException($"This {provider} account is already linked to another user");
        }

        await CreateExternalLoginAsync(userId, provider, oauthUserInfo, cancellationToken);
        _logger.LogInformation("OAuth {Provider} linked to user {UserId}", provider, userId);
    }

    public async Task UnlinkOAuthAsync(
        Guid userId,
        string provider,
        CancellationToken cancellationToken = default)
    {
        var login = await _externalLoginAccessor.GetByUserAndProviderAsync(
            userId, provider, cancellationToken);

        if (login is null)
        {
            throw new InvalidOperationException($"{provider} is not linked to this account");
        }

        // Check if user has a password or other OAuth providers
        var user = await _userAccessor.GetByIdAsync(userId, cancellationToken);
        var allLogins = await _externalLoginAccessor.GetByUserIdAsync(userId, cancellationToken);

        if (string.IsNullOrEmpty(user?.PasswordHash) && allLogins.Count <= 1)
        {
            throw new InvalidOperationException("Cannot unlink the only login method. Set a password first.");
        }

        await _externalLoginAccessor.DeleteAsync(login.Id, cancellationToken);
        _logger.LogInformation("OAuth {Provider} unlinked from user {UserId}", provider, userId);
    }

    private IOAuthEngine GetOAuthEngine(string provider)
    {
        var engine = _oauthEngines.FirstOrDefault(
            e => e.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase));

        if (engine is null)
        {
            throw new InvalidOperationException($"Unsupported OAuth provider: {provider}");
        }

        return engine;
    }

    private async Task CreateExternalLoginAsync(
        Guid userId,
        string provider,
        OAuthUserInfo oauthUserInfo,
        CancellationToken cancellationToken)
    {
        var externalLogin = new ExternalLogin
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = provider,
            ProviderUserId = oauthUserInfo.ProviderUserId,
            ProviderEmail = oauthUserInfo.Email,
            ProviderName = oauthUserInfo.Name,
            CreatedAt = DateTime.UtcNow
        };

        await _externalLoginAccessor.CreateAsync(externalLogin, cancellationToken);
    }
}
