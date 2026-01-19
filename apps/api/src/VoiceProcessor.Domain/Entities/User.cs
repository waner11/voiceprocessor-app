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

    // Authentication fields
    public string? PasswordHash { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndsAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public ICollection<Generation> Generations { get; set; } = [];
    public ICollection<Feedback> Feedbacks { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<ApiKey> ApiKeys { get; set; } = [];
    public ICollection<ExternalLogin> ExternalLogins { get; set; } = [];
}
