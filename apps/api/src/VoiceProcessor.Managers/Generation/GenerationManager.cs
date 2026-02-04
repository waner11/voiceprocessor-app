using Hangfire;
using Microsoft.Extensions.Logging;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Providers;
using VoiceProcessor.Domain.DTOs.Requests;
using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Engines.Contracts;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Managers.Generation;

public class GenerationManager : IGenerationManager
{
    private readonly IGenerationAccessor _generationAccessor;
    private readonly IVoiceAccessor _voiceAccessor;
    private readonly IUserAccessor _userAccessor;
    private readonly IFeedbackAccessor _feedbackAccessor;
    private readonly ITtsProviderFactory _providerFactory;
    private readonly IChunkingEngine _chunkingEngine;
    private readonly IPricingEngine _pricingEngine;
    private readonly IRoutingEngine _routingEngine;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<GenerationManager> _logger;

    public GenerationManager(
        IGenerationAccessor generationAccessor,
        IVoiceAccessor voiceAccessor,
        IUserAccessor userAccessor,
        IFeedbackAccessor feedbackAccessor,
        ITtsProviderFactory providerFactory,
        IChunkingEngine chunkingEngine,
        IPricingEngine pricingEngine,
        IRoutingEngine routingEngine,
        IBackgroundJobClient backgroundJobClient,
        ILogger<GenerationManager> logger)
    {
        _generationAccessor = generationAccessor;
        _voiceAccessor = voiceAccessor;
        _userAccessor = userAccessor;
        _feedbackAccessor = feedbackAccessor;
        _providerFactory = providerFactory;
        _chunkingEngine = chunkingEngine;
        _pricingEngine = pricingEngine;
        _routingEngine = routingEngine;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public async Task<CostEstimateResponse> EstimateCostAsync(
        EstimateCostRequest request,
        CancellationToken cancellationToken = default)
    {
        var characterCount = request.Text.Length;
        var estimatedChunks = _chunkingEngine.EstimateChunkCount(request.Text);

        // Get voice cost if voice specified
        decimal? voiceCost = null;
        if (request.VoiceId.HasValue)
        {
            var voice = await _voiceAccessor.GetByIdAsync(request.VoiceId.Value, cancellationToken);
            voiceCost = voice?.CostPerThousandChars;
        }

        // Calculate estimates for all providers
        var providerEstimates = _pricingEngine.CalculateAllProviderEstimates(new PricingContext
        {
            CharacterCount = characterCount,
            Provider = request.Provider,
            VoiceId = request.VoiceId,
            VoiceCostPerThousandChars = voiceCost
        });

        // Get available providers
        var availableProviders = _providerFactory.GetAllProviders()
            .Select(p => p.Provider)
            .ToHashSet();

        // Determine recommended provider via routing
        var routingContext = new RoutingContext
        {
            VoiceId = request.VoiceId ?? Guid.Empty,
            CharacterCount = characterCount,
            Preference = request.RoutingPreference,
            AvailableProviders = availableProviders
        };

        Provider? recommendedProvider = null;
        try
        {
            var routingDecision = await _routingEngine.SelectProviderAsync(routingContext, cancellationToken);
            recommendedProvider = routingDecision.SelectedProvider;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not determine recommended provider");
        }

        // Find the best estimate
        var bestEstimate = providerEstimates.First();

        return new CostEstimateResponse
        {
            CharacterCount = characterCount,
            EstimatedChunks = estimatedChunks,
            EstimatedCost = bestEstimate.TotalCost,
            CreditsRequired = bestEstimate.CreditsRequired,
            Currency = bestEstimate.Currency,
            RecommendedProvider = recommendedProvider,
            ProviderEstimates = providerEstimates.Select(pe => new ProviderEstimate
            {
                Provider = pe.Provider,
                Cost = pe.TotalCost,
                CreditsRequired = pe.CreditsRequired,
                EstimatedDurationMs = EstimateDuration(characterCount, pe.Provider),
                QualityTier = GetQualityTier(pe.Provider),
                IsAvailable = availableProviders.Contains(pe.Provider)
            }).ToList()
        };
    }

    public async Task<GenerationResponse> CreateGenerationAsync(
        Guid userId,
        CreateGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating generation for user {UserId}, {CharCount} characters",
            userId, request.Text.Length);

        // Validate voice exists
        var voice = await _voiceAccessor.GetByIdAsync(request.VoiceId, cancellationToken);
        if (voice is null)
        {
            throw new InvalidOperationException($"Voice {request.VoiceId} not found");
        }

        // Calculate cost estimate
        var costEstimate = _pricingEngine.CalculateEstimate(new PricingContext
        {
            CharacterCount = request.Text.Length,
            Provider = voice.Provider,
            VoiceId = voice.Id,
            VoiceCostPerThousandChars = voice.CostPerThousandChars
        });

        // Check user credits
        var user = await _userAccessor.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        if (user.CreditsRemaining < costEstimate.CreditsRequired)
        {
            throw new InvalidOperationException("Insufficient credits");
        }

        // Determine chunks
        var chunks = _chunkingEngine.SplitText(request.Text);

        // Select provider via routing
        var availableProviders = _providerFactory.GetAllProviders()
            .Select(p => p.Provider)
            .ToHashSet();

        var routingDecision = await _routingEngine.SelectProviderAsync(new RoutingContext
        {
            VoiceId = voice.Id,
            CharacterCount = request.Text.Length,
            Preference = request.RoutingPreference,
            PreferredProvider = voice.Provider,
            VoiceProvider = voice.Provider,
            AvailableProviders = availableProviders
        }, cancellationToken);

        // Create generation record
        var generation = new Domain.Entities.Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = voice.Id,
            InputText = request.Text,
            CharacterCount = request.Text.Length,
            Status = GenerationStatus.Pending,
            RoutingPreference = request.RoutingPreference,
            SelectedProvider = routingDecision.SelectedProvider,
            AudioFormat = request.AudioFormat ?? "mp3",
            EstimatedCost = costEstimate.EstimatedCost,
            ChunkCount = chunks.Count,
            ChunksCompleted = 0,
            Progress = 0,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _generationAccessor.CreateAsync(generation, cancellationToken);

        _logger.LogInformation("Generation {GenerationId} created, {ChunkCount} chunks, provider {Provider}",
            generation.Id, chunks.Count, routingDecision.SelectedProvider);

        // Queue background job to process generation
        _backgroundJobClient.Enqueue<IGenerationProcessor>(
            "generation",
            processor => processor.ProcessGenerationAsync(generation.Id, CancellationToken.None));

        return MapToResponse(generation);
    }

