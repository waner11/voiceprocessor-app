using VoiceProcessor.Domain.DTOs.Responses.Payments;

namespace VoiceProcessor.Accessors.Contracts;

public interface IStripeAccessor
{
    /// <summary>
    /// Fetches available credit packs from Stripe Products with metadata.
    /// Results are cached for 1 hour.
    /// </summary>
    Task<IReadOnlyList<CreditPackResponse>> GetCreditPacksAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Stripe Checkout Session for purchasing credits.
    /// </summary>
    Task<StripeCheckoutResult> CreateCheckoutSessionAsync(
        Guid userId,
        string priceId,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses and validates a Stripe webhook event.
    /// </summary>
    StripeWebhookResult ParseWebhook(string json, string signature);
}

public record StripeCheckoutResult
{
    public required bool Success { get; init; }
    public string? SessionId { get; init; }
    public string? CheckoutUrl { get; init; }
    public string? ErrorMessage { get; init; }
}

public record StripeWebhookResult
{
    public required bool Success { get; init; }
    public string? EventType { get; init; }
    public StripeCheckoutData? CheckoutData { get; init; }
    public string? ErrorMessage { get; init; }
}

public record StripeCheckoutData
{
    public required string SessionId { get; init; }
    public string? PaymentIntentId { get; init; }
    public required Guid UserId { get; init; }
    public required string PriceId { get; init; }
    public required int Credits { get; init; }
    public required string PackName { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
}
