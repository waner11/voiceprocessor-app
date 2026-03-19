namespace VoiceProcessor.Domain.DTOs.Responses;

public record UsageResponse
{
    public required int CreditsUsedThisMonth { get; init; }
    public required int CreditsRemaining { get; init; }
    public required int GenerationsCount { get; init; }
    public required int TotalAudioMinutes { get; init; }
}
