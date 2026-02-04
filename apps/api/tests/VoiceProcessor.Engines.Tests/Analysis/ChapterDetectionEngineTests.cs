using FluentAssertions;
using VoiceProcessor.Engines.Analysis;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines.Tests.Analysis;

public class ChapterDetectionEngineTests
{
    private readonly ChapterDetectionEngine _engine = new();

    [Fact]
    public void DetectChapters_NumberedChapters_ReturnsCorrect()
    {
        // Arrange
        var text = @"Chapter 1: The Beginning
This is the first chapter content.
Some more text here.

Chapter 2: The Middle
This is the second chapter content.
More content follows.";

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().HaveCount(2);
        result[0].ChapterNumber.Should().Be(1);
        result[0].Title.Should().Be("Chapter 1: The Beginning");
        result[0].StartPosition.Should().Be(0);
        result[0].EndPosition.Should().BeGreaterThan(0);
        result[0].EstimatedWordCount.Should().BeGreaterThan(0);
        
        result[1].ChapterNumber.Should().Be(2);
        result[1].Title.Should().Be("Chapter 2: The Middle");
        result[1].StartPosition.Should().BeGreaterThan(result[0].StartPosition);
        result[1].EndPosition.Should().Be(text.Length);
    }

    [Fact]
    public void DetectChapters_WrittenNumbers_ReturnsCorrect()
    {
        // Arrange
        var text = @"Chapter One
First chapter content here.

Chapter Two
Second chapter content here.

Chapter Three
Third chapter content here.";

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().HaveCount(3);
        result[0].Title.Should().Be("Chapter One");
        result[1].Title.Should().Be("Chapter Two");
        result[2].Title.Should().Be("Chapter Three");
    }

    [Fact]
    public void DetectChapters_PartHeaders_ReturnsCorrect()
    {
        // Arrange
        var text = @"Part 1
First part content.

Part 2
Second part content.";

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Part 1");
        result[1].Title.Should().Be("Part 2");
    }

    [Fact]
    public void DetectChapters_NamedSections_ReturnsCorrect()
    {
        // Arrange
        var text = @"Prologue
The story begins here.

Chapter 1
Main content.

Epilogue
The story ends here.";

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().HaveCount(3);
        result[0].Title.Should().Be("Prologue");
        result[1].Title.Should().Be("Chapter 1");
        result[2].Title.Should().Be("Epilogue");
    }

    [Fact]
    public void DetectChapters_Dividers_ReturnsCorrect()
    {
        // Arrange
        var text = @"***
First section content here.
---
Second section content here.
===
Third section content here.";

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().HaveCount(3);
        result[0].Title.Should().Be("***");
        result[1].Title.Should().Be("---");
        result[2].Title.Should().Be("===");
    }

    [Fact]
    public void DetectChapters_NoChapters_ReturnsEmpty()
    {
        // Arrange
        var text = @"This is just plain text without any chapter markers.
It has multiple paragraphs.
But no chapter indicators.";

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void DetectChapters_EmptyText_ReturnsEmpty()
    {
        // Arrange
        var text = string.Empty;

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void DetectChapters_NullText_ReturnsEmpty()
    {
        // Arrange
        string? text = null;

        // Act
        var result = _engine.DetectChapters(text!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void DetectChapters_CaseInsensitive_DetectsAll()
    {
        // Arrange
        var text = @"CHAPTER 1
Content here.

chapter 2
More content.

Chapter 3
Even more content.";

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().HaveCount(3);
        result[0].Title.Should().Be("CHAPTER 1");
        result[1].Title.Should().Be("chapter 2");
        result[2].Title.Should().Be("Chapter 3");
    }

    [Fact]
    public void DetectChapters_ChapterInDialogue_NotDetected()
    {
        // Arrange
        var text = @"Chapter 1
The story begins.

He said ""You should read Chapter 5 next.""
She replied ""I already read Chapter 3.""

Chapter 2
The story continues.";

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Chapter 1");
        result[1].Title.Should().Be("Chapter 2");
    }

    [Fact]
    public void DetectChapters_MixedFormats_DetectsAll()
    {
        // Arrange
        var text = @"Prologue
Introduction text.

Chapter 1
First chapter.
***
Section break.

Part 2
Second part.

Ch. 3
Third chapter.

Epilogue
Conclusion.";

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().HaveCount(6);
        result[0].Title.Should().Be("Prologue");
        result[1].Title.Should().Be("Chapter 1");
        result[2].Title.Should().Be("***");
        result[3].Title.Should().Be("Part 2");
        result[4].Title.Should().Be("Ch. 3");
        result[5].Title.Should().Be("Epilogue");
    }

    [Fact]
    public void HasChapters_WithChapters_ReturnsTrue()
    {
        // Arrange
        var text = @"Chapter 1
Content here.";

        // Act
        var result = _engine.HasChapters(text);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasChapters_WithoutChapters_ReturnsFalse()
    {
        // Arrange
        var text = @"Just plain text without any chapter markers.";

        // Act
        var result = _engine.HasChapters(text);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DetectChapters_WordCountEstimation_ReasonablyAccurate()
    {
        // Arrange
        var text = @"Chapter 1
This is a test with exactly ten words here.

Chapter 2
Another test with exactly ten words here too.";

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().HaveCount(2);
        result[0].EstimatedWordCount.Should().BeInRange(8, 12); // Allow some variance
        result[1].EstimatedWordCount.Should().BeInRange(8, 12);
    }

    [Fact]
    public void DetectChapters_ChAbbreviation_Detected()
    {
        // Arrange
        var text = @"Ch. 1
First chapter.

Ch 2
Second chapter.";

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Ch. 1");
        result[1].Title.Should().Be("Ch 2");
    }

    [Fact]
    public void DetectChapters_SectionHeaders_Detected()
    {
        // Arrange
        var text = @"Section 1
First section.

Section 2
Second section.";

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Section 1");
        result[1].Title.Should().Be("Section 2");
    }

    [Fact]
    public void DetectChapters_AllNamedSections_Detected()
    {
        // Arrange
        var text = @"Foreword
Foreword content.

Introduction
Introduction content.

Preface
Preface content.

Afterword
Afterword content.";

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().HaveCount(4);
        result[0].Title.Should().Be("Foreword");
        result[1].Title.Should().Be("Introduction");
        result[2].Title.Should().Be("Preface");
        result[3].Title.Should().Be("Afterword");
    }

    [Fact]
    public void DetectChapters_PartWithWrittenNumber_Detected()
    {
        // Arrange
        var text = @"Part One
First part.

Part Two
Second part.";

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Part One");
        result[1].Title.Should().Be("Part Two");
    }

    [Fact]
    public void DetectChapters_EndPositions_AreCorrect()
    {
        // Arrange
        var text = @"Chapter 1
Content 1.

Chapter 2
Content 2.

Chapter 3
Content 3.";

        // Act
        var result = _engine.DetectChapters(text);

        // Assert
        result.Should().HaveCount(3);
        
        // First chapter ends where second starts
        result[0].EndPosition.Should().Be(result[1].StartPosition);
        
        // Second chapter ends where third starts
        result[1].EndPosition.Should().Be(result[2].StartPosition);
        
        // Last chapter ends at text end
        result[2].EndPosition.Should().Be(text.Length);
    }
}
