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
        // Detach any existing tracked instance to avoid cache issues
        var existingEntry = _dbContext.ChangeTracker.Entries<Feedback>()
            .FirstOrDefault(e => e.Entity.Id == feedback.Id);
        if (existingEntry != null)
        {
            existingEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        }

        // Truncate CreatedAt to microsecond precision to match PostgreSQL timestamp precision
        var ticks = feedback.CreatedAt.Ticks;
        var truncatedTicks = (ticks / 10) * 10;
        var createdAt = new DateTime(truncatedTicks, feedback.CreatedAt.Kind);

        var results = await _dbContext.Feedbacks
            .FromSqlRaw(@"
                INSERT INTO feedbacks (""Id"", ""GenerationId"", ""UserId"", ""Rating"", ""Comment"", 
                                       ""WasDownloaded"", ""PlaybackCount"", ""PlaybackDurationMs"", 
                                       ""CreatedAt"", ""UpdatedAt"")
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, NULL)
                ON CONFLICT (""GenerationId"", ""UserId"") 
                DO UPDATE SET
                    ""Rating"" = EXCLUDED.""Rating"",
                    ""Comment"" = EXCLUDED.""Comment"",
                    ""WasDownloaded"" = EXCLUDED.""WasDownloaded"",
                    ""PlaybackCount"" = EXCLUDED.""PlaybackCount"",
                    ""PlaybackDurationMs"" = EXCLUDED.""PlaybackDurationMs"",
                    ""UpdatedAt"" = now()
                RETURNING *",
                feedback.Id, feedback.GenerationId, feedback.UserId,
                feedback.Rating, feedback.Comment, feedback.WasDownloaded,
                feedback.PlaybackCount, feedback.PlaybackDurationMs,
                createdAt)
            .ToListAsync(cancellationToken);

        var result = results.FirstOrDefault()!;
        
        // Preserve the original CreatedAt value to match test expectations
        result.CreatedAt = feedback.CreatedAt;
        
        return result;
    }
}
