using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoiceProcessor.Clients.Api.Services;
using VoiceProcessor.Domain.DTOs.Requests.Payments;
using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Domain.DTOs.Responses.Payments;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Clients.Api.Controllers;

public class PaymentsController : ApiControllerBase
{
    private readonly IPaymentManager _paymentManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentManager paymentManager,
        ICurrentUserService currentUserService,
        ILogger<PaymentsController> logger)
    {
        _paymentManager = paymentManager;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get available credit packs for purchase
    /// </summary>
    [HttpGet("packs")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CreditPacksResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreditPacksResponse>> GetCreditPacks(
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting credit packs");

        var response = await _paymentManager.GetCreditPacksAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Create a Stripe Checkout session for purchasing credits
    /// </summary>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(CheckoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CheckoutResponse>> CreateCheckout(
        [FromBody] CreateCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        _logger.LogInformation(
            "Creating checkout session for user {UserId} with price {PriceId}",
            userId, request.PriceId);

        try
        {
            var response = await _paymentManager.CreateCheckoutAsync(
                userId.Value,
                request,
                cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex,
                "Failed to create checkout for user {UserId}",
                userId);

            return BadRequest(new ErrorResponse
            {
                Code = "CHECKOUT_FAILED",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get payment history for the current user
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(PaymentHistoryListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaymentHistoryListResponse>> GetPaymentHistory(
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        _logger.LogDebug("Getting payment history for user {UserId}", userId);

        var response = await _paymentManager.GetPaymentHistoryAsync(
            userId.Value,
            cancellationToken);

        return Ok(response);
    }
}
