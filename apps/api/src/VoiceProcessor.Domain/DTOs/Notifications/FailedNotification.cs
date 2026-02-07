namespace VoiceProcessor.Domain.DTOs.Notifications;

public record FailedNotification
{
    public required string GenerationId { get; init; }
    public required string Error { get; init; }
}
