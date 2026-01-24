using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Domain.DTOs.Responses.Payments;

namespace VoiceProcessor.Accessors.Payments;

public class StripeAccessor : IStripeAccessor
{
    private readonly StripeOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<StripeAccessor> _logger;

    private const string CreditPacksCacheKey = "stripe_credit_packs";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    private const string CreditsMetadataKey = "credits";

    public StripeAccessor(
        IOptions<StripeOptions> options,
        IMemoryCache cache,
        ILogger<StripeAccessor> logger)
    {
        _options = options.Value;
        _cache = cache;
        _logger = logger;

        StripeConfiguration.ApiKey = _options.SecretKey;
    }

    public async Task<IReadOnlyList<CreditPackResponse>> GetCreditPacksAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CreditPacksCacheKey, out IReadOnlyList<CreditPackResponse>? cached) && cached is not null)
        {
            _logger.LogDebug("Returning cached credit packs");
            return cached;
        }

        _logger.LogInformation("Fetching credit packs from Stripe");

        try
        {
            var priceService = new PriceService();
            var prices = await priceService.ListAsync(
                new PriceListOptions
                {
                    Active = true,
                    Type = "one_time",
                    Expand = ["data.product"]
                },
                cancellationToken: cancellationToken);

            var packs = new List<CreditPackResponse>();

            foreach (var price in prices.Data)
            {
                var product = price.Product;
                if (product is null || !product.Active)
                    continue;

                // Check if product has credits metadata
                if (!product.Metadata.TryGetValue(CreditsMetadataKey, out var creditsStr) ||
                    !int.TryParse(creditsStr, out var credits))
                {
                    _logger.LogDebug(
                        "Skipping product {ProductId} - no valid credits metadata",
                        product.Id);
                    continue;
                }

                packs.Add(new CreditPackResponse
                {
                    PriceId = price.Id,
                    ProductId = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Credits = credits,
                    PriceAmount = price.UnitAmount.HasValue
                        ? price.UnitAmount.Value / 100m
                        : 0,
                    Currency = price.Currency
                });
            }

            // Sort by credits ascending
            var sortedPacks = packs.OrderBy(p => p.Credits).ToList();

            _cache.Set(CreditPacksCacheKey, (IReadOnlyList<CreditPackResponse>)sortedPacks, CacheDuration);

            _logger.LogInformation(
                "Fetched {PackCount} credit packs from Stripe",
                sortedPacks.Count);

            return sortedPacks;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to fetch credit packs from Stripe");
            throw;
        }
    }

    public async Task<StripeCheckoutResult> CreateCheckoutSessionAsync(
        Guid userId,
        string priceId,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating checkout session for user {UserId} with price {PriceId}",
            userId, priceId);

        try
        {
            // Fetch the price to get product metadata
            var priceService = new PriceService();
            var price = await priceService.GetAsync(
                priceId,
                new PriceGetOptions { Expand = ["product"] },
                cancellationToken: cancellationToken);

            var product = price.Product;
            if (product is null)
            {
                return new StripeCheckoutResult
                {
                    Success = false,
                    ErrorMessage = "Product not found for price"
                };
            }

            if (!product.Metadata.TryGetValue(CreditsMetadataKey, out var creditsStr) ||
                !int.TryParse(creditsStr, out var credits))
            {
                return new StripeCheckoutResult
                {
                    Success = false,
                    ErrorMessage = "Product does not have valid credits metadata"
                };
            }

            var sessionService = new SessionService();
            var session = await sessionService.CreateAsync(
                new SessionCreateOptions
                {
                    Mode = "payment",
                    LineItems =
                    [
                        new SessionLineItemOptions
                        {
                            Price = priceId,
                            Quantity = 1
                        }
                    ],
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        ["user_id"] = userId.ToString(),
                        ["price_id"] = priceId,
                        ["credits"] = credits.ToString(),
                        ["pack_name"] = product.Name
                    }
                },
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Created checkout session {SessionId} for user {UserId}",
                session.Id, userId);

            return new StripeCheckoutResult
            {
                Success = true,
                SessionId = session.Id,
                CheckoutUrl = session.Url
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex,
                "Failed to create checkout session for user {UserId}",
                userId);

            return new StripeCheckoutResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public StripeWebhookResult ParseWebhook(string json, string signature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                signature,
                _options.WebhookSecret);

            _logger.LogInformation(
                "Received Stripe webhook event {EventType} ({EventId})",
                stripeEvent.Type, stripeEvent.Id);

            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                if (session is null)
                {
                    return new StripeWebhookResult
                    {
                        Success = false,
                        ErrorMessage = "Failed to parse checkout session from event"
                    };
                }

                // Extract metadata
                var metadata = session.Metadata;
                if (!metadata.TryGetValue("user_id", out var userIdStr) ||
                    !Guid.TryParse(userIdStr, out var userId))
                {
                    return new StripeWebhookResult
                    {
                        Success = false,
                        ErrorMessage = "Missing or invalid user_id in session metadata"
                    };
                }

                if (!metadata.TryGetValue("credits", out var creditsStr) ||
                    !int.TryParse(creditsStr, out var credits))
                {
                    return new StripeWebhookResult
                    {
                        Success = false,
                        ErrorMessage = "Missing or invalid credits in session metadata"
                    };
                }

                metadata.TryGetValue("price_id", out var priceId);
                metadata.TryGetValue("pack_name", out var packName);

                return new StripeWebhookResult
                {
                    Success = true,
                    EventType = stripeEvent.Type,
                    CheckoutData = new StripeCheckoutData
                    {
                        SessionId = session.Id,
                        PaymentIntentId = session.PaymentIntentId,
                        UserId = userId,
                        PriceId = priceId ?? string.Empty,
                        Credits = credits,
                        PackName = packName ?? string.Empty,
                        Amount = session.AmountTotal.HasValue
                            ? session.AmountTotal.Value / 100m
                            : 0,
                        Currency = session.Currency ?? "usd"
                    }
                };
            }

            // Other event types - just acknowledge
            return new StripeWebhookResult
            {
                Success = true,
                EventType = stripeEvent.Type
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to parse Stripe webhook");
            return new StripeWebhookResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
