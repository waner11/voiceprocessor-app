using Microsoft.Extensions.Options;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines.Pricing;

public class PricingEngine : IPricingEngine
{
    private readonly PricingOptions _options;

    // Default provider rates (cost per 1000 characters in USD)
    private static readonly Dictionary<Provider, decimal> DefaultProviderRates = new()
    {
        [Provider.ElevenLabs] = 0.30m,
        [Provider.OpenAI] = 0.015m,
        [Provider.GoogleCloud] = 0.016m,
        [Provider.AmazonPolly] = 0.004m,
        [Provider.FishAudio] = 0.20m,
        [Provider.Cartesia] = 0.25m,
        [Provider.Deepgram] = 0.015m
    };

    public PricingEngine(IOptions<PricingOptions> options)
    {
        _options = options.Value;
    }

    public PriceEstimate CalculateEstimate(PricingContext context)
    {
        var costPerThousand = GetCostPerThousandChars(context);
        var estimatedCost = CalculateCost(context.CharacterCount, costPerThousand);
        var creditsRequired = CalculateCreditsRequired(estimatedCost);

        return new PriceEstimate
        {
            CharacterCount = context.CharacterCount,
            EstimatedCost = estimatedCost,
            Currency = _options.Currency,
            Provider = context.Provider,
            CreditsRequired = creditsRequired
        };
    }

    public IReadOnlyList<ProviderPriceEstimate> CalculateAllProviderEstimates(PricingContext context)
    {
        var estimates = new List<ProviderPriceEstimate>();

        foreach (var provider in Enum.GetValues<Provider>())
        {
            var costPerThousand = GetProviderRate(provider);
            var totalCost = CalculateCost(context.CharacterCount, costPerThousand);

            estimates.Add(new ProviderPriceEstimate
            {
                Provider = provider,
                CostPerThousandChars = costPerThousand,
                TotalCost = totalCost,
                Currency = _options.Currency,
                CreditsRequired = CalculateCreditsRequired(totalCost)
            });
        }

        return estimates.OrderBy(e => e.TotalCost).ToList();
    }

    public int CalculateCreditsRequired(decimal cost)
    {
        if (cost <= 0)
            return 0;

        // Convert USD cost to credits (e.g., $0.01 = 1 credit)
        var credits = cost / _options.CostPerCredit;
        return (int)Math.Ceiling(credits);
    }

    private decimal GetCostPerThousandChars(PricingContext context)
    {
        // Use voice-specific cost if provided
        if (context.VoiceCostPerThousandChars.HasValue)
            return context.VoiceCostPerThousandChars.Value;

        // Use provider rate if specified
        if (context.Provider.HasValue)
            return GetProviderRate(context.Provider.Value);

        // Default to cheapest provider rate
        return DefaultProviderRates.Values.Min();
    }

    private decimal GetProviderRate(Provider provider)
    {
        // Check configured overrides first
        if (_options.ProviderRateOverrides.TryGetValue(provider, out var overrideRate))
            return overrideRate;

        // Fall back to defaults
        return DefaultProviderRates.GetValueOrDefault(provider, 0.10m);
    }

    private static decimal CalculateCost(int characterCount, decimal costPerThousand)
    {
        return characterCount * costPerThousand / 1000m;
    }
}

public class PricingOptions
{
    public const string SectionName = "Pricing";

    public string Currency { get; set; } = "USD";
    public decimal CostPerCredit { get; set; } = 0.01m;
    public Dictionary<Provider, decimal> ProviderRateOverrides { get; set; } = [];
}
