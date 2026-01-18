using Microsoft.EntityFrameworkCore;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Data.DbContext;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Data;

public class GenerationAccessor : IGenerationAccessor
{
    private readonly VoiceProcessorDbContext _dbContext;

    public GenerationAccessor(VoiceProcessorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Generation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Generations
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<Generation?> GetByIdWithChunksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Generations
            .Include(g => g.Chunks.OrderBy(c => c.Index))
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Generation> Items, int TotalCount)> GetByUserPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        GenerationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Generations.Where(g => g.UserId == userId);

        if (status.HasValue)
            query = query.Where(g => g.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Generation> CreateAsync(Generation generation, CancellationToken cancellationToken = default)
    {
        _dbContext.Generations.Add(generation);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return generation;
    }

    public async Task UpdateAsync(Generation generation, CancellationToken cancellationToken = default)
    {
        _dbContext.Generations.Update(generation);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(
        Guid id,
        GenerationStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Generations
            .Where(g => g.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(g => g.Status, status)
                .SetProperty(g => g.ErrorMessage, errorMessage)
                .SetProperty(g => g.StartedAt, status == GenerationStatus.Processing ? DateTime.UtcNow : (DateTime?)null),
                cancellationToken);
    }

    public async Task UpdateProgressAsync(
        Guid id,
        int chunksCompleted,
        int progress,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Generations
            .Where(g => g.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(g => g.ChunksCompleted, chunksCompleted)
                .SetProperty(g => g.Progress, progress),
                cancellationToken);
    }

    public async Task SetCompletedAsync(
        Guid id,
        string audioUrl,
        string audioFormat,
        int audioDurationMs,
        long audioSizeBytes,
        decimal actualCost,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Generations
            .Where(g => g.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(g => g.Status, GenerationStatus.Completed)
                .SetProperty(g => g.AudioUrl, audioUrl)
                .SetProperty(g => g.AudioFormat, audioFormat)
                .SetProperty(g => g.AudioDurationMs, audioDurationMs)
                .SetProperty(g => g.AudioSizeBytes, audioSizeBytes)
                .SetProperty(g => g.ActualCost, actualCost)
                .SetProperty(g => g.Progress, 100)
                .SetProperty(g => g.CompletedAt, DateTime.UtcNow),
                cancellationToken);
    }
}
