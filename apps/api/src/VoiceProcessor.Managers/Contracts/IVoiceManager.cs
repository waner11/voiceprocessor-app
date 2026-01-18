using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Managers.Contracts;

public interface IVoiceManager
{
    Task<PagedResponse<VoiceResponse>> GetVoicesAsync(
        int page = 1,
        int pageSize = 50,
        Provider? provider = null,
        string? language = null,
        string? gender = null,
        CancellationToken cancellationToken = default);

    Task<VoiceResponse?> GetVoiceAsync(
        Guid voiceId,
        CancellationToken cancellationToken = default);

    Task<Dictionary<Provider, IReadOnlyList<VoiceResponse>>> GetVoicesByProviderAsync(
        CancellationToken cancellationToken = default);

    Task RefreshVoiceCatalogAsync(
        CancellationToken cancellationToken = default);
}
