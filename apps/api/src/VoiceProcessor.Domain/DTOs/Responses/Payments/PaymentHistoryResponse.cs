namespace VoiceProcessor.Domain.DTOs.Responses.Payments;

public record PaymentHistoryResponse
{
    public required Guid Id { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required int CreditsAdded { get; init; }
    public required string PackName { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedAt { get; init; }
}
