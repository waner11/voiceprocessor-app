namespace VoiceProcessor.Engines.Contracts;

public interface IOAuthEngine
{
    string Provider { get; }
    string GetAuthorizationUrl(string state, string redirectUri);
    Task<OAuthUserInfo> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default);
}

public record OAuthUserInfo(
    string ProviderUserId,
    string Email,
    string? Name
);
