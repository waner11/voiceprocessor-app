namespace VoiceProcessor.Domain.DTOs.Requests.Auth;

public record SetPasswordRequest
{
    public required string NewPassword { get; init; }
}
