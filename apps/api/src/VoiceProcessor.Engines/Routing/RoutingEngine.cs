using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines.Routing;

public class RoutingEngine : IRoutingEngine
{
    private readonly ILogger<RoutingEngine> _logger;
    private readonly RoutingOptions _options;

    // Provider characteristics (can be overridden via config)
    private static readonly Dictionary<Provider, ProviderCharacteristics> DefaultCharacteristics = new()
    {
        [Provider.ElevenLabs] = new(CostPerK: 0.30m, AvgLatencyMs: 800, QualityRating: 0.95),
        [Provider.OpenAI] = new(CostPerK: 0.015m, AvgLatencyMs: 600, QualityRating: 0.85),
        [Provider.GoogleCloud] = new(CostPerK: 0.016m, AvgLatencyMs: 400, QualityRating: 0.80),
        [Provider.AmazonPolly] = new(CostPerK: 0.004m, AvgLatencyMs: 300, QualityRating: 0.70),
        [Provider.FishAudio] = new(CostPerK: 0.20m, AvgLatencyMs: 900, QualityRating: 0.90),
        [Provider.Cartesia] = new(CostPerK: 0.25m, AvgLatencyMs: 500, QualityRating: 0.88),
        [Provider.Deepgram] = new(CostPerK: 0.015m, AvgLatencyMs: 350, QualityRating: 0.82)
    };

    public RoutingEngine(
        IOptions<RoutingOptions> options,
        ILogger<RoutingEngine> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<RoutingDecision> SelectProviderAsync(
        RoutingContext context,
        CancellationToken cancellationToken = default)
    {
        var scores = ScoreProviders(context);
        var bestProvider = scores
            .Where(s => s.IsAvailable)
            .OrderByDescending(s => s.Score)
            .FirstOrDefault();

        if (bestProvider is null)
        {
            _logger.LogWarning("No available providers for routing context {@Context}", context);
            throw new InvalidOperationException("No TTS providers available");
        }

        var characteristics = GetCharacteristics(bestProvider.Provider);
        var estimatedCost = context.CharacterCount * characteristics.CostPerK / 1000m;

        _logger.LogInformation(
            "Routing selected {Provider} for voice {VoiceId} with preference {Preference} (score: {Score:F2})",
            bestProvider.Provider, context.VoiceId, context.Preference, bestProvider.Score);

        return Task.FromResult(new RoutingDecision
        {
            SelectedProvider = bestProvider.Provider,
            Reason = GetRoutingReason(context.Preference, bestProvider),
            EstimatedCost = estimatedCost,
            EstimatedLatencyMs = characteristics.AvgLatencyMs
        });
    }

    public Task<IReadOnlyList<ProviderScore>> ScoreProvidersAsync(
        RoutingContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ScoreProviders(context));
    }

    private IReadOnlyList<ProviderScore> ScoreProviders(RoutingContext context)
    {
        var scores = new List<ProviderScore>();

        foreach (var provider in Enum.GetValues<Provider>())
        {
            var isAvailable = context.AvailableProviders.Contains(provider);
            var characteristics = GetCharacteristics(provider);

            // If voice is specific to a provider, only that provider can serve it
            var canServeVoice = context.VoiceProvider is null || context.VoiceProvider == provider;

            var score = isAvailable && canServeVoice
                ? CalculateScore(context.Preference, characteristics, context.PreferredProvider == provider)
                : 0;

            scores.Add(new ProviderScore
            {
                Provider = provider,
                Score = score,
                IsAvailable = isAvailable && canServeVoice,
                CostPerThousandChars = characteristics.CostPerK,
                AvgLatencyMs = characteristics.AvgLatencyMs,
                QualityRating = characteristics.QualityRating
            });
        }

        return scores.OrderByDescending(s => s.Score).ToList();
    }

    private double CalculateScore(
        RoutingPreference preference,
        ProviderCharacteristics characteristics,
        bool isPreferred)
    {
        // Normalize metrics to 0-1 scale
        var costScore = 1 - NormalizeCost(characteristics.CostPerK);
        var speedScore = 1 - NormalizeLatency(characteristics.AvgLatencyMs);
        var qualityScore = characteristics.QualityRating;

        // Apply weights based on preference
        var (costWeight, speedWeight, qualityWeight) = preference switch
        {
            RoutingPreference.Cost => (0.7, 0.1, 0.2),
            RoutingPreference.Speed => (0.1, 0.7, 0.2),
            RoutingPreference.Quality => (0.1, 0.2, 0.7),
            RoutingPreference.Balanced => (0.33, 0.33, 0.34),
            _ => (0.33, 0.33, 0.34)
        };

        var score = (costScore * costWeight) + (speedScore * speedWeight) + (qualityScore * qualityWeight);

        // Bonus for preferred provider
        if (isPreferred)
            score += _options.PreferredProviderBonus;

        return Math.Min(score, 1.0);
    }

    private static double NormalizeCost(decimal cost)
    {
        // Normalize cost where $0.30/1K is 1.0 and $0.004/1K is ~0
        const decimal maxCost = 0.35m;
        return (double)Math.Min(cost / maxCost, 1m);
    }

    private static double NormalizeLatency(int latencyMs)
    {
        // Normalize latency where 1000ms is 1.0 and 0ms is 0
        const int maxLatency = 1000;
        return Math.Min((double)latencyMs / maxLatency, 1.0);
    }

    private ProviderCharacteristics GetCharacteristics(Provider provider)
    {
        if (_options.ProviderOverrides.TryGetValue(provider, out var overrides))
            return overrides;

        return DefaultCharacteristics.GetValueOrDefault(provider,
            new ProviderCharacteristics(0.10m, 500, 0.75));
    }

    private static string GetRoutingReason(RoutingPreference preference, ProviderScore score)
    {
        return preference switch
        {
            RoutingPreference.Cost =>
                $"Lowest cost at ${score.CostPerThousandChars:F3}/1K chars",
            RoutingPreference.Speed =>
                $"Fastest response at ~{score.AvgLatencyMs}ms average latency",
            RoutingPreference.Quality =>
                $"Highest quality with {score.QualityRating:P0} rating",
            RoutingPreference.Balanced =>
                $"Best balance of cost (${score.CostPerThousandChars:F3}/1K), speed ({score.AvgLatencyMs}ms), quality ({score.QualityRating:P0})",
            _ => "Default routing selection"
        };
    }
}

public record ProviderCharacteristics(decimal CostPerK, int AvgLatencyMs, double QualityRating);

public class RoutingOptions
{
    public const string SectionName = "Routing";

    public double PreferredProviderBonus { get; set; } = 0.1;
    public Dictionary<Provider, ProviderCharacteristics> ProviderOverrides { get; set; } = new();
}
