using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Engines.Contracts;

public interface IVoicePresetEngine
{
    VoicePresetSettings GetSettingsForProvider(VoicePreset preset, Provider provider);
}

public record VoicePresetSettings
{
    public double? Stability { get; init; }
    public double? SimilarityBoost { get; init; }
    public double? Style { get; init; }
    public double Speed { get; init; } = 1.0;
}
