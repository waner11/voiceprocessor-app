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

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Handle race condition: concurrent insert for same (GenerationId, UserId)
            // Clear the change tracker and retry by fetching the existing record
            _dbContext.ChangeTracker.Clear();

            // Re-fetch the existing record that was inserted concurrently
            var existingRecord = await _dbContext.Feedbacks
                .FirstOrDefaultAsync(f => f.GenerationId == feedback.GenerationId && f.UserId == feedback.UserId, cancellationToken);

            if (existingRecord is not null)
            {
                existingRecord.Rating = feedback.Rating;
                existingRecord.Comment = feedback.Comment;
                existingRecord.WasDownloaded = feedback.WasDownloaded;
                existingRecord.PlaybackCount = feedback.PlaybackCount;
                existingRecord.PlaybackDurationMs = feedback.PlaybackDurationMs;
                existingRecord.UpdatedAt = DateTime.UtcNow;

                feedback = existingRecord;

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return feedback;
    }
}
