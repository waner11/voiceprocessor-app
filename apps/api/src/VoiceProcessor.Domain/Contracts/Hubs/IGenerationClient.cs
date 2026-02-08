using VoiceProcessor.Domain.DTOs.Notifications;

namespace VoiceProcessor.Domain.Contracts.Hubs;

public interface IGenerationClient
{
    Task StatusUpdate(StatusUpdateNotification notification);
    Task Progress(ProgressNotification notification);
    Task Completed(CompletedNotification notification);
    Task Failed(FailedNotification notification);
}
