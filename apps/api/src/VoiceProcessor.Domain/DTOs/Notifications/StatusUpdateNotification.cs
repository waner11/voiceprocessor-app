namespace VoiceProcessor.Domain.DTOs.Notifications;

public record StatusUpdateNotification
{
    public required string GenerationId { get; init; }
    public required string Status { get; init; }
    public string? Message { get; init; }
}
