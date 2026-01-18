namespace VoiceProcessor.Domain.DTOs.Requests;

public record SubmitFeedbackRequest
{
    public int? Rating { get; init; }
    public string? Comment { get; init; }
}
