using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Domain.DTOs.Requests;

public record EstimateCostRequest
{
    public required string Text { get; init; }
    public Guid? VoiceId { get; init; }
    public Provider? Provider { get; init; }
    public RoutingPreference RoutingPreference { get; init; } = RoutingPreference.Balanced;
}
