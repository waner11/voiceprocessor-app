namespace VoiceProcessor.Domain.Entities;

public sealed class CreditDeduction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid IdempotencyKey { get; set; }
    public Guid? GenerationId { get; set; }
    public int Credits { get; set; }
    public DateTime CreatedAt { get; set; }
}
