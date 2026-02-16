using System.Diagnostics;
using System.Globalization;

namespace VoiceProcessor.Engines.Tests.Audio.TestData;

public static class Mp3TestHelper
{
    public static byte[] CreateMinimalMp3(int durationMs, int sampleRate = 16000, int bitRate = 32)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"vp-mp3-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var outputFile = Path.Combine(tempDir, "sample.mp3");
            var durationSeconds = (durationMs / 1000d).ToString("0.###", CultureInfo.InvariantCulture);

            var process = new Process
            {
                StartInfo =
                {
                    FileName = "ffmpeg",
                    WorkingDirectory = tempDir,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.StartInfo.ArgumentList.Add("-y");
            process.StartInfo.ArgumentList.Add("-f");
            process.StartInfo.ArgumentList.Add("lavfi");
            process.StartInfo.ArgumentList.Add("-i");
            process.StartInfo.ArgumentList.Add($"anullsrc=r={sampleRate}:cl=mono");
            process.StartInfo.ArgumentList.Add("-t");
            process.StartInfo.ArgumentList.Add(durationSeconds);
            process.StartInfo.ArgumentList.Add("-codec:a");
            process.StartInfo.ArgumentList.Add("libmp3lame");
            process.StartInfo.ArgumentList.Add("-b:a");
            process.StartInfo.ArgumentList.Add($"{bitRate}k");
            process.StartInfo.ArgumentList.Add(outputFile);

            process.Start();
            var errorOutput = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"ffmpeg failed to generate MP3 fixture: {errorOutput}");
            }

            return File.ReadAllBytes(outputFile);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    public static byte[] CreateShortMp3Chunk() => CreateMinimalMp3(1000);
    
    public static bool IsValidMp3(byte[] mp3Data)
    {
        if (mp3Data == null || mp3Data.Length < 4)
        {
            return false;
        }

        var offset = 0;
        if (mp3Data.Length >= 10 && mp3Data[0] == 0x49 && mp3Data[1] == 0x44 && mp3Data[2] == 0x33)
        {
            var tagSize = ((mp3Data[6] & 0x7F) << 21)
                        | ((mp3Data[7] & 0x7F) << 14)
                        | ((mp3Data[8] & 0x7F) << 7)
                        | (mp3Data[9] & 0x7F);
            offset = 10 + tagSize;
        }

        if (offset + 1 >= mp3Data.Length)
        {
            return false;
        }

        return mp3Data[offset] == 0xFF && (mp3Data[offset + 1] & 0xE0) == 0xE0;
    }
}
