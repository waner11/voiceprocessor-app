using Microsoft.AspNetCore.Mvc;
using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Clients.Api.Controllers;

public class VoicesController : ApiControllerBase
{
    private readonly IVoiceManager _voiceManager;
    private readonly ILogger<VoicesController> _logger;

    public VoicesController(
        IVoiceManager voiceManager,
        ILogger<VoicesController> logger)
    {
        _voiceManager = voiceManager;
        _logger = logger;
    }

    /// <summary>
    /// Get all available voices
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<VoiceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<VoiceResponse>>> GetVoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] Provider? provider = null,
        [FromQuery] string? language = null,
        [FromQuery] string? gender = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting voices, provider {Provider}, language {Language}, gender {Gender}",
            provider, language, gender);

        var response = await _voiceManager.GetVoicesAsync(
            page, pageSize, provider, language, gender, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Get a specific voice by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VoiceResponse>> GetVoice(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting voice {VoiceId}", id);

        var response = await _voiceManager.GetVoiceAsync(id, cancellationToken);
        if (response is null)
        {
            return NotFound(new ErrorResponse { Code = "VOICE_NOT_FOUND", Message = $"Voice {id} not found" });
        }

        return Ok(response);
    }

    /// <summary>
    /// Get voices grouped by provider
    /// </summary>
    [HttpGet("by-provider")]
    [ProducesResponseType(typeof(Dictionary<Provider, IReadOnlyList<VoiceResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<Provider, IReadOnlyList<VoiceResponse>>>> GetVoicesByProvider(
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting voices grouped by provider");

        var response = await _voiceManager.GetVoicesByProviderAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Refresh voice catalog from all providers
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RefreshVoices(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Voice catalog refresh requested");

        // Note: In production, this should trigger a background job
        // For now, we'll do it inline but return 202 Accepted
        _ = Task.Run(async () =>
        {
            try
            {
                await _voiceManager.RefreshVoiceCatalogAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Voice catalog refresh failed");
            }
        });

        return Accepted();
    }
}
