using Microsoft.EntityFrameworkCore;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Data.DbContext;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Data;

public class VoiceAccessor : IVoiceAccessor
{
    private readonly VoiceProcessorDbContext _dbContext;

    public VoiceAccessor(VoiceProcessorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Voice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Voices
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Voice?> GetByProviderVoiceIdAsync(
        Provider provider,
        string providerVoiceId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Voices
            .FirstOrDefaultAsync(v => v.Provider == provider && v.ProviderVoiceId == providerVoiceId, cancellationToken);
    }

    public async Task<IReadOnlyList<Voice>> GetAllAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Voices.AsQueryable();

        if (activeOnly)
            query = query.Where(v => v.IsActive);

        return await query.OrderBy(v => v.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Voice>> GetByProviderAsync(
        Provider provider,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Voices.Where(v => v.Provider == provider);

        if (activeOnly)
            query = query.Where(v => v.IsActive);

        return await query.OrderBy(v => v.Name).ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Voice> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Provider? provider = null,
        string? language = null,
        string? gender = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Voices.AsQueryable();

        if (activeOnly)
            query = query.Where(v => v.IsActive);

        if (provider.HasValue)
            query = query.Where(v => v.Provider == provider.Value);

        if (!string.IsNullOrWhiteSpace(language))
            query = query.Where(v => v.Language == language);

        if (!string.IsNullOrWhiteSpace(gender))
            query = query.Where(v => v.Gender == gender);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(v => v.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Voice> CreateAsync(Voice voice, CancellationToken cancellationToken = default)
    {
        _dbContext.Voices.Add(voice);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return voice;
    }

    public async Task UpdateAsync(Voice voice, CancellationToken cancellationToken = default)
    {
        _dbContext.Voices.Update(voice);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertAsync(Voice voice, CancellationToken cancellationToken = default)
    {
        var existing = await GetByProviderVoiceIdAsync(voice.Provider, voice.ProviderVoiceId, cancellationToken);

        if (existing is null)
        {
            _dbContext.Voices.Add(voice);
        }
        else
        {
            existing.Name = voice.Name;
            existing.Description = voice.Description;
            existing.Language = voice.Language;
            existing.Accent = voice.Accent;
            existing.Gender = voice.Gender;
            existing.AgeGroup = voice.AgeGroup;
            existing.UseCase = voice.UseCase;
            existing.PreviewUrl = voice.PreviewUrl;
            existing.CostPerThousandChars = voice.CostPerThousandChars;
            existing.IsActive = voice.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
