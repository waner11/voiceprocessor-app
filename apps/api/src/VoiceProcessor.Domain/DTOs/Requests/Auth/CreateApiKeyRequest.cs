namespace VoiceProcessor.Domain.DTOs.Requests.Auth;

public record CreateApiKeyRequest
{
    public required string Name { get; init; }
    public DateTime? ExpiresAt { get; init; }
}
