namespace VoiceProcessor.Domain.DTOs.Notifications;

public record CompletedNotification
{
    public required Guid GenerationId { get; init; }
    public required string AudioUrl { get; init; }
    public required int Duration { get; init; }
}
