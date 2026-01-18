using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Engines.Contracts;

public interface IRoutingEngine
{
    Task<RoutingDecision> SelectProviderAsync(
        RoutingContext context,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProviderScore>> ScoreProvidersAsync(
        RoutingContext context,
        CancellationToken cancellationToken = default);
}

public record RoutingContext
{
    public required Guid VoiceId { get; init; }
    public required int CharacterCount { get; init; }
    public required RoutingPreference Preference { get; init; }
    public Provider? PreferredProvider { get; init; }

    /// <summary>
    /// The provider that owns this voice (if voice is provider-specific)
    /// </summary>
    public Provider? VoiceProvider { get; init; }

    /// <summary>
    /// Set of providers that are currently available/configured
    /// </summary>
    public IReadOnlySet<Provider> AvailableProviders { get; init; } = new HashSet<Provider>();
}

public record RoutingDecision
{
    public required Provider SelectedProvider { get; init; }
    public required string Reason { get; init; }
    public decimal EstimatedCost { get; init; }
    public int EstimatedLatencyMs { get; init; }
}

public record ProviderScore
{
    public required Provider Provider { get; init; }
    public required double Score { get; init; }
    public required bool IsAvailable { get; init; }
    public decimal CostPerThousandChars { get; init; }
    public int AvgLatencyMs { get; init; }
    public double QualityRating { get; init; }
}
