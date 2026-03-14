namespace VoiceProcessor.Domain.DTOs.Requests.Auth;

public record UpdateProfileRequest
{
    public required string Name { get; init; }
}
