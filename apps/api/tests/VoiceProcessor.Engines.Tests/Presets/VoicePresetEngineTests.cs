using FluentAssertions;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Engines.Contracts;
using VoiceProcessor.Engines.Presets;

namespace VoiceProcessor.Engines.Tests.Presets;

public class VoicePresetEngineTests
{
    private readonly IVoicePresetEngine _engine;

    public VoicePresetEngineTests()
    {
        _engine = new VoicePresetEngine();
    }

    [Fact]
    public void GetSettingsForProvider_ElevenLabs_Audiobook_ReturnsCorrectValues()
    {
        // Act
        var settings = _engine.GetSettingsForProvider(VoicePreset.Audiobook, Provider.ElevenLabs);

        // Assert
        settings.Stability.Should().Be(0.70);
        settings.SimilarityBoost.Should().Be(0.80);
        settings.Style.Should().Be(0.0);
        settings.Speed.Should().Be(1.0);
    }

    [Fact]
    public void GetSettingsForProvider_ElevenLabs_Dramatic_ReturnsCorrectValues()
    {
        // Act
        var settings = _engine.GetSettingsForProvider(VoicePreset.Dramatic, Provider.ElevenLabs);

        // Assert
        settings.Stability.Should().Be(0.35);
        settings.SimilarityBoost.Should().Be(0.70);
        settings.Style.Should().Be(0.5);
        settings.Speed.Should().Be(0.95);
    }

    [Fact]
    public void GetSettingsForProvider_OpenAI_Conversational_ReturnsCorrectSpeed()
    {
        // Act
        var settings = _engine.GetSettingsForProvider(VoicePreset.Conversational, Provider.OpenAI);

        // Assert
        settings.Speed.Should().Be(1.05);
        settings.Stability.Should().BeNull();
        settings.SimilarityBoost.Should().BeNull();
        settings.Style.Should().BeNull();
    }

    [Fact]
    public void GetSettingsForProvider_UnknownProvider_ReturnsDefaults()
    {
        // Act
        var settings = _engine.GetSettingsForProvider(VoicePreset.Professional, Provider.GoogleCloud);

        // Assert
        settings.Speed.Should().Be(1.0);
        settings.Stability.Should().BeNull();
        settings.SimilarityBoost.Should().BeNull();
        settings.Style.Should().BeNull();
    }

    [Fact]
    public void GetSettingsForProvider_ElevenLabs_Conversational_ReturnsCorrectValues()
    {
        // Act
        var settings = _engine.GetSettingsForProvider(VoicePreset.Conversational, Provider.ElevenLabs);

        // Assert
        settings.Stability.Should().Be(0.50);
        settings.SimilarityBoost.Should().Be(0.75);
        settings.Style.Should().Be(0.2);
        settings.Speed.Should().Be(1.05);
    }

    [Fact]
    public void GetSettingsForProvider_ElevenLabs_Professional_ReturnsCorrectValues()
    {
        // Act
        var settings = _engine.GetSettingsForProvider(VoicePreset.Professional, Provider.ElevenLabs);

        // Assert
        settings.Stability.Should().Be(0.80);
        settings.SimilarityBoost.Should().Be(0.85);
        settings.Style.Should().Be(0.0);
        settings.Speed.Should().Be(1.0);
    }

    [Fact]
    public void GetSettingsForProvider_OpenAI_Audiobook_ReturnsCorrectSpeed()
    {
        // Act
        var settings = _engine.GetSettingsForProvider(VoicePreset.Audiobook, Provider.OpenAI);

        // Assert
        settings.Speed.Should().Be(1.0);
        settings.Stability.Should().BeNull();
        settings.SimilarityBoost.Should().BeNull();
        settings.Style.Should().BeNull();
    }
}
