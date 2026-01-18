using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Contracts;

public interface IGenerationChunkAccessor
{
    Task<GenerationChunk?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GenerationChunk>> GetByGenerationIdAsync(
        Guid generationId,
        CancellationToken cancellationToken = default);

    Task CreateManyAsync(
        IEnumerable<GenerationChunk> chunks,
        CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(
        Guid id,
        ChunkStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default);

    Task SetCompletedAsync(
        Guid id,
        string audioUrl,
        int audioDurationMs,
        long audioSizeBytes,
        decimal actualCost,
        CancellationToken cancellationToken = default);
}
