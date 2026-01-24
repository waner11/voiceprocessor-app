namespace VoiceProcessor.Domain.DTOs.Responses.Payments;

public record CreditPacksResponse
{
    public required IReadOnlyList<CreditPackResponse> Packs { get; init; }
}
