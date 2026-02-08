using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Domain.Contracts.Accessors;

public interface INotificationAccessor
{
    Task SendStatusUpdateAsync(Guid userId, Guid generationId, GenerationStatus status, string? message = null, CancellationToken cancellationToken = default);
    Task SendProgressAsync(Guid userId, Guid generationId, int progress, int? currentChunk = null, int? totalChunks = null, CancellationToken cancellationToken = default);
    Task SendCompletedAsync(Guid userId, Guid generationId, string audioUrl, int durationMs, CancellationToken cancellationToken = default);
    Task SendFailedAsync(Guid userId, Guid generationId, string error, CancellationToken cancellationToken = default);
}
