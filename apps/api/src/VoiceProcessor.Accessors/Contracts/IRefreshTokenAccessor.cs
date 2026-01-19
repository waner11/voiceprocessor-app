using VoiceProcessor.Domain.Entities;

namespace VoiceProcessor.Accessors.Contracts;

public interface IRefreshTokenAccessor
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
}
