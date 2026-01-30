using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Domain.DTOs.Requests.Auth;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Engines.Contracts;
using VoiceProcessor.Engines.Security;
using VoiceProcessor.Managers.Auth;

namespace VoiceProcessor.Managers.Tests.Auth;

public class AuthManagerTests
{
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly Mock<IRefreshTokenAccessor> _mockRefreshTokenAccessor;
    private readonly Mock<IApiKeyAccessor> _mockApiKeyAccessor;
    private readonly Mock<IExternalLoginAccessor> _mockExternalLoginAccessor;
    private readonly Mock<IJwtEngine> _mockJwtEngine;
    private readonly Mock<IPasswordEngine> _mockPasswordEngine;
    private readonly Mock<IApiKeyEngine> _mockApiKeyEngine;
    private readonly Mock<IOAuthEngine> _mockGoogleOAuthEngine;
    private readonly Mock<IOAuthEngine> _mockGitHubOAuthEngine;
    private readonly JwtOptions _jwtOptions;
    private readonly Mock<ILogger<AuthManager>> _mockLogger;

    public AuthManagerTests()
    {
        _mockUserAccessor = new Mock<IUserAccessor>();
        _mockRefreshTokenAccessor = new Mock<IRefreshTokenAccessor>();
        _mockApiKeyAccessor = new Mock<IApiKeyAccessor>();
        _mockExternalLoginAccessor = new Mock<IExternalLoginAccessor>();
        _mockJwtEngine = new Mock<IJwtEngine>();
        _mockPasswordEngine = new Mock<IPasswordEngine>();
        _mockApiKeyEngine = new Mock<IApiKeyEngine>();
        _mockGoogleOAuthEngine = new Mock<IOAuthEngine>();
        _mockGitHubOAuthEngine = new Mock<IOAuthEngine>();
        _mockLogger = new Mock<ILogger<AuthManager>>();

        _jwtOptions = new JwtOptions
        {
            SecretKey = "test-secret-key-minimum-32-chars-long",
            Issuer = "VoiceProcessor",
            Audience = "VoiceProcessor",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        _mockGoogleOAuthEngine.Setup(x => x.Provider).Returns("google");
        _mockGitHubOAuthEngine.Setup(x => x.Provider).Returns("github");
    }

    private AuthManager CreateManager()
    {
        var oauthEngines = new List<IOAuthEngine> { _mockGoogleOAuthEngine.Object, _mockGitHubOAuthEngine.Object };
        return new AuthManager(
            _mockUserAccessor.Object,
            _mockRefreshTokenAccessor.Object,
            _mockApiKeyAccessor.Object,
            _mockExternalLoginAccessor.Object,
            _mockJwtEngine.Object,
            _mockPasswordEngine.Object,
            _mockApiKeyEngine.Object,
            oauthEngines,
            Options.Create(_jwtOptions),
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_CreatesUserAndReturnsTokens()
    {
        // Arrange
        var manager = CreateManager();
        var request = new RegisterRequest
        {
            Email = "Test@Example.COM",
            Password = "SecurePassword123!",
            Name = "Test User"
        };

        _mockUserAccessor.Setup(x => x.EmailExistsAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockPasswordEngine.Setup(x => x.HashPassword(request.Password))
            .Returns("hashed_password");

        _mockJwtEngine.Setup(x => x.GenerateAccessToken(It.IsAny<JwtGenerationContext>()))
            .Returns(new JwtGenerationResult
            {
                Token = "access_token",
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            });

        _mockJwtEngine.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        Domain.Entities.User? capturedUser = null;
        _mockUserAccessor.Setup(x => x.CreateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.User, CancellationToken>((u, ct) => capturedUser = u)
            .ReturnsAsync((Domain.Entities.User u, CancellationToken ct) => u);

        // Act
        var result = await manager.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.User.Email.Should().Be("test@example.com");
        result.User.Name.Should().Be("Test User");
        result.User.Tier.Should().Be(SubscriptionTier.Free);
        result.User.CreditsRemaining.Should().Be(1000);

        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be("test@example.com");
        capturedUser.PasswordHash.Should().Be("hashed_password");
        capturedUser.IsActive.Should().BeTrue();

        _mockUserAccessor.Verify(x => x.CreateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRefreshTokenAccessor.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "SecurePassword123!",
            Name = "Test User"
        };

        _mockUserAccessor.Setup(x => x.EmailExistsAsync("existing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await manager.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email already registered");

        _mockUserAccessor.Verify(x => x.CreateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokens()
    {
        // Arrange
        var manager = CreateManager();
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "CorrectPassword123!",
            DeviceInfo = "Chrome/Windows"
        };

        var user = new Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = "hashed_password",
            Tier = SubscriptionTier.Free,
            CreditsRemaining = 1000,
            IsActive = true,
            FailedLoginAttempts = 2,
            LockoutEndsAt = null
        };

        _mockUserAccessor.Setup(x => x.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordEngine.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);

        _mockJwtEngine.Setup(x => x.GenerateAccessToken(It.IsAny<JwtGenerationContext>()))
            .Returns(new JwtGenerationResult
            {
                Token = "access_token",
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            });

        _mockJwtEngine.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        Domain.Entities.User? updatedUser = null;
        _mockUserAccessor.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.User, CancellationToken>((u, ct) => updatedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        var result = await manager.LoginAsync(request, "192.168.1.1");

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");

        updatedUser.Should().NotBeNull();
        updatedUser!.FailedLoginAttempts.Should().Be(0);
        updatedUser.LockoutEndsAt.Should().BeNull();
        updatedUser.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        updatedUser.LastActiveAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _mockUserAccessor.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        // Arrange
        var manager = CreateManager();
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var user = new Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            IsActive = true,
            FailedLoginAttempts = 2
        };

        _mockUserAccessor.Setup(x => x.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordEngine.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(false);

        Domain.Entities.User? updatedUser = null;
        _mockUserAccessor.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.User, CancellationToken>((u, ct) => updatedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        var act = async () => await manager.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid credentials");

        updatedUser.Should().NotBeNull();
        updatedUser!.FailedLoginAttempts.Should().Be(3);
    }

    [Fact]
    public async Task LoginAsync_AccountLockedOut_ThrowsAfterFiveFailedAttempts()
    {
        // Arrange
        var manager = CreateManager();
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var user = new Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            IsActive = true,
            FailedLoginAttempts = 4
        };

        _mockUserAccessor.Setup(x => x.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordEngine.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(false);

        Domain.Entities.User? updatedUser = null;
        _mockUserAccessor.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.User, CancellationToken>((u, ct) => updatedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        var act = async () => await manager.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid credentials");

        updatedUser.Should().NotBeNull();
        updatedUser!.FailedLoginAttempts.Should().Be(5);
        updatedUser.LockoutEndsAt.Should().NotBeNull();
        updatedUser.LockoutEndsAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task LoginAsync_DeactivatedAccount_ThrowsUnauthorized()
    {
        // Arrange
        var manager = CreateManager();
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "CorrectPassword123!"
        };

        var user = new Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            IsActive = false,
            FailedLoginAttempts = 0
        };

        _mockUserAccessor.Setup(x => x.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordEngine.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var act = async () => await manager.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Account is deactivated");
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_RotatesTokenAndReturnsNew()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var oldRefreshToken = "old_refresh_token";
        var newRefreshToken = "new_refresh_token";

        var user = new Domain.Entities.User
        {
            Id = userId,
            Email = "test@example.com",
            IsActive = true
        };

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = oldRefreshToken,
            DeviceInfo = "Chrome/Windows",
            IpAddress = "192.168.1.1",
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            ExpiresAt = DateTime.UtcNow.AddDays(4),
            RevokedAt = null,
            User = user
        };

        _mockRefreshTokenAccessor.Setup(x => x.GetByTokenAsync(oldRefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _mockJwtEngine.Setup(x => x.GenerateRefreshToken())
            .Returns(newRefreshToken);

        _mockJwtEngine.Setup(x => x.GenerateAccessToken(It.IsAny<JwtGenerationContext>()))
            .Returns(new JwtGenerationResult
            {
                Token = "new_access_token",
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            });

        RefreshToken? updatedOldToken = null;
        RefreshToken? createdNewToken = null;

        _mockRefreshTokenAccessor.Setup(x => x.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((rt, ct) => updatedOldToken = rt)
            .Returns(Task.CompletedTask);

        _mockRefreshTokenAccessor.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((rt, ct) => createdNewToken = rt)
            .ReturnsAsync((RefreshToken rt, CancellationToken ct) => rt);

        // Act
        var result = await manager.RefreshTokenAsync(oldRefreshToken, "192.168.1.2");

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new_access_token");
        result.RefreshToken.Should().Be(newRefreshToken);

        updatedOldToken.Should().NotBeNull();
        updatedOldToken!.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        updatedOldToken.ReplacedByToken.Should().Be(newRefreshToken);

        createdNewToken.Should().NotBeNull();
        createdNewToken!.Token.Should().Be(newRefreshToken);
        createdNewToken.UserId.Should().Be(userId);
        createdNewToken.IpAddress.Should().Be("192.168.1.2");
        createdNewToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));

        _mockRefreshTokenAccessor.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRefreshTokenAccessor.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_RevokedToken_RevokesAllUserTokens()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var revokedToken = "revoked_token";

        var user = new Domain.Entities.User
        {
            Id = userId,
            Email = "test@example.com",
            IsActive = true
        };

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = revokedToken,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            RevokedAt = DateTime.UtcNow.AddDays(-1),
            User = user
        };

        _mockRefreshTokenAccessor.Setup(x => x.GetByTokenAsync(revokedToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        var act = async () => await manager.RefreshTokenAsync(revokedToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid refresh token");

        _mockRefreshTokenAccessor.Verify(x => x.RevokeAllUserTokensAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateApiKeyAsync_ReturnsKeyWithPrefix()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var request = new CreateApiKeyRequest
        {
            Name = "Production API Key",
            ExpiresAt = DateTime.UtcNow.AddYears(1)
        };

        var user = new Domain.Entities.User
        {
            Id = userId,
            Email = "test@example.com",
            IsActive = true
        };

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockApiKeyEngine.Setup(x => x.GenerateApiKey())
            .Returns(new ApiKeyGenerationResult
            {
                FullKey = "vp_test_1234567890abcdef",
                Prefix = "vp_test_1234",
                Hash = "hashed_key"
            });

        ApiKey? capturedApiKey = null;
        _mockApiKeyAccessor.Setup(x => x.CreateAsync(It.IsAny<ApiKey>(), It.IsAny<CancellationToken>()))
            .Callback<ApiKey, CancellationToken>((ak, ct) => capturedApiKey = ak)
            .ReturnsAsync((ApiKey ak, CancellationToken ct) => ak);

        // Act
        var result = await manager.CreateApiKeyAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.ApiKey.Should().Be("vp_test_1234567890abcdef");
        result.KeyPrefix.Should().Be("vp_test_1234");
        result.Name.Should().Be("Production API Key");
        result.IsActive.Should().BeTrue();

        capturedApiKey.Should().NotBeNull();
        capturedApiKey!.UserId.Should().Be(userId);
        capturedApiKey.KeyHash.Should().Be("hashed_key");
        capturedApiKey.KeyPrefix.Should().Be("vp_test_1234");

        _mockApiKeyAccessor.Verify(x => x.CreateAsync(It.IsAny<ApiKey>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OAuthLoginAsync_ExistingEmail_AutoLinksAccount()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var provider = "google";
        var code = "auth_code_123";
        var redirectUri = "https://app.example.com/callback";

        var oauthUserInfo = new OAuthUserInfo(
            ProviderUserId: "google_user_123",
            Email: "existing@example.com",
            Name: "Existing User"
        );

        var existingUser = new Domain.Entities.User
        {
            Id = userId,
            Email = "existing@example.com",
            Name = "Existing User",
            PasswordHash = "hashed_password",
            Tier = SubscriptionTier.Free,
            CreditsRemaining = 1000,
            IsActive = true
        };

        _mockGoogleOAuthEngine.Setup(x => x.ExchangeCodeAsync(code, redirectUri, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oauthUserInfo);

        _mockExternalLoginAccessor.Setup(x => x.GetByProviderAsync(provider, oauthUserInfo.ProviderUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExternalLogin?)null);

        _mockUserAccessor.Setup(x => x.GetByEmailAsync("existing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockJwtEngine.Setup(x => x.GenerateAccessToken(It.IsAny<JwtGenerationContext>()))
            .Returns(new JwtGenerationResult
            {
                Token = "access_token",
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            });

        _mockJwtEngine.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        ExternalLogin? capturedExternalLogin = null;
        _mockExternalLoginAccessor.Setup(x => x.CreateAsync(It.IsAny<ExternalLogin>(), It.IsAny<CancellationToken>()))
            .Callback<ExternalLogin, CancellationToken>((el, ct) => capturedExternalLogin = el)
            .Returns(Task.CompletedTask);

        // Act
        var result = await manager.OAuthLoginAsync(provider, code, redirectUri, "192.168.1.1");

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.IsNewUser.Should().BeFalse();
        result.Provider.Should().Be(provider);

        capturedExternalLogin.Should().NotBeNull();
        capturedExternalLogin!.UserId.Should().Be(userId);
        capturedExternalLogin.Provider.Should().Be(provider);
        capturedExternalLogin.ProviderUserId.Should().Be("google_user_123");
        capturedExternalLogin.ProviderEmail.Should().Be("existing@example.com");

        _mockExternalLoginAccessor.Verify(x => x.CreateAsync(It.IsAny<ExternalLogin>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUserAccessor.Verify(x => x.CreateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UnlinkOAuthAsync_OnlyAuthMethod_NoPassword_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var provider = "google";

        var user = new Domain.Entities.User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = null,
            IsActive = true
        };

        var externalLogin = new ExternalLogin
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = provider,
            ProviderUserId = "google_123"
        };

        _mockExternalLoginAccessor.Setup(x => x.GetByUserAndProviderAsync(userId, provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync(externalLogin);

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockExternalLoginAccessor.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExternalLogin> { externalLogin });

        // Act
        var act = async () => await manager.UnlinkOAuthAsync(userId, provider);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot unlink the only login method. Set a password first.");

        _mockExternalLoginAccessor.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
