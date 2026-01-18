namespace VoiceProcessor.Managers.Contracts;

public interface IUserManager
{
    Task<Domain.Entities.User?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Domain.Entities.User?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> HasSufficientCreditsAsync(
        Guid userId,
        int requiredCredits,
        CancellationToken cancellationToken = default);

    Task DeductCreditsAsync(
        Guid userId,
        int credits,
        CancellationToken cancellationToken = default);

    Task UpdateLastActivityAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
