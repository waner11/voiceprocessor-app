using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Engines.Contracts;
using VoiceProcessor.Engines.Presets;

namespace VoiceProcessor.Engines.Tests.Presets;

public class VoicePresetEngineTests
{
    private readonly Mock<ILogger<VoicePresetEngine>> _loggerMock;
    private readonly IVoicePresetEngine _engine;

    public VoicePresetEngineTests()
    {
        _loggerMock = new Mock<ILogger<VoicePresetEngine>>();
        _engine = new VoicePresetEngine(_loggerMock.Object);
    }

    [Fact]
    public void GetSettingsForProvider_ElevenLabs_Audiobook_ReturnsCorrectValues()
    {
        var settings = _engine.GetSettingsForProvider(VoicePreset.Audiobook, Provider.ElevenLabs);

        settings.Stability.Should().Be(0.70);
        settings.SimilarityBoost.Should().Be(0.80);
        settings.Style.Should().Be(0.0);
        settings.Speed.Should().Be(1.0);
    }

    [Fact]
    public void GetSettingsForProvider_ElevenLabs_Dramatic_ReturnsCorrectValues()
    {
        var settings = _engine.GetSettingsForProvider(VoicePreset.Dramatic, Provider.ElevenLabs);

        settings.Stability.Should().Be(0.35);
        settings.SimilarityBoost.Should().Be(0.70);
        settings.Style.Should().Be(0.5);
        settings.Speed.Should().Be(0.95);
    }

    [Fact]
    public void GetSettingsForProvider_OpenAI_Conversational_ReturnsCorrectSpeed()
    {
        var settings = _engine.GetSettingsForProvider(VoicePreset.Conversational, Provider.OpenAI);

        settings.Speed.Should().Be(1.05);
        settings.Stability.Should().BeNull();
        settings.SimilarityBoost.Should().BeNull();
        settings.Style.Should().BeNull();
    }

    [Fact]
    public void GetSettingsForProvider_UnknownProvider_ReturnsDefaults()
    {
        var settings = _engine.GetSettingsForProvider(VoicePreset.Professional, Provider.GoogleCloud);

        settings.Speed.Should().Be(1.0);
        settings.Stability.Should().BeNull();
        settings.SimilarityBoost.Should().BeNull();
        settings.Style.Should().BeNull();
    }

    [Fact]
    public void GetSettingsForProvider_ElevenLabs_Conversational_ReturnsCorrectValues()
    {
        var settings = _engine.GetSettingsForProvider(VoicePreset.Conversational, Provider.ElevenLabs);

        settings.Stability.Should().Be(0.50);
        settings.SimilarityBoost.Should().Be(0.75);
        settings.Style.Should().Be(0.2);
        settings.Speed.Should().Be(1.05);
    }

    [Fact]
    public void GetSettingsForProvider_ElevenLabs_Professional_ReturnsCorrectValues()
    {
        var settings = _engine.GetSettingsForProvider(VoicePreset.Professional, Provider.ElevenLabs);

        settings.Stability.Should().Be(0.80);
        settings.SimilarityBoost.Should().Be(0.85);
        settings.Style.Should().Be(0.0);
        settings.Speed.Should().Be(1.0);
    }

    [Fact]
    public void GetSettingsForProvider_OpenAI_Audiobook_ReturnsCorrectSpeed()
    {
        var settings = _engine.GetSettingsForProvider(VoicePreset.Audiobook, Provider.OpenAI);

        settings.Speed.Should().Be(1.0);
        settings.Stability.Should().BeNull();
        settings.SimilarityBoost.Should().BeNull();
        settings.Style.Should().BeNull();
    }

    [Fact]
    public void GetSettingsForProvider_UnknownPreset_ElevenLabs_LogsWarning()
    {
        var unknownPreset = (VoicePreset)999;

        var settings = _engine.GetSettingsForProvider(unknownPreset, Provider.ElevenLabs);

        settings.Speed.Should().Be(1.0);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unknown VoicePreset") && v.ToString().Contains("ElevenLabs")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetSettingsForProvider_UnknownPreset_OpenAI_LogsWarning()
    {
        var unknownPreset = (VoicePreset)999;

        var settings = _engine.GetSettingsForProvider(unknownPreset, Provider.OpenAI);

        settings.Speed.Should().Be(1.0);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unknown VoicePreset") && v.ToString().Contains("OpenAI")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetSettingsForProvider_UnsupportedProvider_LogsWarning()
    {
        var unsupportedProvider = (Provider)999;

        var settings = _engine.GetSettingsForProvider(VoicePreset.Professional, unsupportedProvider);

        settings.Speed.Should().Be(1.0);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unsupported provider")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
