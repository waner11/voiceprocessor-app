using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Contracts;

public interface IVoiceAccessor
{
    Task<Voice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Voice?> GetByProviderVoiceIdAsync(
        Provider provider,
        string providerVoiceId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Voice>> GetAllAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Voice>> GetByProviderAsync(
        Provider provider,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Voice> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Provider? provider = null,
        string? language = null,
        string? gender = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    Task<Voice> CreateAsync(Voice voice, CancellationToken cancellationToken = default);

    Task UpdateAsync(Voice voice, CancellationToken cancellationToken = default);

    Task UpsertAsync(Voice voice, CancellationToken cancellationToken = default);
}
