using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines;

public class ChapterTimingEngine : IChapterTimingEngine
{
    public List<ChapterDto> MapChaptersToTimestamps(
        IReadOnlyList<DetectedChapter> chapters,
        ICollection<GenerationChunk> chunks)
    {
        if (chapters.Count == 0)
        {
            return new List<ChapterDto>();
        }

        var chunkRanges = BuildChunkRanges(chunks);
        return chapters
            .Select(ch =>
            {
                var (startTimeMs, endTimeMs) = EstimateChapterTimes(ch.StartPosition, ch.EndPosition, chunkRanges);

                return new ChapterDto
                {
                    Title = ch.Title,
                    Index = ch.ChapterNumber,
                    StartPosition = ch.StartPosition,
                    EndPosition = ch.EndPosition,
                    EstimatedWordCount = ch.EstimatedWordCount,
                    StartTimeMs = startTimeMs,
                    EndTimeMs = endTimeMs
                };
            })
            .ToList();
    }

    private static List<ChunkRange> BuildChunkRanges(ICollection<GenerationChunk> chunks)
    {
        var ranges = new List<ChunkRange>();
        var currentTextPosition = 0;
        var currentTimeMs = 0;

        foreach (var chunk in chunks.OrderBy(c => c.Index))
        {
            var chunkLength = chunk.CharacterCount > 0
                ? chunk.CharacterCount
                : chunk.Text.Length;
            var chunkDurationMs = Math.Max(chunk.AudioDurationMs ?? 0, 0);

            var range = new ChunkRange(
                StartPosition: currentTextPosition,
                EndPosition: currentTextPosition + chunkLength,
                StartTimeMs: currentTimeMs,
                EndTimeMs: currentTimeMs + chunkDurationMs);

            ranges.Add(range);

            currentTextPosition = range.EndPosition;
            currentTimeMs = range.EndTimeMs;
        }

        return ranges;
    }

    private static (int StartTimeMs, int EndTimeMs) EstimateChapterTimes(
        int chapterStartPosition,
        int chapterEndPosition,
        List<ChunkRange> chunkRanges)
    {
        if (chunkRanges.Count == 0)
        {
            return (0, 0);
        }

        var startTimeMs = EstimateTimestampAtPosition(chapterStartPosition, chunkRanges);
        var endTimeMs = EstimateTimestampAtPosition(chapterEndPosition, chunkRanges);

        return (startTimeMs, Math.Max(startTimeMs, endTimeMs));
    }

    private static int EstimateTimestampAtPosition(int position, List<ChunkRange> chunkRanges)
    {
        if (position <= 0)
        {
            return 0;
        }

        foreach (var chunkRange in chunkRanges)
        {
            if (position <= chunkRange.StartPosition)
            {
                return chunkRange.StartTimeMs;
            }

            if (position < chunkRange.EndPosition)
            {
                var chunkLength = chunkRange.EndPosition - chunkRange.StartPosition;
                if (chunkLength <= 0)
                {
                    return chunkRange.StartTimeMs;
                }

                var elapsedCharacters = position - chunkRange.StartPosition;
                var completionRatio = elapsedCharacters / (double)chunkLength;
                var chunkDurationMs = chunkRange.EndTimeMs - chunkRange.StartTimeMs;

                return chunkRange.StartTimeMs + (int)Math.Round(
                    chunkDurationMs * completionRatio,
                    MidpointRounding.AwayFromZero);
            }
        }

        return chunkRanges[^1].EndTimeMs;
    }

    private sealed record ChunkRange(int StartPosition, int EndPosition, int StartTimeMs, int EndTimeMs);
}
