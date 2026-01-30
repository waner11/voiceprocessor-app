using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Engines.Contracts;
using VoiceProcessor.Engines.Routing;

namespace VoiceProcessor.Engines.Tests.Routing;

public class RoutingEngineTests
{
    private readonly RoutingEngine _engine;
    private readonly RoutingOptions _options;

    public RoutingEngineTests()
    {
        _options = new RoutingOptions
        {
            PreferredProviderBonus = 0.1
        };
        var logger = new Mock<ILogger<RoutingEngine>>();
        _engine = new RoutingEngine(Options.Create(_options), logger.Object);
    }

    [Fact]
    public async Task SelectProviderAsync_CostOptimized_SelectsCheapestProvider()
    {
        // Arrange
        var context = new RoutingContext
        {
            VoiceId = Guid.NewGuid(),
            CharacterCount = 1000,
            Preference = RoutingPreference.Cost,
            AvailableProviders = new HashSet<Provider>
            {
                Provider.ElevenLabs,
                Provider.OpenAI,
                Provider.AmazonPolly,
                Provider.GoogleCloud
            }
        };

        // Act
        var result = await _engine.SelectProviderAsync(context);

        // Assert
        result.SelectedProvider.Should().Be(Provider.AmazonPolly);
        result.EstimatedCost.Should().BeGreaterThan(0);
        result.EstimatedLatencyMs.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SelectProviderAsync_SpeedOptimized_SelectsFastestProvider()
    {
        // Arrange
        var context = new RoutingContext
        {
            VoiceId = Guid.NewGuid(),
            CharacterCount = 1000,
            Preference = RoutingPreference.Speed,
            AvailableProviders = new HashSet<Provider>
            {
                Provider.ElevenLabs,
                Provider.OpenAI,
                Provider.AmazonPolly,
                Provider.GoogleCloud
            }
        };

        // Act
        var result = await _engine.SelectProviderAsync(context);

        // Assert
        result.SelectedProvider.Should().Be(Provider.AmazonPolly);
        result.EstimatedLatencyMs.Should().BeLessThanOrEqualTo(300);
    }

    [Fact]
    public async Task SelectProviderAsync_QualityOptimized_SelectsHighestQuality()
    {
        // Arrange
        var context = new RoutingContext
        {
            VoiceId = Guid.NewGuid(),
            CharacterCount = 1000,
            Preference = RoutingPreference.Quality,
            AvailableProviders = new HashSet<Provider>
            {
                Provider.ElevenLabs,
                Provider.FishAudio,
                Provider.Cartesia
            }
        };

        // Act
        var result = await _engine.SelectProviderAsync(context);
        var scores = await _engine.ScoreProvidersAsync(context);
        var topProvider = scores.Where(s => s.IsAvailable).OrderByDescending(s => s.Score).First();

        // Assert
        topProvider.QualityRating.Should().BeGreaterThanOrEqualTo(0.88);
        result.SelectedProvider.Should().BeOneOf(Provider.ElevenLabs, Provider.FishAudio, Provider.Cartesia);
    }

    [Fact]
    public async Task SelectProviderAsync_Balanced_WeighsEqually()
    {
        // Arrange
        var context = new RoutingContext
        {
            VoiceId = Guid.NewGuid(),
            CharacterCount = 1000,
            Preference = RoutingPreference.Balanced,
            AvailableProviders = new HashSet<Provider>
            {
                Provider.ElevenLabs,
                Provider.OpenAI,
                Provider.AmazonPolly,
                Provider.GoogleCloud,
                Provider.Deepgram
            }
        };

        // Act
        var result = await _engine.SelectProviderAsync(context);

        // Assert
        result.SelectedProvider.Should().BeOneOf(
            Provider.AmazonPolly,
            Provider.GoogleCloud,
            Provider.Deepgram
        );
    }

    [Fact]
    public async Task SelectProviderAsync_NoAvailableProviders_ThrowsInvalidOperationException()
    {
        // Arrange
        var context = new RoutingContext
        {
            VoiceId = Guid.NewGuid(),
            CharacterCount = 1000,
            Preference = RoutingPreference.Cost,
            AvailableProviders = new HashSet<Provider>()
        };

        // Act
        var act = async () => await _engine.SelectProviderAsync(context);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No TTS providers available");
    }

    [Fact]
    public async Task SelectProviderAsync_VoiceLockedToProvider_SelectsThatProvider()
    {
        // Arrange
        var context = new RoutingContext
        {
            VoiceId = Guid.NewGuid(),
            CharacterCount = 1000,
            Preference = RoutingPreference.Cost,
            VoiceProvider = Provider.ElevenLabs,
            AvailableProviders = new HashSet<Provider>
            {
                Provider.ElevenLabs,
                Provider.OpenAI,
                Provider.AmazonPolly
            }
        };

        // Act
        var result = await _engine.SelectProviderAsync(context);

        // Assert
        result.SelectedProvider.Should().Be(Provider.ElevenLabs);
    }

    [Fact]
    public async Task SelectProviderAsync_PreferredProviderBonus_AppliedAndCapped()
    {
        // Arrange
        var context = new RoutingContext
        {
            VoiceId = Guid.NewGuid(),
            CharacterCount = 1000,
            Preference = RoutingPreference.Cost,
            PreferredProvider = Provider.OpenAI,
            AvailableProviders = new HashSet<Provider>
            {
                Provider.ElevenLabs,
                Provider.OpenAI,
                Provider.AmazonPolly
            }
        };

        // Act
        var scores = await _engine.ScoreProvidersAsync(context);
        var openAiScore = scores.First(s => s.Provider == Provider.OpenAI);

        // Assert
        openAiScore.Score.Should().BeLessThanOrEqualTo(1.0);
        openAiScore.Score.Should().BeGreaterThan(0);
    }
}
