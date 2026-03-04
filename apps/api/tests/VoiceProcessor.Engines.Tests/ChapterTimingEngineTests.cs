using FluentAssertions;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Engines;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines.Tests;

public class ChapterTimingEngineTests
{
    private readonly ChapterTimingEngine _engine = new();

    [Fact]
    public void MapChaptersToTimestamps_EmptyChapters_ReturnsEmptyList()
    {
        // Arrange
        var chapters = new List<DetectedChapter>();
        var chunks = new List<GenerationChunk>
        {
            new() { Index = 0, Text = "abc", CharacterCount = 3, AudioDurationMs = 1000 }
        };

        // Act
        var result = _engine.MapChaptersToTimestamps(chapters, chunks);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void MapChaptersToTimestamps_EmptyChunks_ReturnsZeroTimestamps()
    {
        // Arrange
        var chapters = new List<DetectedChapter>
        {
            new() { ChapterNumber = 1, Title = "Chapter 1", StartPosition = 10, EndPosition = 20, EstimatedWordCount = 5 }
        };

        // Act
        var result = _engine.MapChaptersToTimestamps(chapters, new List<GenerationChunk>());

        // Assert
        result.Should().HaveCount(1);
        result[0].StartTimeMs.Should().Be(0);
        result[0].EndTimeMs.Should().Be(0);
    }

    [Fact]
    public void MapChaptersToTimestamps_SingleChunk_InterpolatesWithinChunk()
    {
        // Arrange
        var chapters = new List<DetectedChapter>
        {
            new() { ChapterNumber = 1, Title = "Chapter 1", StartPosition = 25, EndPosition = 75, EstimatedWordCount = 10 }
        };
        var chunks = new List<GenerationChunk>
        {
            new() { Index = 0, Text = new string('a', 100), CharacterCount = 100, AudioDurationMs = 10000 }
        };

        // Act
        var result = _engine.MapChaptersToTimestamps(chapters, chunks);

        // Assert
        result.Should().HaveCount(1);
        result[0].StartTimeMs.Should().Be(2500);
        result[0].EndTimeMs.Should().Be(7500);
    }

    [Fact]
    public void MapChaptersToTimestamps_MultipleChunks_MapsAcrossBoundaries()
    {
        // Arrange
        var chapters = new List<DetectedChapter>
        {
            new() { ChapterNumber = 1, Title = "Chapter 1", StartPosition = 0, EndPosition = 50, EstimatedWordCount = 10 },
            new() { ChapterNumber = 2, Title = "Chapter 2", StartPosition = 50, EndPosition = 100, EstimatedWordCount = 10 }
        };
        var chunks = new List<GenerationChunk>
        {
            new() { Index = 0, Text = new string('a', 50), CharacterCount = 50, AudioDurationMs = 3000 },
            new() { Index = 1, Text = new string('b', 50), CharacterCount = 50, AudioDurationMs = 5000 }
        };

        // Act
        var result = _engine.MapChaptersToTimestamps(chapters, chunks);

        // Assert
        result.Should().HaveCount(2);
        result[0].StartTimeMs.Should().Be(0);
        result[0].EndTimeMs.Should().Be(3000);
        result[1].StartTimeMs.Should().Be(3000);
        result[1].EndTimeMs.Should().Be(8000);
    }
}
