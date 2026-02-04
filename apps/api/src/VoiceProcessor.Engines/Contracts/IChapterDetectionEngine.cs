using VoiceProcessor.Domain.DTOs;

namespace VoiceProcessor.Engines.Contracts;

public interface IChapterDetectionEngine
{
    IReadOnlyList<DetectedChapter> DetectChapters(string text);
    
    bool HasChapters(string text);
}
