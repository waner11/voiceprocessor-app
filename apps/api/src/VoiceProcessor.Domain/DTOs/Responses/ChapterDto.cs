namespace VoiceProcessor.Domain.DTOs.Responses;

public record ChapterDto
{
    public required string Title { get; init; }
    public required int Index { get; init; }
    public required int StartPosition { get; init; }
    public required int EndPosition { get; init; }
    public required int EstimatedWordCount { get; init; }
    public required int StartTimeMs { get; init; }
    public required int EndTimeMs { get; init; }
}
