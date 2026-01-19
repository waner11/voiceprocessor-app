using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Domain.DTOs.Responses.Auth;

public record UserInfoResponse
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public string? Name { get; init; }
    public required SubscriptionTier Tier { get; init; }
    public required int CreditsRemaining { get; init; }
}
