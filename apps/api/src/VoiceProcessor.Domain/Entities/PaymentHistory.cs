namespace VoiceProcessor.Domain.Entities;

public class PaymentHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string StripeSessionId { get; set; } = string.Empty;
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public int CreditsAdded { get; set; }
    public string PackId { get; set; } = string.Empty;
    public string PackName { get; set; } = string.Empty;
    public string Status { get; set; } = "completed";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
}
