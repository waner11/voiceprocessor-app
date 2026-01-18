namespace VoiceProcessor.Domain.Entities;

public class Feedback
{
    public Guid Id { get; set; }
    public Guid GenerationId { get; set; }
    public Guid UserId { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public bool? WasDownloaded { get; set; }
    public int? PlaybackCount { get; set; }
    public int? PlaybackDurationMs { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Generation Generation { get; set; } = null!;
    public User User { get; set; } = null!;
}