    public async Task<GenerationResponse?> GetGenerationAsync(
        Guid generationId,
        CancellationToken cancellationToken = default)
    {
        var generation = await _generationAccessor.GetByIdAsync(generationId, cancellationToken);
        return generation is null ? null : MapToResponse(generation);
    }

    public async Task<PagedResponse<GenerationResponse>> GetGenerationsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        GenerationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _generationAccessor.GetByUserPagedAsync(
            userId, page, pageSize, status, cancellationToken);

        return new PagedResponse<GenerationResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task SubmitFeedbackAsync(
        Guid generationId,
        Guid userId,
        SubmitFeedbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var generation = await _generationAccessor.GetByIdAsync(generationId, cancellationToken);
        if (generation is null)
        {
            throw new InvalidOperationException($"Generation {generationId} not found");
        }

        if (generation.UserId != userId)
        {
            throw new InvalidOperationException("Generation does not belong to user");
        }

        var feedback = new Feedback
        {
            Id = Guid.NewGuid(),
            GenerationId = generationId,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _feedbackAccessor.UpsertAsync(feedback, cancellationToken);

        _logger.LogInformation("Feedback submitted for generation {GenerationId}, rating: {Rating}",
            generationId, request.Rating);
    }

    public async Task<bool> CancelGenerationAsync(
        Guid generationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var generation = await _generationAccessor.GetByIdAsync(generationId, cancellationToken);
        if (generation is null)
        {
            return false;
        }

        if (generation.UserId != userId)
        {
            throw new InvalidOperationException("Generation does not belong to user");
        }

        if (generation.Status is GenerationStatus.Completed or GenerationStatus.Failed)
        {
            return false;
        }

        await _generationAccessor.UpdateStatusAsync(
            generationId, GenerationStatus.Cancelled, cancellationToken: cancellationToken);

        _logger.LogInformation("Generation {GenerationId} cancelled", generationId);

        return true;
    }

    private static GenerationResponse MapToResponse(Domain.Entities.Generation generation)
    {
        return new GenerationResponse
        {
            Id = generation.Id,
            Status = generation.Status,
            CharacterCount = generation.CharacterCount,
            Progress = generation.Progress,
            ChunkCount = generation.ChunkCount,
            ChunksCompleted = generation.ChunksCompleted,
            Provider = generation.SelectedProvider,
            AudioUrl = generation.AudioUrl,
            AudioFormat = generation.AudioFormat,
            AudioDurationMs = generation.AudioDurationMs,
            EstimatedCost = generation.EstimatedCost,
            ActualCost = generation.ActualCost,
            ErrorMessage = generation.ErrorMessage,
            CreatedAt = generation.CreatedAt,
            StartedAt = generation.StartedAt,
            CompletedAt = generation.CompletedAt
        };
    }

    private static int EstimateDuration(int characterCount, Provider provider)
    {
        // Rough estimate: ~150 words per minute, ~5 chars per word
        var estimatedWords = characterCount / 5;
        var baseDurationMs = (int)(estimatedWords / 150.0 * 60 * 1000);

        // Add provider-specific processing overhead
        var overhead = provider switch
        {
            Provider.ElevenLabs => 800,
            Provider.OpenAI => 600,
            Provider.GoogleCloud => 400,
            Provider.AmazonPolly => 300,
            _ => 500
        };

        return baseDurationMs + overhead;
    }

    private static string GetQualityTier(Provider provider) => provider switch
    {
        Provider.ElevenLabs => "Premium",
        Provider.OpenAI => "High",
        Provider.FishAudio => "Premium",
        Provider.Cartesia => "High",
        Provider.GoogleCloud => "Standard",
        Provider.Deepgram => "Standard",
        Provider.AmazonPolly => "Basic",
        _ => "Standard"
    };
}
