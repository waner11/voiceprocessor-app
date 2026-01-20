using Microsoft.Extensions.Logging;
using NAudio.Lame;
using NAudio.Wave;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines.Audio;

public class AudioMergeEngine : IAudioMergeEngine
{
    private readonly ILogger<AudioMergeEngine> _logger;

    public AudioMergeEngine(ILogger<AudioMergeEngine> logger)
    {
        _logger = logger;
    }

    public async Task<AudioMergeResult> MergeAudioChunksAsync(
        IReadOnlyList<byte[]> audioChunks,
        AudioMergeOptions options,
        CancellationToken cancellationToken = default)
    {
        if (audioChunks.Count == 0)
        {
            throw new ArgumentException("No audio chunks provided", nameof(audioChunks));
        }

        _logger.LogInformation("Merging {ChunkCount} audio chunks to {Format}",
            audioChunks.Count, options.OutputFormat);

        if (audioChunks.Count == 1)
        {
            var singleChunkDuration = await GetAudioDurationAsync(audioChunks[0], cancellationToken);
            return new AudioMergeResult
            {
                AudioData = audioChunks[0],
                ContentType = GetContentType(options.OutputFormat),
                DurationMs = singleChunkDuration,
                SizeBytes = audioChunks[0].Length
            };
        }

        return await Task.Run(() => MergeChunksInternal(audioChunks, options), cancellationToken);
    }

    private AudioMergeResult MergeChunksInternal(IReadOnlyList<byte[]> audioChunks, AudioMergeOptions options)
    {
        // Decode all MP3 chunks to PCM
        var pcmChunks = new List<byte[]>();
        WaveFormat? targetFormat = null;
        var totalDurationMs = 0;

        foreach (var chunk in audioChunks)
        {
            using var ms = new MemoryStream(chunk);
            using var mp3Reader = new Mp3FileReader(ms);

            // Use the format of the first chunk as target
            targetFormat ??= mp3Reader.WaveFormat;

            // Read all PCM data
            var pcmData = new byte[mp3Reader.Length];
            var bytesRead = mp3Reader.Read(pcmData, 0, pcmData.Length);
            if (bytesRead < pcmData.Length)
            {
                Array.Resize(ref pcmData, bytesRead);
            }

            pcmChunks.Add(pcmData);
            totalDurationMs += (int)mp3Reader.TotalTime.TotalMilliseconds;
        }

        if (targetFormat is null)
        {
            throw new InvalidOperationException("Could not determine audio format from chunks");
        }

        // Generate silence samples if needed
        byte[]? silenceSamples = null;
        if (options.SilenceBetweenChunksMs > 0)
        {
            var silenceByteCount = (int)(targetFormat.AverageBytesPerSecond * options.SilenceBetweenChunksMs / 1000.0);
            silenceSamples = new byte[silenceByteCount];
            totalDurationMs += options.SilenceBetweenChunksMs * (audioChunks.Count - 1);
        }

        // Calculate total size
        var totalPcmSize = pcmChunks.Sum(c => c.Length);
        if (silenceSamples is not null)
        {
            totalPcmSize += silenceSamples.Length * (pcmChunks.Count - 1);
        }

        // Merge PCM chunks
        var mergedPcm = new byte[totalPcmSize];
        var offset = 0;
        for (var i = 0; i < pcmChunks.Count; i++)
        {
            Array.Copy(pcmChunks[i], 0, mergedPcm, offset, pcmChunks[i].Length);
            offset += pcmChunks[i].Length;

            if (silenceSamples is not null && i < pcmChunks.Count - 1)
            {
                Array.Copy(silenceSamples, 0, mergedPcm, offset, silenceSamples.Length);
                offset += silenceSamples.Length;
            }
        }

        // Encode back to MP3
        byte[] outputData;
        using (var pcmStream = new RawSourceWaveStream(new MemoryStream(mergedPcm), targetFormat))
        using (var outputStream = new MemoryStream())
        {
            var bitRate = options.BitRate ?? 128;
            using (var mp3Writer = new LameMP3FileWriter(outputStream, targetFormat, bitRate))
            {
                pcmStream.CopyTo(mp3Writer);
            }
            outputData = outputStream.ToArray();
        }

        _logger.LogInformation(
            "Merged audio: {Duration}ms, {Size} bytes",
            totalDurationMs, outputData.Length);

        return new AudioMergeResult
        {
            AudioData = outputData,
            ContentType = GetContentType(options.OutputFormat),
            DurationMs = totalDurationMs,
            SizeBytes = outputData.Length
        };
    }

    private static async Task<int> GetAudioDurationAsync(byte[] audioData, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            using var ms = new MemoryStream(audioData);
            using var mp3Reader = new Mp3FileReader(ms);
            return (int)mp3Reader.TotalTime.TotalMilliseconds;
        }, cancellationToken);
    }

    private static string GetContentType(string format) => format.ToLowerInvariant() switch
    {
        "mp3" => "audio/mpeg",
        "wav" => "audio/wav",
        "ogg" => "audio/ogg",
        "flac" => "audio/flac",
        _ => "audio/mpeg"
    };
}
