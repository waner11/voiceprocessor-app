using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Domain.Entities;

public class Voice
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Provider Provider { get; set; }
    public string ProviderVoiceId { get; set; } = string.Empty;
    public string? Language { get; set; }
    public string? Accent { get; set; }
    public string? Gender { get; set; }
    public string? AgeGroup { get; set; }
    public string? UseCase { get; set; }
    public string? PreviewUrl { get; set; }
    public decimal CostPerThousandChars { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Generation> Generations { get; set; } = new List<Generation>();
}
