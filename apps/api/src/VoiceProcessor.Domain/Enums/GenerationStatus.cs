namespace VoiceProcessor.Domain.Enums;

public enum GenerationStatus
{
    Pending,
    Analyzing,
    Chunking,
    Processing,
    Merging,
    Completed,
    Failed,
    Cancelled
}
