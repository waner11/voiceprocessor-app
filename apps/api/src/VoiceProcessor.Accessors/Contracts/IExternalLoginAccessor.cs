using VoiceProcessor.Domain.Entities;

namespace VoiceProcessor.Accessors.Contracts;

public interface IExternalLoginAccessor
{
    Task<ExternalLogin?> GetByProviderAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExternalLogin>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ExternalLogin?> GetByUserAndProviderAsync(
        Guid userId,
        string provider,
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        ExternalLogin externalLogin,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
