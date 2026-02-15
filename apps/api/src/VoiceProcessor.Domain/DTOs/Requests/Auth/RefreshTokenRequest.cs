namespace VoiceProcessor.Domain.DTOs.Requests.Auth;

public record RefreshTokenRequest
{
    public string? RefreshToken { get; init; }
}
