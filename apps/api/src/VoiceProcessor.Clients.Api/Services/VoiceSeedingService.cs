using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Clients.Api.Services;

public class VoiceSeedingService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VoiceSeedingService> _logger;

    public VoiceSeedingService(
        IServiceProvider serviceProvider,
        ILogger<VoiceSeedingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Voice seeding service starting");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var voiceManager = scope.ServiceProvider.GetRequiredService<IVoiceManager>();

            // Check if voices already exist
            var existingVoices = await voiceManager.GetVoicesAsync(
                page: 1, pageSize: 1, cancellationToken: cancellationToken);

            if (existingVoices.TotalCount == 0)
            {
                _logger.LogInformation("No voices found in database, seeding from providers...");
                await voiceManager.RefreshVoiceCatalogAsync(cancellationToken);
                _logger.LogInformation("Voice seeding completed");
            }
            else
            {
                _logger.LogInformation("Voices already exist ({Count} total), skipping seed",
                    existingVoices.TotalCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Voice seeding failed - voices can be manually refreshed via POST /api/v1/voices/refresh");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
