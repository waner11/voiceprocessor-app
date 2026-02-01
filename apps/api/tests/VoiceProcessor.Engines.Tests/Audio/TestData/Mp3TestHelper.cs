using System.Reflection;

namespace VoiceProcessor.Engines.Tests.Audio.TestData;

public static class Mp3TestHelper
{
    public static byte[] CreateMinimalMp3(int durationMs, int sampleRate = 16000, int bitRate = 32)
    {
        return LoadEmbeddedMp3("short_chunk_1s.mp3");
    }
    
    public static byte[] CreateShortMp3Chunk() => LoadEmbeddedMp3("short_chunk_1s.mp3");
    
    public static bool IsValidMp3(byte[] mp3Data)
    {
        if (mp3Data == null || mp3Data.Length < 4)
        {
            return false;
        }
        
        return (mp3Data[0] == 0xFF && (mp3Data[1] & 0xE0) == 0xE0);
    }
    
    private static byte[] LoadEmbeddedMp3(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fullResourceName = $"VoiceProcessor.Engines.Tests.Audio.TestData.{resourceName}";
        
        using var stream = assembly.GetManifestResourceStream(fullResourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource not found: {fullResourceName}");
        }
        
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}
