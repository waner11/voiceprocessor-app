namespace VoiceProcessor.Engines.Contracts;

public interface IAudioMergeEngine
{
    Task<AudioMergeResult> MergeAudioChunksAsync(
        IReadOnlyList<byte[]> audioChunks,
        AudioMergeOptions options,
        CancellationToken cancellationToken = default);
}

public record AudioMergeOptions
{
    public string OutputFormat { get; init; } = "mp3";
    public int? SampleRate { get; init; }
    public int? BitRate { get; init; }
    public int? Channels { get; init; }
    public bool NormalizeVolume { get; init; } = false;
    public int SilenceBetweenChunksMs { get; init; } = 0;
}

public record AudioMergeResult
{
    public required byte[] AudioData { get; init; }
    public required string ContentType { get; init; }
    public required int DurationMs { get; init; }
    public required long SizeBytes { get; init; }
}
