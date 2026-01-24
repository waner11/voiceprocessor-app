namespace VoiceProcessor.Domain.DTOs.Responses.Payments;

public record CreditPackResponse
{
    public required string PriceId { get; init; }
    public required string ProductId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required int Credits { get; init; }
    public required decimal PriceAmount { get; init; }
    public required string Currency { get; init; }
}
