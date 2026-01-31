namespace VoiceProcessor.Utilities.Timing;

public class DelayService : IDelayService
{
    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
        => Task.Delay(delay, cancellationToken);
}
