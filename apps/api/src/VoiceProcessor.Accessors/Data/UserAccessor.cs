using Microsoft.EntityFrameworkCore;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Data.DbContext;
using VoiceProcessor.Domain.Entities;

namespace VoiceProcessor.Accessors.Data;

public class UserAccessor : IUserAccessor
{
    private readonly VoiceProcessorDbContext _dbContext;

    public UserAccessor(VoiceProcessorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task AddCreditsAsync(Guid userId, int credits, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.CreditsRemaining, u => u.CreditsRemaining + credits),
                cancellationToken);
    }

    public async Task<bool> TryDeductCreditsAsync(
        Guid userId, int credits, Guid idempotencyKey,
        Guid? generationId = null, CancellationToken cancellationToken = default)
    {
        var deductionId = Guid.NewGuid();
        var rows = await _dbContext.Database.ExecuteSqlAsync($@"
WITH ins AS (
    INSERT INTO credit_deductions (id, user_id, idempotency_key, generation_id, credits, created_at)
    VALUES ({deductionId}, {userId}, {idempotencyKey}, {generationId}, {credits}, now())
    ON CONFLICT (idempotency_key) DO NOTHING
    RETURNING 1
)
UPDATE users
SET credits_remaining = credits_remaining - {credits},
    credits_used_this_month = credits_used_this_month + {credits}
WHERE id = {userId}
  AND EXISTS (SELECT 1 FROM ins)", cancellationToken);

        // Returns 1 when credits were deducted (new idempotency key + valid userId).
        // Returns 0 if idempotency key already exists (duplicate) or userId not found.
        // In the current flow, userId is always valid (loaded from Generation entity).
        return rows == 1;
    }
}
