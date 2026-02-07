namespace VoiceProcessor.Domain.DTOs.Notifications;

public record ProgressNotification
{
    public required string GenerationId { get; init; }
    public required int Progress { get; init; }
    public int? CurrentChunk { get; init; }
    public int? TotalChunks { get; init; }
}
