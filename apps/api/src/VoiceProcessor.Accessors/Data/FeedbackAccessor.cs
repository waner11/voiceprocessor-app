using Microsoft.EntityFrameworkCore;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Data.DbContext;
using VoiceProcessor.Domain.Entities;

namespace VoiceProcessor.Accessors.Data;

public class FeedbackAccessor : IFeedbackAccessor
{
    private readonly VoiceProcessorDbContext _dbContext;

    public FeedbackAccessor(VoiceProcessorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Feedback> UpsertAsync(Feedback feedback, CancellationToken cancellationToken = default)
    {
        // Try to find existing feedback for this generation and user
        var existing = await _dbContext.Feedbacks
            .FirstOrDefaultAsync(f => f.GenerationId == feedback.GenerationId && f.UserId == feedback.UserId, cancellationToken);

        if (existing is null)
        {
            // Create new feedback
            _dbContext.Feedbacks.Add(feedback);
        }
        else
        {
            // Update existing feedback - full field replacement
            existing.Rating = feedback.Rating;
            existing.Comment = feedback.Comment;
            existing.WasDownloaded = feedback.WasDownloaded;
            existing.PlaybackCount = feedback.PlaybackCount;
            existing.PlaybackDurationMs = feedback.PlaybackDurationMs;
            existing.UpdatedAt = DateTime.UtcNow;

            feedback = existing;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return feedback;
    }
}
