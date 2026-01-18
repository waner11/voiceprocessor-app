using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Data;
using VoiceProcessor.Accessors.Providers;
using VoiceProcessor.Accessors.Storage;

namespace VoiceProcessor.Accessors.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccessors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database accessors
        services.AddScoped<IUserAccessor, UserAccessor>();
        services.AddScoped<IVoiceAccessor, VoiceAccessor>();
        services.AddScoped<IGenerationAccessor, GenerationAccessor>();
        services.AddScoped<IGenerationChunkAccessor, GenerationChunkAccessor>();

        // Storage accessor
        services.Configure<LocalStorageOptions>(
            configuration.GetSection(LocalStorageOptions.SectionName));
        services.AddScoped<IStorageAccessor, LocalStorageAccessor>();

        // TTS Provider accessors with IHttpClientFactory
        AddElevenLabsAccessor(services, configuration);
        AddOpenAiTtsAccessor(services, configuration);

        // TTS Provider factory for runtime resolution
        services.AddScoped<ITtsProviderFactory, TtsProviderFactory>();

        return services;
    }

    private static void AddElevenLabsAccessor(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ElevenLabsOptions>(
            configuration.GetSection(ElevenLabsOptions.SectionName));

        services.AddHttpClient<ElevenLabsAccessor>(client =>
        {
            var options = configuration
                .GetSection(ElevenLabsOptions.SectionName)
                .Get<ElevenLabsOptions>();

            client.BaseAddress = new Uri("https://api.elevenlabs.io/v1/");
            client.DefaultRequestHeaders.Add("xi-api-key", options?.ApiKey ?? string.Empty);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromMinutes(2);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Register as ITtsProviderAccessor for collection injection
        services.AddScoped<ITtsProviderAccessor, ElevenLabsAccessor>(sp =>
            sp.GetRequiredService<ElevenLabsAccessor>());
    }

    private static void AddOpenAiTtsAccessor(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OpenAiTtsOptions>(
            configuration.GetSection(OpenAiTtsOptions.SectionName));

        services.AddHttpClient<OpenAiTtsAccessor>(client =>
        {
            var options = configuration
                .GetSection(OpenAiTtsOptions.SectionName)
                .Get<OpenAiTtsOptions>();

            client.BaseAddress = new Uri("https://api.openai.com/v1/");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", options?.ApiKey ?? string.Empty);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromMinutes(2);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Register as ITtsProviderAccessor for collection injection
        services.AddScoped<ITtsProviderAccessor, OpenAiTtsAccessor>(sp =>
            sp.GetRequiredService<OpenAiTtsAccessor>());
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
