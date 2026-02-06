using Microsoft.Extensions.Logging;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines.Presets;

public class VoicePresetEngine : IVoicePresetEngine
{
    private readonly ILogger<VoicePresetEngine> _logger;

    public VoicePresetEngine(ILogger<VoicePresetEngine> logger)
    {
        _logger = logger;
    }

    public VoicePresetSettings GetSettingsForProvider(VoicePreset preset, Provider provider)
    {
        return provider switch
        {
            Provider.ElevenLabs => GetElevenLabsSettings(preset),
            Provider.OpenAI => GetOpenAiSettings(preset),
            _ => GetDefaultSettings(provider)
        };
    }

    private VoicePresetSettings GetElevenLabsSettings(VoicePreset preset)
    {
        var settings = preset switch
        {
            VoicePreset.Audiobook => new VoicePresetSettings
            {
                Stability = 0.70,
                SimilarityBoost = 0.80,
                Style = 0.0,
                Speed = 1.0
            },
            VoicePreset.Conversational => new VoicePresetSettings
            {
                Stability = 0.50,
                SimilarityBoost = 0.75,
                Style = 0.2,
                Speed = 1.05
            },
            VoicePreset.Dramatic => new VoicePresetSettings
            {
                Stability = 0.35,
                SimilarityBoost = 0.70,
                Style = 0.5,
                Speed = 0.95
            },
            VoicePreset.Professional => new VoicePresetSettings
            {
                Stability = 0.80,
                SimilarityBoost = 0.85,
                Style = 0.0,
                Speed = 1.0
            },
            _ => null
        };

        if (settings is null)
        {
            _logger.LogWarning("Unknown VoicePreset {Preset} for ElevenLabs, using defaults", preset);
            return new VoicePresetSettings { Speed = 1.0 };
        }

        return settings;
    }

    private VoicePresetSettings GetOpenAiSettings(VoicePreset preset)
    {
        var settings = preset switch
        {
            VoicePreset.Audiobook => new VoicePresetSettings { Speed = 1.0 },
            VoicePreset.Conversational => new VoicePresetSettings { Speed = 1.05 },
            VoicePreset.Dramatic => new VoicePresetSettings { Speed = 0.95 },
            VoicePreset.Professional => new VoicePresetSettings { Speed = 1.0 },
            _ => null
        };

        if (settings is null)
        {
            _logger.LogWarning("Unknown VoicePreset {Preset} for OpenAI, using defaults", preset);
            return new VoicePresetSettings { Speed = 1.0 };
        }

        return settings;
    }

    private VoicePresetSettings GetDefaultSettings(Provider provider)
    {
        _logger.LogWarning("Unsupported provider {Provider} for voice presets, using defaults", provider);
        return new VoicePresetSettings { Speed = 1.0 };
    }
}
