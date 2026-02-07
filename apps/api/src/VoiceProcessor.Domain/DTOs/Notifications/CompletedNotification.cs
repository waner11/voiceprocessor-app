namespace VoiceProcessor.Domain.DTOs.Notifications;

public record CompletedNotification
{
    public required string GenerationId { get; init; }
    public required string AudioUrl { get; init; }
    public required int Duration { get; init; }
}
