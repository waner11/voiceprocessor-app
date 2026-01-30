using FluentAssertions;
using Microsoft.Extensions.Options;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Engines.Contracts;
using VoiceProcessor.Engines.Pricing;

namespace VoiceProcessor.Engines.Tests.Pricing;

public class PricingEngineTests
{
    private readonly PricingEngine _engine;
    private readonly PricingOptions _options;

    public PricingEngineTests()
    {
        _options = new PricingOptions
        {
            Currency = "USD",
            CostPerCredit = 0.01m
        };
        _engine = new PricingEngine(Options.Create(_options));
    }

    [Fact]
    public void CalculateEstimate_BasicInput_ReturnsCorrectCost()
    {
        // Arrange
        var context = new PricingContext
        {
            CharacterCount = 1000,
            Provider = Provider.OpenAI
        };

        // Act
        var result = _engine.CalculateEstimate(context);

        // Assert
        var expectedCost = 1000 * 0.015m / 1000m; // OpenAI rate is 0.015 per 1K chars
        result.EstimatedCost.Should().Be(expectedCost);
        result.CharacterCount.Should().Be(1000);
        result.Currency.Should().Be("USD");
        result.Provider.Should().Be(Provider.OpenAI);
    }

    [Fact]
    public void CalculateCreditsRequired_CalculatesCeilingCorrectly()
    {
        // Arrange
        var cost1 = 0.015m; // Should require 2 credits (0.015 / 0.01 = 1.5 -> ceiling = 2)
        var cost2 = 0.01m;  // Should require 1 credit (0.01 / 0.01 = 1.0 -> ceiling = 1)
        var cost3 = 0.025m; // Should require 3 credits (0.025 / 0.01 = 2.5 -> ceiling = 3)

        // Act
        var credits1 = _engine.CalculateCreditsRequired(cost1);
        var credits2 = _engine.CalculateCreditsRequired(cost2);
        var credits3 = _engine.CalculateCreditsRequired(cost3);

        // Assert
        credits1.Should().Be(2);
        credits2.Should().Be(1);
        credits3.Should().Be(3);
    }

    [Fact]
    public void CalculateEstimate_VoiceCostPerThousandChars_UsesVoiceRate()
    {
        // Arrange
        var customRate = 0.50m;
        var context = new PricingContext
        {
            CharacterCount = 2000,
            Provider = Provider.OpenAI,
            VoiceCostPerThousandChars = customRate
        };

        // Act
        var result = _engine.CalculateEstimate(context);

        // Assert
        var expectedCost = 2000 * customRate / 1000m;
        result.EstimatedCost.Should().Be(expectedCost);
    }

    [Fact]
    public void CalculateEstimate_UnknownProvider_FallsBackToDefault()
    {
        // Arrange - Create a custom options with provider override to test fallback
        var customOptions = new PricingOptions
        {
            Currency = "USD",
            CostPerCredit = 0.01m,
            ProviderRateOverrides = new Dictionary<Provider, decimal>()
        };
        var customEngine = new PricingEngine(Options.Create(customOptions));

        // Use a provider that's in the enum but might not have a default rate
        var context = new PricingContext
        {
            CharacterCount = 1000,
            Provider = Provider.Cartesia
        };

        // Act
        var result = customEngine.CalculateEstimate(context);

        // Assert
        result.EstimatedCost.Should().BeGreaterThan(0);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void CalculateAllProviderEstimates_SortsByCost()
    {
        // Arrange
        var context = new PricingContext
        {
            CharacterCount = 1000
        };

        // Act
        var result = _engine.CalculateAllProviderEstimates(context);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(7); // All 7 providers
        
        // Verify sorted by cost (ascending)
        for (int i = 0; i < result.Count - 1; i++)
        {
            result[i].TotalCost.Should().BeLessThanOrEqualTo(result[i + 1].TotalCost);
        }

        // AmazonPolly should be first (cheapest at 0.004)
        result[0].Provider.Should().Be(Provider.AmazonPolly);
    }

    [Fact]
    public void CalculateEstimate_ZeroCharacterCount_ReturnsZeroCost()
    {
        // Arrange
        var context = new PricingContext
        {
            CharacterCount = 0,
            Provider = Provider.OpenAI
        };

        // Act
        var result = _engine.CalculateEstimate(context);

        // Assert
        result.EstimatedCost.Should().Be(0);
        result.CreditsRequired.Should().Be(0);
    }
}
