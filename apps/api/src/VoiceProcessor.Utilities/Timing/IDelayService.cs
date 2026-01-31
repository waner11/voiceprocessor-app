namespace VoiceProcessor.Utilities.Timing;

public interface IDelayService
{
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default);
}
