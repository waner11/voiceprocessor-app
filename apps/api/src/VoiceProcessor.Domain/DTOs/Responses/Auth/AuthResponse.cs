namespace VoiceProcessor.Domain.DTOs.Responses.Auth;

public record AuthResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime AccessTokenExpiresAt { get; init; }
    public required DateTime RefreshTokenExpiresAt { get; init; }
    public required UserInfoResponse User { get; init; }
}
