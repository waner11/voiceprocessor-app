using System.Diagnostics;
using System.Globalization;
using FFMpegCore;
using Microsoft.Extensions.Logging;
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
        var tempDir = Path.Combine(Path.GetTempPath(), $"vp-merge-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var bitRate = options.BitRate ?? 128;
            var chunkFileNames = new List<string>(audioChunks.Count);
            for (var i = 0; i < audioChunks.Count; i++)
            {
                var chunkFileName = $"chunk_{i}.mp3";
                File.WriteAllBytes(Path.Combine(tempDir, chunkFileName), audioChunks[i]);
                chunkFileNames.Add(chunkFileName);
            }

            var mergeSequence = new List<string>();
            if (options.SilenceBetweenChunksMs > 0 && chunkFileNames.Count > 1)
            {
                var firstChunkPath = Path.Combine(tempDir, chunkFileNames[0]);
                var probe = FFProbe.Analyse(firstChunkPath);
                var sampleRate = probe.PrimaryAudioStream?.SampleRateHz ?? 24000;
                var channelLayout = probe.PrimaryAudioStream?.ChannelLayout;
                if (string.IsNullOrWhiteSpace(channelLayout))
                {
                    channelLayout = probe.PrimaryAudioStream?.Channels == 2 ? "stereo" : "mono";
                }

                var silenceFileName = "silence.mp3";
                var silenceDuration = (options.SilenceBetweenChunksMs / 1000d)
                    .ToString("0.###", CultureInfo.InvariantCulture);

                RunFfmpeg(
                    tempDir,
                    "-y",
                    "-f", "lavfi",
                    "-i", $"anullsrc=r={sampleRate}:cl={channelLayout}",
                    "-t", silenceDuration,
                    "-codec:a", "libmp3lame",
                    "-b:a", $"{bitRate}k",
                    silenceFileName);

                for (var i = 0; i < chunkFileNames.Count; i++)
                {
                    mergeSequence.Add(chunkFileNames[i]);
                    if (i < chunkFileNames.Count - 1)
                    {
                        mergeSequence.Add(silenceFileName);
                    }
                }
            }
            else
            {
                mergeSequence.AddRange(chunkFileNames);
            }

            var listFilePath = Path.Combine(tempDir, "list.txt");
            var concatLines = mergeSequence.Select(fileName => $"file '{fileName.Replace("'", "''")}'");
            File.WriteAllLines(listFilePath, concatLines);

            if (!TryRunFfmpeg(
                    tempDir,
                    out var concatCopyError,
                    "-y",
                    "-f", "concat",
                    "-safe", "0",
                    "-i", "list.txt",
                    "-c", "copy",
                    "output.mp3"))
            {
                var fallbackSucceeded = TryRunFfmpeg(
                    tempDir,
                    out var concatReencodeError,
                    "-y",
                    "-f", "concat",
                    "-safe", "0",
                    "-i", "list.txt",
                    "-codec:a", "libmp3lame",
                    "-b:a", $"{bitRate}k",
                    "output.mp3");

                if (!fallbackSucceeded)
                {
                    throw new InvalidOperationException(
                        $"FFmpeg concat failed. Copy mode error: {concatCopyError}. Re-encode fallback error: {concatReencodeError}");
                }
            }

            var outputPath = Path.Combine(tempDir, "output.mp3");
            var outputData = File.ReadAllBytes(outputPath);
            var outputProbe = FFProbe.Analyse(outputPath);
            var totalDurationMs = (int)outputProbe.Duration.TotalMilliseconds;

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
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    private static async Task<int> GetAudioDurationAsync(byte[] audioData, CancellationToken cancellationToken)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"vp-merge-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var inputPath = Path.Combine(tempDir, "input.mp3");
            await File.WriteAllBytesAsync(inputPath, audioData, cancellationToken);
            var probe = await FFProbe.AnalyseAsync(inputPath);
            return (int)probe.Duration.TotalMilliseconds;
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    private static void RunFfmpeg(string workingDirectory, params string[] args)
    {
        if (!TryRunFfmpeg(workingDirectory, out var errorOutput, args))
        {
            throw new InvalidOperationException($"FFmpeg process failed: {errorOutput}");
        }
    }

    private static bool TryRunFfmpeg(string workingDirectory, out string errorOutput, params string[] args)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            WorkingDirectory = workingDirectory,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in args)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        process.Start();
        var standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();
        errorOutput = standardError;
        return process.ExitCode == 0;
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
