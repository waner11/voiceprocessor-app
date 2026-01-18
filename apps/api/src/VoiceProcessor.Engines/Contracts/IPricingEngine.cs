using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Engines.Contracts;

public interface IPricingEngine
{
    PriceEstimate CalculateEstimate(PricingContext context);

    IReadOnlyList<ProviderPriceEstimate> CalculateAllProviderEstimates(PricingContext context);

    int CalculateCreditsRequired(decimal cost);
}

public record PricingContext
{
    public required int CharacterCount { get; init; }
    public Provider? Provider { get; init; }
    public Guid? VoiceId { get; init; }
    public decimal? VoiceCostPerThousandChars { get; init; }
}

public record PriceEstimate
{
    public required int CharacterCount { get; init; }
    public required decimal EstimatedCost { get; init; }
    public required string Currency { get; init; }
    public Provider? Provider { get; init; }
    public int CreditsRequired { get; init; }
}

public record ProviderPriceEstimate
{
    public required Provider Provider { get; init; }
    public required decimal CostPerThousandChars { get; init; }
    public required decimal TotalCost { get; init; }
    public required string Currency { get; init; }
    public int CreditsRequired { get; init; }
}
