using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoiceProcessor.Engines.Audio;
using VoiceProcessor.Engines.Chunking;
using VoiceProcessor.Engines.Contracts;
using VoiceProcessor.Engines.Pricing;
using VoiceProcessor.Engines.Routing;
using VoiceProcessor.Engines.Security;

namespace VoiceProcessor.Engines.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEngines(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Chunking Engine (stateless, can be singleton)
        services.AddSingleton<IChunkingEngine, ChunkingEngine>();

        // Audio Merge Engine (stateless, can be singleton)
        services.AddSingleton<IAudioMergeEngine, AudioMergeEngine>();

        // Pricing Engine
        services.Configure<PricingOptions>(
            configuration.GetSection(PricingOptions.SectionName));
        services.AddScoped<IPricingEngine, PricingEngine>();

        // Routing Engine
        services.Configure<RoutingOptions>(
            configuration.GetSection(RoutingOptions.SectionName));
        services.AddScoped<IRoutingEngine, RoutingEngine>();

        // Security Engines (stateless, can be singleton)
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IJwtEngine, JwtEngine>();
        services.AddSingleton<IPasswordEngine, PasswordEngine>();
        services.AddSingleton<IApiKeyEngine, ApiKeyEngine>();

        // OAuth Engines
        services.Configure<OAuthOptions>(
            configuration.GetSection(OAuthOptions.SectionName));
        services.AddHttpClient<GoogleOAuthEngine>();
        services.AddHttpClient<GitHubOAuthEngine>();
        services.AddScoped<IOAuthEngine, GoogleOAuthEngine>(sp =>
            sp.GetRequiredService<GoogleOAuthEngine>());
        services.AddScoped<IOAuthEngine, GitHubOAuthEngine>(sp =>
            sp.GetRequiredService<GitHubOAuthEngine>());

        return services;
    }
}
