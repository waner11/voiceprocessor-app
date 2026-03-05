using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoiceProcessor.Managers.Auth;
using VoiceProcessor.Managers.Contracts;
using VoiceProcessor.Managers.Documents;
using VoiceProcessor.Managers.Generation;
using VoiceProcessor.Managers.Payment;
using VoiceProcessor.Managers.Voice;
using VoiceProcessor.Utilities.Timing;
using VoiceProcessor.Managers.Options;

namespace VoiceProcessor.Managers.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddManagers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // App Options
        services.Configure<AppOptions>(
            configuration.GetSection(AppOptions.SectionName));

        // Auth Manager
        services.AddScoped<IAuthManager, AuthManager>();

        // Voice Manager
        services.AddScoped<IVoiceManager, VoiceManager>();

        // Generation Manager
        services.AddScoped<IGenerationManager, GenerationManager>();

        services.AddScoped<IDocumentManager, DocumentManager>();

        // Generation Processor (background job processor)
        services.AddScoped<IGenerationProcessor, GenerationProcessor>();

        // Payment Manager
        services.AddScoped<IPaymentManager, PaymentManager>();

        // Timing utilities
        // TODO: Move IDelayService registration to a VoiceProcessor.Utilities DI module when more utility services are added
        services.AddSingleton<IDelayService, DelayService>();

        return services;
    }
}
