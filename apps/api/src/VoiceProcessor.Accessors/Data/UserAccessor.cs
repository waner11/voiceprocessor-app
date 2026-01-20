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

    public async Task DeductCreditsAsync(Guid userId, int credits, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.CreditsRemaining, u => u.CreditsRemaining - credits)
                .SetProperty(u => u.CreditsUsedThisMonth, u => u.CreditsUsedThisMonth + credits),
                cancellationToken);
    }
}
