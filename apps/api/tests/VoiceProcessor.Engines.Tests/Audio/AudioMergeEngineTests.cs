using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VoiceProcessor.Engines.Audio;
using VoiceProcessor.Engines.Contracts;
using VoiceProcessor.Engines.Tests.Audio.TestData;

namespace VoiceProcessor.Engines.Tests.Audio;

public class AudioMergeEngineTests
{
    private readonly Mock<ILogger<AudioMergeEngine>> _loggerMock;
    private readonly AudioMergeEngine _engine;

    public AudioMergeEngineTests()
    {
        _loggerMock = new Mock<ILogger<AudioMergeEngine>>();
        _engine = new AudioMergeEngine(_loggerMock.Object);
    }

    [Fact]
    public async Task MergeAudioChunksAsync_EmptyChunks_ThrowsArgumentException()
    {
        // Arrange
        var emptyChunks = Array.Empty<byte[]>();
        var options = new AudioMergeOptions { OutputFormat = "mp3" };

        // Act
        var act = async () => await _engine.MergeAudioChunksAsync(emptyChunks, options);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("No audio chunks provided*")
            .WithParameterName("audioChunks");
    }

    [Fact]
    public async Task MergeAudioChunksAsync_SingleChunk_ReturnsSameData()
    {
        // Arrange
        var chunk = Mp3TestHelper.CreateShortMp3Chunk();
        var chunks = new[] { chunk };
        var options = new AudioMergeOptions { OutputFormat = "mp3" };

        // Act
        var result = await _engine.MergeAudioChunksAsync(chunks, options);

        // Assert
        result.Should().NotBeNull();
        result.AudioData.Should().BeEquivalentTo(chunk, "single chunk optimization should return same data");
        result.ContentType.Should().Be("audio/mpeg");
        result.DurationMs.Should().BeGreaterThan(0);
        result.SizeBytes.Should().Be(chunk.Length);
    }

    [Fact]
    public async Task MergeAudioChunksAsync_TwoChunks_ProducesValidMergedAudio()
    {
        // Arrange
        var chunk1 = Mp3TestHelper.CreateShortMp3Chunk();
        var chunk2 = Mp3TestHelper.CreateShortMp3Chunk();
        var chunks = new[] { chunk1, chunk2 };
        var options = new AudioMergeOptions { OutputFormat = "mp3" };

        // Act
        var result = await _engine.MergeAudioChunksAsync(chunks, options);

        // Assert
        result.Should().NotBeNull();
        result.AudioData.Should().NotBeEmpty();
        Mp3TestHelper.IsValidMp3(result.AudioData).Should().BeTrue("merged audio should be valid MP3");
        result.ContentType.Should().Be("audio/mpeg");
        result.DurationMs.Should().BeGreaterThan(0);
        result.SizeBytes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task MergeAudioChunksAsync_WithSilenceBetweenChunks_IncreasesDuration()
    {
        // Arrange
        var chunk1 = Mp3TestHelper.CreateShortMp3Chunk();
        var chunk2 = Mp3TestHelper.CreateShortMp3Chunk();
        var chunks = new[] { chunk1, chunk2 };
        
        var optionsWithoutSilence = new AudioMergeOptions { OutputFormat = "mp3", SilenceBetweenChunksMs = 0 };
        var optionsWithSilence = new AudioMergeOptions { OutputFormat = "mp3", SilenceBetweenChunksMs = 500 };

        // Act
        var resultWithoutSilence = await _engine.MergeAudioChunksAsync(chunks, optionsWithoutSilence);
        var resultWithSilence = await _engine.MergeAudioChunksAsync(chunks, optionsWithSilence);

        // Assert
        resultWithSilence.DurationMs.Should().BeGreaterThan(resultWithoutSilence.DurationMs,
            "adding silence should increase duration");
        
        // Silence should add approximately 500ms (one gap between two chunks)
        var expectedIncrease = 500;
        var actualIncrease = resultWithSilence.DurationMs - resultWithoutSilence.DurationMs;
        actualIncrease.Should().BeInRange(expectedIncrease - 100, expectedIncrease + 100,
            "silence duration should be approximately as specified");
    }

    [Fact]
    public async Task MergeAudioChunksAsync_MergedResult_HasCorrectContentType()
    {
        // Arrange
        var chunk1 = Mp3TestHelper.CreateShortMp3Chunk();
        var chunk2 = Mp3TestHelper.CreateShortMp3Chunk();
        var chunks = new[] { chunk1, chunk2 };
        var options = new AudioMergeOptions { OutputFormat = "mp3" };

        // Act
        var result = await _engine.MergeAudioChunksAsync(chunks, options);

        // Assert
        result.ContentType.Should().Be("audio/mpeg");
    }

    [Fact]
    public async Task MergeAudioChunksAsync_MergedResult_HasPositiveSizeBytes()
    {
        // Arrange
        var chunk1 = Mp3TestHelper.CreateShortMp3Chunk();
        var chunk2 = Mp3TestHelper.CreateShortMp3Chunk();
        var chunks = new[] { chunk1, chunk2 };
        var options = new AudioMergeOptions { OutputFormat = "mp3" };

        // Act
        var result = await _engine.MergeAudioChunksAsync(chunks, options);

        // Assert
        result.SizeBytes.Should().BeGreaterThan(0);
        result.SizeBytes.Should().Be(result.AudioData.Length);
    }
}
