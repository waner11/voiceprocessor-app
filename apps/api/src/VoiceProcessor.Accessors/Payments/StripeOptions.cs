namespace VoiceProcessor.Accessors.Payments;

public class StripeOptions
{
    public const string SectionName = "Stripe";

    public required string SecretKey { get; set; }
    public required string WebhookSecret { get; set; }
}
