using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Contracts;

public interface ITtsProviderAccessor
{
    Provider Provider { get; }

    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    Task<TtsResult> GenerateSpeechAsync(
        TtsRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProviderVoice>> GetVoicesAsync(
        CancellationToken cancellationToken = default);
}

public record TtsRequest
{
    public required string Text { get; init; }
    public required string ProviderVoiceId { get; init; }
    public string OutputFormat { get; init; } = "mp3";
    public double? Speed { get; init; }
    public double? Pitch { get; init; }
    public VoicePreset? Preset { get; init; }
    public double? Stability { get; init; }
    public double? SimilarityBoost { get; init; }
    public double? Style { get; init; }
}

public record TtsResult
{
    public required bool Success { get; init; }
    public byte[]? AudioData { get; init; }
    public string? ContentType { get; init; }
    public int? DurationMs { get; init; }
    public string? ErrorMessage { get; init; }
    public int CharactersProcessed { get; init; }
    public decimal Cost { get; init; }
}

public record ProviderVoice
{
    public required string ProviderVoiceId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? Language { get; init; }
    public string? Accent { get; init; }
    public string? Gender { get; init; }
    public string? AgeGroup { get; init; }
    public string? UseCase { get; init; }
    public string? PreviewUrl { get; init; }
}
