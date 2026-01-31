using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoiceProcessor.Managers.Auth;
using VoiceProcessor.Managers.Contracts;
using VoiceProcessor.Managers.Generation;
using VoiceProcessor.Managers.Payment;
using VoiceProcessor.Managers.User;
using VoiceProcessor.Managers.Voice;
using VoiceProcessor.Utilities.Timing;

namespace VoiceProcessor.Managers.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddManagers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Auth Manager
        services.AddScoped<IAuthManager, AuthManager>();

        // User Manager
        services.AddScoped<IUserManager, UserManager>();

        // Voice Manager
        services.AddScoped<IVoiceManager, VoiceManager>();

        // Generation Manager
        services.AddScoped<IGenerationManager, GenerationManager>();

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
