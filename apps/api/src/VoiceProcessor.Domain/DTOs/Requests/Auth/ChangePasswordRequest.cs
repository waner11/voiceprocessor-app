namespace VoiceProcessor.Domain.DTOs.Requests.Auth;

public record ChangePasswordRequest
{
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
}
