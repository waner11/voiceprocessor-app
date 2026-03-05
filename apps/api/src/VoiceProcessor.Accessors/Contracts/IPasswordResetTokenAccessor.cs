using VoiceProcessor.Domain.Entities;

namespace VoiceProcessor.Accessors.Contracts;

public interface IPasswordResetTokenAccessor
{
    Task<PasswordResetToken> CreateAsync(PasswordResetToken token, CancellationToken cancellationToken = default);

    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task MarkAsUsedAsync(Guid tokenId, CancellationToken cancellationToken = default);

    Task InvalidateAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
}
