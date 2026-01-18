using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Domain.DTOs.Responses;

public record GenerationResponse
{
    public required Guid Id { get; init; }
    public required GenerationStatus Status { get; init; }
    public required int CharacterCount { get; init; }
    public required int Progress { get; init; }
    public int ChunkCount { get; init; }
    public int ChunksCompleted { get; init; }
    public Provider? Provider { get; init; }
    public string? AudioUrl { get; init; }
    public string? AudioFormat { get; init; }
    public int? AudioDurationMs { get; init; }
    public decimal? EstimatedCost { get; init; }
    public decimal? ActualCost { get; init; }
    public string? ErrorMessage { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}
