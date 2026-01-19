using Microsoft.EntityFrameworkCore;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Data.DbContext;
using VoiceProcessor.Domain.Entities;

namespace VoiceProcessor.Accessors.Data;

public class ApiKeyAccessor : IApiKeyAccessor
{
    private readonly VoiceProcessorDbContext _dbContext;

    public ApiKeyAccessor(VoiceProcessorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ApiKeys
            .Include(ak => ak.User)
            .FirstOrDefaultAsync(ak => ak.Id == id, cancellationToken);
    }

    public async Task<ApiKey?> GetByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ApiKeys
            .Include(ak => ak.User)
            .FirstOrDefaultAsync(ak => ak.KeyPrefix == prefix, cancellationToken);
    }

    public async Task<IReadOnlyList<ApiKey>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ApiKeys
            .Where(ak => ak.UserId == userId)
            .OrderByDescending(ak => ak.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ApiKey>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ApiKeys
            .Where(ak => ak.UserId == userId && ak.IsActive && ak.RevokedAt == null)
            .Where(ak => ak.ExpiresAt == null || ak.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(ak => ak.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ApiKey> CreateAsync(ApiKey apiKey, CancellationToken cancellationToken = default)
    {
        _dbContext.ApiKeys.Add(apiKey);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return apiKey;
    }

    public async Task UpdateAsync(ApiKey apiKey, CancellationToken cancellationToken = default)
    {
        _dbContext.ApiKeys.Update(apiKey);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ApiKeys
            .AnyAsync(ak => ak.UserId == userId && ak.IsActive && ak.RevokedAt == null, cancellationToken);
    }
}
