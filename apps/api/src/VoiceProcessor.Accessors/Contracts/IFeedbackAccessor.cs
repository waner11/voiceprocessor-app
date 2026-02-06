using VoiceProcessor.Domain.Entities;

namespace VoiceProcessor.Accessors.Contracts;

public interface IFeedbackAccessor
{
    Task<Feedback> UpsertAsync(Feedback feedback, CancellationToken cancellationToken = default);
}
