# MP3 Test Fixtures

This directory contains test helpers and fixtures for MP3 audio testing.

## Files

- **Mp3TestHelper.cs** - Helper class for generating MP3 test data
- **Mp3TestHelperTests.cs** - Unit tests for the helper
- **short_chunk_1s.mp3** - Embedded MP3 fixture (~1 second, 3.8KB)

## Usage

```csharp
using VoiceProcessor.Engines.Tests.Audio.TestData;

// Get a short MP3 chunk for testing
var mp3Data = Mp3TestHelper.CreateShortMp3Chunk();

// Or use the parameterized version (currently returns same fixture)
var mp3Data = Mp3TestHelper.CreateMinimalMp3(1000);

// Validate MP3 data
bool isValid = Mp3TestHelper.IsValidMp3(mp3Data);
```

## For AudioMergeEngine Tests

```csharp
// Single chunk test
var chunk = Mp3TestHelper.CreateShortMp3Chunk();
var result = await audioMergeEngine.MergeAudioChunksAsync(
    new[] { chunk },
    options
);

// Multi-chunk test
var chunks = new[]
{
    Mp3TestHelper.CreateShortMp3Chunk(),
    Mp3TestHelper.CreateShortMp3Chunk(),
    Mp3TestHelper.CreateShortMp3Chunk()
};
var result = await audioMergeEngine.MergeAudioChunksAsync(chunks, options);
```

## Implementation Notes

- MP3 fixtures are embedded resources (no file I/O needed)
- Generated programmatically using Python (raw MP3 frame construction)
- Validation uses MP3 sync word check (0xFF 0xE0 header)
- NAudio.Lame not used due to platform limitations on Linux

## Adding New Fixtures

If you need different durations or formats:

1. Generate MP3 file programmatically (Python, ffmpeg, etc.)
2. Keep file size under 5KB
3. Add to `TestData/` directory
4. Add `<EmbeddedResource Include="Audio\TestData\your_file.mp3" />` to .csproj
5. Update `Mp3TestHelper` to load the new resource
