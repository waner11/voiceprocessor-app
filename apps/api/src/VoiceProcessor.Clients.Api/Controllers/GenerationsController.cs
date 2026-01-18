using Microsoft.AspNetCore.Mvc;
using VoiceProcessor.Domain.DTOs.Requests;
using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Clients.Api.Controllers;

public class GenerationsController : ApiControllerBase
{
    private readonly ILogger<GenerationsController> _logger;

    public GenerationsController(ILogger<GenerationsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Estimate the cost of a text-to-speech generation
    /// </summary>
    [HttpPost("estimate")]
    [ProducesResponseType(typeof(CostEstimateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CostEstimateResponse>> EstimateCost(
        [FromBody] EstimateCostRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cost estimate requested for {CharCount} characters",
            request.Text.Length);

        // TODO: Implement via IGenerationManager
        var response = new CostEstimateResponse
        {
            CharacterCount = request.Text.Length,
            EstimatedChunks = (int)Math.Ceiling(request.Text.Length / 5000.0),
            EstimatedCost = request.Text.Length * 0.00001m,
            Currency = "USD",
            RecommendedProvider = Provider.ElevenLabs,
            ProviderEstimates = []
        };

        return Ok(response);
    }

    /// <summary>
    /// Start a new text-to-speech generation
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(GenerationResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<GenerationResponse>> CreateGeneration(
        [FromBody] CreateGenerationRequest request,
        CancellationToken cancellationToken)
    {
        var generationId = Guid.NewGuid();

        _logger.LogInformation("Generation {GenerationId} started, {CharCount} characters, voice {VoiceId}",
            generationId, request.Text.Length, request.VoiceId);

        // TODO: Implement via IGenerationManager
        var response = new GenerationResponse
        {
            Id = generationId,
            Status = GenerationStatus.Pending,
            CharacterCount = request.Text.Length,
            Progress = 0,
            ChunkCount = 0,
            ChunksCompleted = 0,
            CreatedAt = DateTime.UtcNow
        };

        return AcceptedAtAction(nameof(GetGeneration), new { id = generationId }, response);
    }

    /// <summary>
    /// Get the status and details of a generation
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GenerationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenerationResponse>> GetGeneration(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting generation {GenerationId}", id);

        // TODO: Implement via IGenerationManager
        var response = new GenerationResponse
        {
            Id = id,
            Status = GenerationStatus.Completed,
            CharacterCount = 1000,
            Progress = 100,
            ChunkCount = 1,
            ChunksCompleted = 1,
            Provider = Provider.ElevenLabs,
            AudioUrl = $"https://storage.example.com/generations/{id}.mp3",
            AudioFormat = "mp3",
            AudioDurationMs = 5000,
            EstimatedCost = 0.01m,
            ActualCost = 0.01m,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            StartedAt = DateTime.UtcNow.AddMinutes(-4),
            CompletedAt = DateTime.UtcNow.AddMinutes(-3)
        };

        return Ok(response);
    }

    /// <summary>
    /// Get a user's generation history
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<GenerationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<GenerationResponse>>> GetGenerations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] GenerationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting generations, page {Page}, pageSize {PageSize}, status {Status}",
            page, pageSize, status);

        // TODO: Implement via IGenerationManager
        var response = new PagedResponse<GenerationResponse>
        {
            Items = [],
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };

        return Ok(response);
    }

    /// <summary>
    /// Submit feedback for a generation
    /// </summary>
    [HttpPost("{id:guid}/feedback")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitFeedback(
        Guid id,
        [FromBody] SubmitFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Feedback submitted for generation {GenerationId}, rating {Rating}",
            id, request.Rating);

        // TODO: Implement via IGenerationManager
        return NoContent();
    }

    /// <summary>
    /// Cancel a pending or in-progress generation
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelGeneration(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancellation requested for generation {GenerationId}", id);

        // TODO: Implement via IGenerationManager
        return NoContent();
    }
}
