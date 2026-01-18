using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Contracts;

public interface IGenerationAccessor
{
    Task<Generation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Generation?> GetByIdWithChunksAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Generation> Items, int TotalCount)> GetByUserPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        GenerationStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<Generation> CreateAsync(Generation generation, CancellationToken cancellationToken = default);

    Task UpdateAsync(Generation generation, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(
        Guid id,
        GenerationStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default);

    Task UpdateProgressAsync(
        Guid id,
        int chunksCompleted,
        int progress,
        CancellationToken cancellationToken = default);

    Task SetCompletedAsync(
        Guid id,
        string audioUrl,
        string audioFormat,
        int audioDurationMs,
        long audioSizeBytes,
        decimal actualCost,
        CancellationToken cancellationToken = default);
}
