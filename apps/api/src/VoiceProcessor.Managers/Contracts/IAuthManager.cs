using VoiceProcessor.Domain.DTOs.Requests.Auth;
using VoiceProcessor.Domain.DTOs.Responses.Auth;

namespace VoiceProcessor.Managers.Contracts;

public interface IAuthManager
{
    // Email/Password authentication
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, string? refreshToken = null, CancellationToken cancellationToken = default);

    // API Key management
    Task<ApiKeyCreatedResponse> CreateApiKeyAsync(Guid userId, CreateApiKeyRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApiKeyResponse>> GetUserApiKeysAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RevokeApiKeyAsync(Guid userId, Guid apiKeyId, CancellationToken cancellationToken = default);
    Task<AuthValidationResult> ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);

    // OAuth authentication
    OAuthUrlResponse GetOAuthUrl(string provider, string redirectUri);
    Task<OAuthLoginResponse> OAuthLoginAsync(string provider, string code, string redirectUri, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<LinkedProvidersResponse> GetLinkedProvidersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task LinkOAuthAsync(Guid userId, string provider, string code, string redirectUri, CancellationToken cancellationToken = default);
    Task UnlinkOAuthAsync(Guid userId, string provider, CancellationToken cancellationToken = default);
}

public record AuthValidationResult
{
    public required bool IsValid { get; init; }
    public Guid? UserId { get; init; }
    public string? Email { get; init; }
    public string? Tier { get; init; }
    public string? Error { get; init; }
}
