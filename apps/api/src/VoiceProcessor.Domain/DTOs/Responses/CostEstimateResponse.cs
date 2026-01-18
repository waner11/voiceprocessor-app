using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Domain.DTOs.Responses;

public record CostEstimateResponse
{
    public required int CharacterCount { get; init; }
    public required int EstimatedChunks { get; init; }
    public required decimal EstimatedCost { get; init; }
    public required string Currency { get; init; } = "USD";
    public Provider? RecommendedProvider { get; init; }
    public IReadOnlyList<ProviderEstimate> ProviderEstimates { get; init; } = [];
}

public record ProviderEstimate
{
    public required Provider Provider { get; init; }
    public required decimal Cost { get; init; }
    public required int EstimatedDurationMs { get; init; }
    public required string QualityTier { get; init; }
    public bool IsAvailable { get; init; } = true;
}
