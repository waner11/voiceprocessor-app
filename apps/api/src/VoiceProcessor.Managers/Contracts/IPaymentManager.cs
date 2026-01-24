using VoiceProcessor.Domain.DTOs.Requests.Payments;
using VoiceProcessor.Domain.DTOs.Responses.Payments;

namespace VoiceProcessor.Managers.Contracts;

public interface IPaymentManager
{
    /// <summary>
    /// Gets available credit packs from Stripe.
    /// </summary>
    Task<CreditPacksResponse> GetCreditPacksAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Stripe Checkout session for purchasing credits.
    /// </summary>
    Task<CheckoutResponse> CreateCheckoutAsync(
        Guid userId,
        CreateCheckoutRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles Stripe webhook events.
    /// Idempotent - will not double-credit if same event is received twice.
    /// </summary>
    Task HandleWebhookAsync(
        string payload,
        string signature,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payment history for a user.
    /// </summary>
    Task<PaymentHistoryListResponse> GetPaymentHistoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
