using FluentAssertions;
using VoiceProcessor.Engines.Chunking;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines.Tests.Chunking;

public class ChunkingEngineTests
{
    private readonly ChunkingEngine _engine = new();

    [Fact]
    public void SplitText_EmptyText_ReturnsEmptyList()
    {
        // Arrange
        var text = string.Empty;

        // Act
        var result = _engine.SplitText(text);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SplitText_TextShorterThanMaxSize_ReturnsSingleChunk()
    {
        // Arrange
        var text = "This is a short text.";
        var options = new ChunkingOptions { MaxChunkSize = 100 };

        // Act
        var result = _engine.SplitText(text, options);

        // Assert
        result.Should().HaveCount(1);
        result[0].Index.Should().Be(0);
        result[0].Text.Should().Be(text);
        result[0].StartPosition.Should().Be(0);
        result[0].EndPosition.Should().Be(text.Length);
        result[0].CharacterCount.Should().Be(text.Length);
    }

    [Fact]
    public void SplitText_SplitsOnParagraphBoundary()
    {
        // Arrange
        var paragraph1 = "First paragraph with some content.";
        var paragraph2 = "Second paragraph with more content.";
        var text = paragraph1 + "\n\n" + paragraph2;
        var options = new ChunkingOptions { MaxChunkSize = 50, MinChunkSize = 10 };

        // Act
        var result = _engine.SplitText(text, options);

        // Assert
        result.Should().HaveCount(2);
        result[0].Text.Trim().Should().Be(paragraph1);
        result[1].Text.Trim().Should().Be(paragraph2);
    }

    [Fact]
    public void SplitText_SplitsOnSentenceBoundary()
    {
        // Arrange
        var sentence1 = "First sentence here with more words.";
        var sentence2 = "Second sentence here with more words!";
        var sentence3 = "Third sentence here with more words?";
        var text = sentence1 + " " + sentence2 + " " + sentence3;
        var options = new ChunkingOptions { MaxChunkSize = 50, MinChunkSize = 10, PreserveParagraphBoundaries = false };

        // Act
        var result = _engine.SplitText(text, options);

        // Assert
        result.Should().HaveCountGreaterThan(1);
        result.Any(chunk => chunk.Text.Trim().EndsWith(".") || chunk.Text.Trim().EndsWith("!") || chunk.Text.Trim().EndsWith("?")).Should().BeTrue();
    }

    [Fact]
    public void SplitText_DoesNotSplitOnAbbreviation()
    {
        // Arrange
        var text = "Dr. X. Smith is a great doctor. He works at the hospital.";
        var options = new ChunkingOptions { MaxChunkSize = 40, MinChunkSize = 10 };

        // Act
        var result = _engine.SplitText(text, options);

        // Assert
        result.Should().HaveCount(2);
        result[0].Text.Should().Contain("Dr. X. Smith");
        result[0].Text.Should().NotEndWith("Dr.");
        result[0].Text.Should().NotEndWith("X.");
    }

    [Fact]
    public void SplitText_SplitsOnWordBoundary_WhenNoSentenceBreak()
    {
        // Arrange
        var text = "This is a very long text without any sentence endings and it should split on word boundaries instead of breaking in the middle of words";
        var options = new ChunkingOptions { MaxChunkSize = 50, MinChunkSize = 10, PreserveSentenceBoundaries = false };

        // Act
        var result = _engine.SplitText(text, options);

        // Assert
        result.Should().HaveCountGreaterThan(1);
        foreach (var chunk in result)
        {
            chunk.Text.Trim().Should().NotBeEmpty();
            chunk.Text.Trim().Should().NotStartWith(" ");
        }
    }

    [Fact]
    public void SplitText_HardBreak_WhenNoValidBreakPoint()
    {
        // Arrange
        var text = new string('a', 200);
        var options = new ChunkingOptions { MaxChunkSize = 50, MinChunkSize = 10 };

        // Act
        var result = _engine.SplitText(text, options);

        // Assert
        result.Should().HaveCountGreaterThan(1);
        foreach (var chunk in result.Take(result.Count - 1))
        {
            chunk.Text.Length.Should().Be(50);
        }
    }

    [Fact]
    public void SplitText_WindowsVsUnixLineEndings()
    {
        // Arrange
        var paragraph1 = "First paragraph.";
        var paragraph2 = "Second paragraph.";
        var textWindows = paragraph1 + "\r\n\n" + paragraph2;
        var options = new ChunkingOptions { MaxChunkSize = 30, MinChunkSize = 10 };

        // Act
        var result = _engine.SplitText(textWindows, options);

        // Assert
        result.Should().HaveCount(2);
        result[0].Text.Trim().Should().Be(paragraph1);
        result[1].Text.Trim().Should().Be(paragraph2);
    }

    [Fact]
    public void EstimateChunkCount_ReturnsCorrectEstimate()
    {
        // Arrange
        var text = new string('a', 10000);
        var options = new ChunkingOptions { MaxChunkSize = 5000 };

        // Act
        var estimate = _engine.EstimateChunkCount(text, options);

        // Assert
        var avgChunkSize = (int)(options.MaxChunkSize * 0.85);
        var expectedCount = (int)Math.Ceiling((double)text.Length / avgChunkSize);
        estimate.Should().Be(expectedCount);
    }
}
