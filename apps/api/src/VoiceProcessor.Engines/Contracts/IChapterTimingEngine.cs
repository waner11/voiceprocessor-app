using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Domain.Entities;

namespace VoiceProcessor.Engines.Contracts;

public interface IChapterTimingEngine
{
    List<ChapterDto> MapChaptersToTimestamps(
        IReadOnlyList<DetectedChapter> chapters,
        ICollection<GenerationChunk> chunks);
}
