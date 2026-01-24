namespace VoiceProcessor.Domain.DTOs.Responses.Payments;

public record PaymentHistoryListResponse
{
    public required IReadOnlyList<PaymentHistoryResponse> Payments { get; init; }
}
