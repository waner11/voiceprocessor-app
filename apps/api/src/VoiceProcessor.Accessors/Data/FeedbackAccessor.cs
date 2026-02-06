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
        var result = await _dbContext.Feedbacks
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
                feedback.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return result ?? feedback;
    }
}
