using Microsoft.AspNetCore.SignalR;
using VoiceProcessor.Clients.Api.Hubs;
using VoiceProcessor.Domain.Contracts.Accessors;
using VoiceProcessor.Domain.Contracts.Hubs;
using VoiceProcessor.Domain.DTOs.Notifications;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Clients.Api.Notifications;

/// <summary>
/// Implements notification delivery via SignalR hub context.
/// Lives in Clients.Api (not Accessors project) because it requires
/// IHubContext which is a Clients-layer infrastructure concern.
/// </summary>
public class GenerationNotificationAccessor : INotificationAccessor
{
    private readonly IHubContext<GenerationHub, IGenerationClient> _hubContext;
    private readonly ILogger<GenerationNotificationAccessor> _logger;

    public GenerationNotificationAccessor(
        IHubContext<GenerationHub, IGenerationClient> hubContext,
        ILogger<GenerationNotificationAccessor> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendStatusUpdateAsync(Guid userId, Guid generationId, GenerationStatus status, string? message = null, CancellationToken cancellationToken = default)
    {
        try
        {
             cancellationToken.ThrowIfCancellationRequested();
             
             var notification = new StatusUpdateNotification
             {
                 GenerationId = generationId,
                 Status = MapStatus(status),
                 Message = message
             };
             await _hubContext.Clients.User(userId.ToString()).StatusUpdate(notification).WaitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send status update notification for generation {GenerationId}", generationId);
        }
    }

    public async Task SendProgressAsync(Guid userId, Guid generationId, int progress, int? currentChunk = null, int? totalChunks = null, CancellationToken cancellationToken = default)
    {
        try
        {
             cancellationToken.ThrowIfCancellationRequested();
             
             var notification = new ProgressNotification
             {
                 GenerationId = generationId,
                 Progress = progress,
                 CurrentChunk = currentChunk,
                 TotalChunks = totalChunks
             };
             await _hubContext.Clients.User(userId.ToString()).Progress(notification).WaitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send progress notification for generation {GenerationId}", generationId);
        }
    }

    public async Task SendCompletedAsync(Guid userId, Guid generationId, string audioUrl, int durationMs, CancellationToken cancellationToken = default)
    {
        try
        {
             cancellationToken.ThrowIfCancellationRequested();
             
             var notification = new CompletedNotification
             {
                 GenerationId = generationId,
                 AudioUrl = audioUrl,
                 Duration = durationMs
             };
             await _hubContext.Clients.User(userId.ToString()).Completed(notification).WaitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send completed notification for generation {GenerationId}", generationId);
        }
    }

    public async Task SendFailedAsync(Guid userId, Guid generationId, string error, CancellationToken cancellationToken = default)
    {
        try
        {
             cancellationToken.ThrowIfCancellationRequested();
             
             var notification = new FailedNotification
             {
                 GenerationId = generationId,
                 Error = error
             };
             await _hubContext.Clients.User(userId.ToString()).Failed(notification).WaitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send failed notification for generation {GenerationId}", generationId);
        }
    }

     private static string MapStatus(GenerationStatus status) => status switch
     {
         GenerationStatus.Pending => "queued",
         GenerationStatus.Analyzing => "processing",
         GenerationStatus.Chunking => "processing",
         GenerationStatus.Processing => "processing",
         GenerationStatus.Merging => "processing",
         GenerationStatus.Completed => "completed",
         GenerationStatus.Failed => "failed",
         GenerationStatus.Cancelled => "cancelled",
         _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown generation status")
     };
}
