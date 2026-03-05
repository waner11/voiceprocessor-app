namespace VoiceProcessor.Domain.DTOs.Requests.Auth;

public record ForgotPasswordRequest
{
    public required string Email { get; init; }
}
