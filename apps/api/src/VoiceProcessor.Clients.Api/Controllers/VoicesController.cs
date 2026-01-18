using Microsoft.AspNetCore.Mvc;
using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Clients.Api.Controllers;

public class VoicesController : ApiControllerBase
{
    private readonly ILogger<VoicesController> _logger;

    public VoicesController(ILogger<VoicesController> logger)
    {
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

        // TODO: Implement via IVoiceManager
        var response = new PagedResponse<VoiceResponse>
        {
            Items = [],
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };

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

        // TODO: Implement via IVoiceManager
        var response = new VoiceResponse
        {
            Id = id,
            Name = "Sample Voice",
            Description = "A sample voice for testing",
            Provider = Provider.ElevenLabs,
            Language = "en",
            Accent = "American",
            Gender = "Female",
            AgeGroup = "Adult",
            UseCase = "Narration",
            PreviewUrl = "https://example.com/preview.mp3",
            CostPerThousandChars = 0.30m
        };

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

        // TODO: Implement via IVoiceManager
        var response = new Dictionary<Provider, IReadOnlyList<VoiceResponse>>();

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

        // TODO: Implement via IVoiceManager - trigger background job
        return Accepted();
    }
}
