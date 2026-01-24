namespace VoiceProcessor.Domain.DTOs.Requests.Payments;

public record CreateCheckoutRequest
{
    public required string PriceId { get; init; }
    public required string SuccessUrl { get; init; }
    public required string CancelUrl { get; init; }
}
