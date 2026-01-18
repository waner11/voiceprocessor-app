using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public SubscriptionTier Tier { get; set; } = SubscriptionTier.Free;
    public int CreditsRemaining { get; set; }
    public int CreditsUsedThisMonth { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActiveAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Generation> Generations { get; set; } = [];
    public ICollection<Feedback> Feedbacks { get; set; } = [];
}
