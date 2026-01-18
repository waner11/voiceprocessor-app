using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoiceProcessor.Managers.Contracts;
using VoiceProcessor.Managers.Generation;
using VoiceProcessor.Managers.User;
using VoiceProcessor.Managers.Voice;

namespace VoiceProcessor.Managers.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddManagers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // User Manager
        services.AddScoped<IUserManager, UserManager>();

        // Voice Manager
        services.AddScoped<IVoiceManager, VoiceManager>();

        // Generation Manager
        services.AddScoped<IGenerationManager, GenerationManager>();

        return services;
    }
}
