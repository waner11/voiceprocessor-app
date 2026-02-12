using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Domain.Entities;

public class Generation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VoiceId { get; set; }
    public string InputText { get; set; } = string.Empty;
    public int CharacterCount { get; set; }
    public GenerationStatus Status { get; set; } = GenerationStatus.Pending;
    public RoutingPreference RoutingPreference { get; set; } = RoutingPreference.Balanced;
    public Provider? SelectedProvider { get; set; }
    public VoicePreset? Preset { get; set; }
    public string? AudioUrl { get; set; }
    public string? AudioFormat { get; set; }
    public int? AudioDurationMs { get; set; }
    public long? AudioSizeBytes { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public int ChunkCount { get; set; }
    public int ChunksCompleted { get; set; }
    public int Progress { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Voice Voice { get; set; } = null!;
    public ICollection<GenerationChunk> Chunks { get; set; } = new List<GenerationChunk>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
