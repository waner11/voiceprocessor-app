using Microsoft.Extensions.Logging;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Managers.User;

public class UserManager : IUserManager
{
    private readonly IUserAccessor _userAccessor;
    private readonly ILogger<UserManager> _logger;

    public UserManager(
        IUserAccessor userAccessor,
        ILogger<UserManager> logger)
    {
        _userAccessor = userAccessor;
        _logger = logger;
    }

    public async Task<Domain.Entities.User?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _userAccessor.GetByIdAsync(userId, cancellationToken);
    }

    public async Task<Domain.Entities.User?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _userAccessor.GetByEmailAsync(email, cancellationToken);
    }

    public async Task<bool> HasSufficientCreditsAsync(
        Guid userId,
        int requiredCredits,
        CancellationToken cancellationToken = default)
    {
        var user = await _userAccessor.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found when checking credits", userId);
            return false;
        }

        return user.CreditsRemaining >= requiredCredits;
    }

    public async Task DeductCreditsAsync(
        Guid userId,
        int credits,
        CancellationToken cancellationToken = default)
    {
        var user = await _userAccessor.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            _logger.LogError("User {UserId} not found when deducting credits", userId);
            throw new InvalidOperationException($"User {userId} not found");
        }

        if (user.CreditsRemaining < credits)
        {
            _logger.LogWarning("User {UserId} has insufficient credits: {Available} < {Required}",
                userId, user.CreditsRemaining, credits);
            throw new InvalidOperationException("Insufficient credits");
        }

        user.CreditsRemaining -= credits;
        user.CreditsUsedThisMonth += credits;

        await _userAccessor.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Deducted {Credits} credits from user {UserId}, remaining: {Remaining}",
            credits, userId, user.CreditsRemaining);
    }

    public async Task UpdateLastActivityAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userAccessor.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return;

        user.LastActiveAt = DateTime.UtcNow;
        await _userAccessor.UpdateAsync(user, cancellationToken);
    }
}
