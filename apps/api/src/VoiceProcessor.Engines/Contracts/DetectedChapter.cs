namespace VoiceProcessor.Engines.Contracts;

public record DetectedChapter
{
    public int ChapterNumber { get; init; }
    public string Title { get; init; } = string.Empty;
    public int StartPosition { get; init; }
    public int EndPosition { get; init; }
    public int EstimatedWordCount { get; init; }
}
