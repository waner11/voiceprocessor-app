using Microsoft.AspNetCore.SignalR;
using VoiceProcessor.Clients.Api.Hubs;
using VoiceProcessor.Domain.Contracts.Accessors;
using VoiceProcessor.Domain.Contracts.Hubs;
using VoiceProcessor.Domain.DTOs.Notifications;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Clients.Api.Notifications;

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
            var notification = new StatusUpdateNotification
            {
                GenerationId = generationId.ToString(),
                Status = MapStatus(status),
                Message = message
            };
            await _hubContext.Clients.User(userId.ToString()).StatusUpdate(notification);
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
            var notification = new ProgressNotification
            {
                GenerationId = generationId.ToString(),
                Progress = progress,
                CurrentChunk = currentChunk,
                TotalChunks = totalChunks
            };
            await _hubContext.Clients.User(userId.ToString()).Progress(notification);
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
            var notification = new CompletedNotification
            {
                GenerationId = generationId.ToString(),
                AudioUrl = audioUrl,
                Duration = durationMs
            };
            await _hubContext.Clients.User(userId.ToString()).Completed(notification);
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
            var notification = new FailedNotification
            {
                GenerationId = generationId.ToString(),
                Error = error
            };
            await _hubContext.Clients.User(userId.ToString()).Failed(notification);
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
        _ => "queued"
    };
}
