namespace VoiceProcessor.Domain.DTOs.Notifications;

public record FailedNotification
{
    public required Guid GenerationId { get; init; }
    public required string Error { get; init; }
}
