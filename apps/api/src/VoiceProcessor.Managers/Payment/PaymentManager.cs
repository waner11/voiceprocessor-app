using Microsoft.Extensions.Logging;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Domain.DTOs.Requests.Payments;
using VoiceProcessor.Domain.DTOs.Responses.Payments;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Managers.Payment;

public class PaymentManager : IPaymentManager
{
    private readonly IStripeAccessor _stripeAccessor;
    private readonly IPaymentHistoryAccessor _paymentHistoryAccessor;
    private readonly IUserAccessor _userAccessor;
    private readonly ILogger<PaymentManager> _logger;

    public PaymentManager(
        IStripeAccessor stripeAccessor,
        IPaymentHistoryAccessor paymentHistoryAccessor,
        IUserAccessor userAccessor,
        ILogger<PaymentManager> logger)
    {
        _stripeAccessor = stripeAccessor;
        _paymentHistoryAccessor = paymentHistoryAccessor;
        _userAccessor = userAccessor;
        _logger = logger;
    }

    public async Task<CreditPacksResponse> GetCreditPacksAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching credit packs");

        var packs = await _stripeAccessor.GetCreditPacksAsync(cancellationToken);

        return new CreditPacksResponse { Packs = packs };
    }

    public async Task<CheckoutResponse> CreateCheckoutAsync(
        Guid userId,
        CreateCheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating checkout session for user {UserId} with price {PriceId}",
            userId, request.PriceId);

        // Verify user exists
        var user = await _userAccessor.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        // Create Stripe checkout session
        var result = await _stripeAccessor.CreateCheckoutSessionAsync(
            userId,
            request.PriceId,
            request.SuccessUrl,
            request.CancelUrl,
            cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning(
                "Failed to create checkout session for user {UserId}: {Error}",
                userId, result.ErrorMessage);

            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to create checkout session");
        }

        _logger.LogInformation(
            "Created checkout session {SessionId} for user {UserId}",
            result.SessionId, userId);

        return new CheckoutResponse
        {
            CheckoutUrl = result.CheckoutUrl!,
            SessionId = result.SessionId!
        };
    }

    public async Task HandleWebhookAsync(
        string payload,
        string signature,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing Stripe webhook");

        // Parse and validate the webhook
        var result = _stripeAccessor.ParseWebhook(payload, signature);

        if (!result.Success)
        {
            _logger.LogWarning("Webhook validation failed: {Error}", result.ErrorMessage);
            throw new InvalidOperationException(result.ErrorMessage ?? "Invalid webhook");
        }

        _logger.LogInformation("Received webhook event: {EventType}", result.EventType);

        // Handle checkout.session.completed event
        if (result.EventType == "checkout.session.completed" && result.CheckoutData is not null)
        {
            await HandleCheckoutCompletedAsync(result.CheckoutData, cancellationToken);
        }
    }

    private async Task HandleCheckoutCompletedAsync(
        StripeCheckoutData checkoutData,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing completed checkout {SessionId} for user {UserId}",
            checkoutData.SessionId, checkoutData.UserId);

        // Idempotency check - see if we already processed this session
        var existingPayment = await _paymentHistoryAccessor.GetByStripeSessionIdAsync(
            checkoutData.SessionId,
            cancellationToken);

        if (existingPayment is not null)
        {
            _logger.LogWarning(
                "Checkout session {SessionId} already processed, skipping",
                checkoutData.SessionId);
            return;
        }

        // Add credits to user
        await _userAccessor.AddCreditsAsync(
            checkoutData.UserId,
            checkoutData.Credits,
            cancellationToken);

        _logger.LogInformation(
            "Added {Credits} credits to user {UserId}",
            checkoutData.Credits, checkoutData.UserId);

        // Record the payment
        var payment = new PaymentHistory
        {
            Id = Guid.NewGuid(),
            UserId = checkoutData.UserId,
            StripeSessionId = checkoutData.SessionId,
            StripePaymentIntentId = checkoutData.PaymentIntentId ?? string.Empty,
            Amount = checkoutData.Amount,
            Currency = checkoutData.Currency,
            CreditsAdded = checkoutData.Credits,
            PackId = checkoutData.PriceId,
            PackName = checkoutData.PackName,
            Status = "completed",
            CreatedAt = DateTime.UtcNow
        };

        await _paymentHistoryAccessor.CreateAsync(payment, cancellationToken);

        _logger.LogInformation(
            "Recorded payment {PaymentId} for user {UserId}: {Credits} credits for ${Amount}",
            payment.Id, checkoutData.UserId, checkoutData.Credits, checkoutData.Amount);
    }

    public async Task<PaymentHistoryListResponse> GetPaymentHistoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching payment history for user {UserId}", userId);

        var payments = await _paymentHistoryAccessor.GetByUserIdAsync(userId, 50, cancellationToken);

        var response = payments.Select(p => new PaymentHistoryResponse
        {
            Id = p.Id,
            Amount = p.Amount,
            Currency = p.Currency,
            CreditsAdded = p.CreditsAdded,
            PackName = p.PackName,
            Status = p.Status,
            CreatedAt = p.CreatedAt
        }).ToList();

        return new PaymentHistoryListResponse { Payments = response };
    }
}
