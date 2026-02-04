using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines.Presets;

public class VoicePresetEngine : IVoicePresetEngine
{
    public VoicePresetSettings GetSettingsForProvider(VoicePreset preset, Provider provider)
    {
        return provider switch
        {
            Provider.ElevenLabs => GetElevenLabsSettings(preset),
            Provider.OpenAI => GetOpenAiSettings(preset),
            _ => GetDefaultSettings()
        };
    }

    private static VoicePresetSettings GetElevenLabsSettings(VoicePreset preset)
    {
        return preset switch
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
            _ => new VoicePresetSettings { Speed = 1.0 }
        };
    }

    private static VoicePresetSettings GetOpenAiSettings(VoicePreset preset)
    {
        return preset switch
        {
            VoicePreset.Audiobook => new VoicePresetSettings { Speed = 1.0 },
            VoicePreset.Conversational => new VoicePresetSettings { Speed = 1.05 },
            VoicePreset.Dramatic => new VoicePresetSettings { Speed = 0.95 },
            VoicePreset.Professional => new VoicePresetSettings { Speed = 1.0 },
            _ => new VoicePresetSettings { Speed = 1.0 }
        };
    }

    private static VoicePresetSettings GetDefaultSettings()
    {
        return new VoicePresetSettings { Speed = 1.0 };
    }
}
