using Microsoft.EntityFrameworkCore;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Data.DbContext;
using VoiceProcessor.Domain.Entities;

namespace VoiceProcessor.Accessors.Data;

public class PaymentHistoryAccessor : IPaymentHistoryAccessor
{
    private readonly VoiceProcessorDbContext _dbContext;

    public PaymentHistoryAccessor(VoiceProcessorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentHistory?> GetByStripeSessionIdAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PaymentHistories
            .FirstOrDefaultAsync(p => p.StripeSessionId == sessionId, cancellationToken);
    }

    public async Task<PaymentHistory> CreateAsync(
        PaymentHistory payment,
        CancellationToken cancellationToken = default)
    {
        _dbContext.PaymentHistories.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<IReadOnlyList<PaymentHistory>> GetByUserIdAsync(
        Guid userId,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PaymentHistories
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
