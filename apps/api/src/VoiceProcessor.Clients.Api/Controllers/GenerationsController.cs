using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoiceProcessor.Clients.Api.Services;
using VoiceProcessor.Domain.DTOs.Requests;
using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Clients.Api.Controllers;

public class GenerationsController : ApiControllerBase
{
    private readonly IGenerationManager _generationManager;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GenerationsController> _logger;

    public GenerationsController(
        IGenerationManager generationManager,
        ICurrentUserService currentUser,
        ILogger<GenerationsController> logger)
    {
        _generationManager = generationManager;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Estimate the cost of a text-to-speech generation
    /// </summary>
    [HttpPost("estimate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CostEstimateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CostEstimateResponse>> EstimateCost(
        [FromBody] EstimateCostRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cost estimate requested for {CharCount} characters",
            request.Text.Length);

        var response = await _generationManager.EstimateCostAsync(request, cancellationToken);
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
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        _logger.LogInformation("Generation requested by user {UserId}, {CharCount} characters, voice {VoiceId}",
            userId, request.Text.Length, request.VoiceId);

        try
        {
            var response = await _generationManager.CreateGenerationAsync(userId, request, cancellationToken);
            return AcceptedAtAction(nameof(GetGeneration), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Insufficient credits"))
        {
            return StatusCode(StatusCodes.Status402PaymentRequired,
                new ErrorResponse { Code = "INSUFFICIENT_CREDITS", Message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return BadRequest(new ErrorResponse { Code = "NOT_FOUND", Message = ex.Message });
        }
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

        var response = await _generationManager.GetGenerationAsync(id, cancellationToken);
        if (response is null)
        {
            return NotFound(new ErrorResponse { Code = "GENERATION_NOT_FOUND", Message = $"Generation {id} not found" });
        }

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
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        _logger.LogDebug("Getting generations for user {UserId}, page {Page}, pageSize {PageSize}, status {Status}",
            userId, page, pageSize, status);

        var response = await _generationManager.GetGenerationsAsync(
            userId, page, pageSize, status, cancellationToken);

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
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        _logger.LogInformation("Feedback submitted for generation {GenerationId}, rating {Rating}",
            id, request.Rating);

        try
        {
            await _generationManager.SubmitFeedbackAsync(id, userId, request, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new ErrorResponse { Code = "GENERATION_NOT_FOUND", Message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("does not belong"))
        {
            return NotFound(new ErrorResponse { Code = "GENERATION_NOT_FOUND", Message = $"Generation {id} not found" });
        }
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
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        _logger.LogInformation("Cancellation requested for generation {GenerationId}", id);

        try
        {
            var cancelled = await _generationManager.CancelGenerationAsync(id, userId, cancellationToken);
            if (!cancelled)
            {
                return Conflict(new ErrorResponse
                {
                    Code = "CANCEL_FAILED",
                    Message = "Generation cannot be cancelled (already completed, failed, or not found)"
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("does not belong"))
        {
            return NotFound(new ErrorResponse { Code = "GENERATION_NOT_FOUND", Message = $"Generation {id} not found" });
        }
    }
}
