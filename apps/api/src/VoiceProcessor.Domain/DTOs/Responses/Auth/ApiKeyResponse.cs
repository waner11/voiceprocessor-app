namespace VoiceProcessor.Domain.DTOs.Responses.Auth;

public record ApiKeyResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string KeyPrefix { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public DateTime? LastUsedAt { get; init; }
    public required bool IsActive { get; init; }
}
