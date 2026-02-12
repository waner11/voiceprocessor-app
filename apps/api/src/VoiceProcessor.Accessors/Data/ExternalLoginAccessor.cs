using Microsoft.EntityFrameworkCore;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Data.DbContext;
using VoiceProcessor.Domain.Entities;

namespace VoiceProcessor.Accessors.Data;

public class ExternalLoginAccessor : IExternalLoginAccessor
{
    private readonly VoiceProcessorDbContext _context;

    public ExternalLoginAccessor(VoiceProcessorDbContext context)
    {
        _context = context;
    }

    public async Task<ExternalLogin?> GetByProviderAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ExternalLogins
            .Include(e => e.User)
            .FirstOrDefaultAsync(
                e => e.Provider == provider && e.ProviderUserId == providerUserId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ExternalLogin>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ExternalLogins
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ExternalLogin?> GetByUserAndProviderAsync(
        Guid userId,
        string provider,
        CancellationToken cancellationToken = default)
    {
        return await _context.ExternalLogins
            .FirstOrDefaultAsync(
                e => e.UserId == userId && e.Provider == provider,
                cancellationToken);
    }

    public async Task CreateAsync(
        ExternalLogin externalLogin,
        CancellationToken cancellationToken = default)
    {
        _context.ExternalLogins.Add(externalLogin);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var externalLogin = await _context.ExternalLogins.FindAsync(new object[] { id }, cancellationToken);
        if (externalLogin is not null)
        {
            _context.ExternalLogins.Remove(externalLogin);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
