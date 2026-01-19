using VoiceProcessor.Domain.Entities;

namespace VoiceProcessor.Accessors.Contracts;

public interface IApiKeyAccessor
{
    Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiKey?> GetByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApiKey>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApiKey>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiKey> CreateAsync(ApiKey apiKey, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApiKey apiKey, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
