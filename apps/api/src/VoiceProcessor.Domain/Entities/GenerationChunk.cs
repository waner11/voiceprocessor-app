using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Domain.Entities;

public class GenerationChunk
{
    public Guid Id { get; set; }
    public Guid GenerationId { get; set; }
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public int CharacterCount { get; set; }
    public ChunkStatus Status { get; set; } = ChunkStatus.Pending;
    public Provider? Provider { get; set; }
    public string? AudioUrl { get; set; }
    public int? AudioDurationMs { get; set; }
    public long? AudioSizeBytes { get; set; }
    public decimal? Cost { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public Generation Generation { get; set; } = null!;
}
