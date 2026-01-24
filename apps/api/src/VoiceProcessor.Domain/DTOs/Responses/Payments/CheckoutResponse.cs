namespace VoiceProcessor.Domain.DTOs.Responses.Payments;

public record CheckoutResponse
{
    public required string CheckoutUrl { get; init; }
    public required string SessionId { get; init; }
}
