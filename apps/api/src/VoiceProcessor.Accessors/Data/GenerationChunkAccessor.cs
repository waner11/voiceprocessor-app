using Microsoft.EntityFrameworkCore;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Data.DbContext;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Data;

public class GenerationChunkAccessor : IGenerationChunkAccessor
{
    private readonly VoiceProcessorDbContext _dbContext;

    public GenerationChunkAccessor(VoiceProcessorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GenerationChunk?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.GenerationChunks
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<GenerationChunk>> GetByGenerationIdAsync(
        Guid generationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.GenerationChunks
            .Where(c => c.GenerationId == generationId)
            .OrderBy(c => c.Index)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateManyAsync(
        IEnumerable<GenerationChunk> chunks,
        CancellationToken cancellationToken = default)
    {
        _dbContext.GenerationChunks.AddRange(chunks);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(
        Guid id,
        ChunkStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.GenerationChunks
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.Status, status)
                .SetProperty(c => c.ErrorMessage, errorMessage)
                .SetProperty(c => c.StartedAt, status == ChunkStatus.Processing ? DateTime.UtcNow : (DateTime?)null),
                cancellationToken);
    }

    public async Task SetCompletedAsync(
        Guid id,
        string audioUrl,
        int audioDurationMs,
        long audioSizeBytes,
        decimal actualCost,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.GenerationChunks
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.Status, ChunkStatus.Completed)
                .SetProperty(c => c.AudioUrl, audioUrl)
                .SetProperty(c => c.AudioDurationMs, audioDurationMs)
                .SetProperty(c => c.AudioSizeBytes, audioSizeBytes)
                .SetProperty(c => c.Cost, actualCost)
                .SetProperty(c => c.CompletedAt, DateTime.UtcNow),
                cancellationToken);
    }
}
