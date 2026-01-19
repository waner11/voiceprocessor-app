namespace VoiceProcessor.Domain.Entities;

public class ApiKey
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsValid => IsActive && !IsRevoked && !IsExpired;

    // Navigation
    public User User { get; set; } = null!;
}
