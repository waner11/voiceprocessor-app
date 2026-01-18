using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoiceProcessor.Engines.Chunking;
using VoiceProcessor.Engines.Contracts;
using VoiceProcessor.Engines.Pricing;
using VoiceProcessor.Engines.Routing;

namespace VoiceProcessor.Engines.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEngines(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Chunking Engine (stateless, can be singleton)
        services.AddSingleton<IChunkingEngine, ChunkingEngine>();

        // Pricing Engine
        services.Configure<PricingOptions>(
            configuration.GetSection(PricingOptions.SectionName));
        services.AddScoped<IPricingEngine, PricingEngine>();

        // Routing Engine
        services.Configure<RoutingOptions>(
            configuration.GetSection(RoutingOptions.SectionName));
        services.AddScoped<IRoutingEngine, RoutingEngine>();

        return services;
    }
}
