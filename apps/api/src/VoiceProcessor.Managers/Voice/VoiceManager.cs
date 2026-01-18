using Microsoft.Extensions.Logging;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Providers;
using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Managers.Voice;

public class VoiceManager : IVoiceManager
{
    private readonly IVoiceAccessor _voiceAccessor;
    private readonly ITtsProviderFactory _providerFactory;
    private readonly ILogger<VoiceManager> _logger;

    public VoiceManager(
        IVoiceAccessor voiceAccessor,
        ITtsProviderFactory providerFactory,
        ILogger<VoiceManager> logger)
    {
        _voiceAccessor = voiceAccessor;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public async Task<PagedResponse<VoiceResponse>> GetVoicesAsync(
        int page = 1,
        int pageSize = 50,
        Provider? provider = null,
        string? language = null,
        string? gender = null,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _voiceAccessor.GetPagedAsync(
            page, pageSize, provider, language, gender, activeOnly: true, cancellationToken);

        return new PagedResponse<VoiceResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<VoiceResponse?> GetVoiceAsync(
        Guid voiceId,
        CancellationToken cancellationToken = default)
    {
        var voice = await _voiceAccessor.GetByIdAsync(voiceId, cancellationToken);
        return voice is null ? null : MapToResponse(voice);
    }

    public async Task<Dictionary<Provider, IReadOnlyList<VoiceResponse>>> GetVoicesByProviderAsync(
        CancellationToken cancellationToken = default)
    {
        var allVoices = await _voiceAccessor.GetAllAsync(activeOnly: true, cancellationToken);

        return allVoices
            .GroupBy(v => v.Provider)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<VoiceResponse>)g.Select(MapToResponse).ToList());
    }

    public async Task RefreshVoiceCatalogAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting voice catalog refresh");

        var providers = _providerFactory.GetAllProviders();
        var totalVoices = 0;

        foreach (var provider in providers)
        {
            try
            {
                _logger.LogDebug("Fetching voices from {Provider}", provider.Provider);

                var providerVoices = await provider.GetVoicesAsync(cancellationToken);

                foreach (var pv in providerVoices)
                {
                    var voice = new Domain.Entities.Voice
                    {
                        Id = Guid.NewGuid(),
                        Name = pv.Name,
                        Description = pv.Description,
                        Provider = provider.Provider,
                        ProviderVoiceId = pv.ProviderVoiceId,
                        Language = pv.Language,
                        Accent = pv.Accent,
                        Gender = pv.Gender,
                        AgeGroup = pv.AgeGroup,
                        UseCase = pv.UseCase,
                        PreviewUrl = pv.PreviewUrl,
                        CostPerThousandChars = GetDefaultCost(provider.Provider),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _voiceAccessor.UpsertAsync(voice, cancellationToken);
                    totalVoices++;
                }

                _logger.LogInformation("Synced {Count} voices from {Provider}",
                    providerVoices.Count, provider.Provider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh voices from {Provider}", provider.Provider);
            }
        }

        _logger.LogInformation("Voice catalog refresh completed, {TotalVoices} voices synced", totalVoices);
    }

    private static VoiceResponse MapToResponse(Domain.Entities.Voice voice)
    {
        return new VoiceResponse
        {
            Id = voice.Id,
            Name = voice.Name,
            Description = voice.Description,
            Provider = voice.Provider,
            Language = voice.Language,
            Accent = voice.Accent,
            Gender = voice.Gender,
            AgeGroup = voice.AgeGroup,
            UseCase = voice.UseCase,
            PreviewUrl = voice.PreviewUrl,
            CostPerThousandChars = voice.CostPerThousandChars
        };
    }

    private static decimal GetDefaultCost(Provider provider) => provider switch
    {
        Provider.ElevenLabs => 0.30m,
        Provider.OpenAI => 0.015m,
        Provider.GoogleCloud => 0.016m,
        Provider.AmazonPolly => 0.004m,
        Provider.FishAudio => 0.20m,
        Provider.Cartesia => 0.25m,
        Provider.Deepgram => 0.015m,
        _ => 0.10m
    };
}
