using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Providers;

public interface ITtsProviderFactory
{
    ITtsProviderAccessor GetProvider(Provider provider);
    IEnumerable<ITtsProviderAccessor> GetAllProviders();
    bool IsProviderAvailable(Provider provider);
}

public class TtsProviderFactory : ITtsProviderFactory
{
    private readonly IEnumerable<ITtsProviderAccessor> _providers;
    private readonly Dictionary<Provider, ITtsProviderAccessor> _providerMap;

    public TtsProviderFactory(IEnumerable<ITtsProviderAccessor> providers)
    {
        _providers = providers;
        _providerMap = providers.ToDictionary(p => p.Provider);
    }

    public ITtsProviderAccessor GetProvider(Provider provider)
    {
        if (_providerMap.TryGetValue(provider, out var accessor))
        {
            return accessor;
        }

        throw new NotSupportedException($"TTS provider '{provider}' is not configured");
    }

    public IEnumerable<ITtsProviderAccessor> GetAllProviders()
    {
        return _providers;
    }

    public bool IsProviderAvailable(Provider provider)
    {
        return _providerMap.ContainsKey(provider);
    }
}
