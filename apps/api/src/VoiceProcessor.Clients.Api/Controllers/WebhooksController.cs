using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Clients.Api.Controllers;

[ApiController]
[Route("webhooks")]
[AllowAnonymous]
public class WebhooksController : ControllerBase
{
    private readonly IPaymentManager _paymentManager;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IPaymentManager paymentManager,
        ILogger<WebhooksController> logger)
    {
        _paymentManager = paymentManager;
        _logger = logger;
    }

    /// <summary>
    /// Handle Stripe webhook events
    /// </summary>
    /// <remarks>
    /// This endpoint receives events from Stripe (e.g., checkout.session.completed).
    /// The signature is verified using the Stripe-Signature header.
    /// </remarks>
    [HttpPost("stripe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> HandleStripeWebhook(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received Stripe webhook");

        // Read the raw request body
        string payload;
        using (var reader = new StreamReader(Request.Body))
        {
            payload = await reader.ReadToEndAsync(cancellationToken);
        }

        // Get the Stripe signature header
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Stripe webhook received without signature");
            return BadRequest(new ErrorResponse
            {
                Code = "MISSING_SIGNATURE",
                Message = "Stripe-Signature header is required"
            });
        }

        try
        {
            await _paymentManager.HandleWebhookAsync(payload, signature, cancellationToken);

            _logger.LogInformation("Stripe webhook processed successfully");
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Stripe webhook validation failed");
            return BadRequest(new ErrorResponse
            {
                Code = "WEBHOOK_VALIDATION_FAILED",
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook processing failed");
            return BadRequest(new ErrorResponse
            {
                Code = "WEBHOOK_PROCESSING_FAILED",
                Message = "An error occurred processing the webhook"
            });
        }
    }
}
