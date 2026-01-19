namespace VoiceProcessor.Domain.DTOs.Requests.Auth;

public record RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}
