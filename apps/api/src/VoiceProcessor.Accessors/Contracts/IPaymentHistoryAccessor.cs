using VoiceProcessor.Domain.Entities;

namespace VoiceProcessor.Accessors.Contracts;

public interface IPaymentHistoryAccessor
{
    Task<PaymentHistory?> GetByStripeSessionIdAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    Task<PaymentHistory> CreateAsync(
        PaymentHistory payment,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentHistory>> GetByUserIdAsync(
        Guid userId,
        int limit = 50,
        CancellationToken cancellationToken = default);
}
