using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Domain.DTOs.Requests;

public record CreateGenerationRequest
{
    public required string Text { get; init; }
    public required Guid VoiceId { get; init; }
    public RoutingPreference RoutingPreference { get; init; } = RoutingPreference.Balanced;
    public string? AudioFormat { get; init; } = "mp3";
    public string? CallbackUrl { get; init; }
    public VoicePreset? Preset { get; init; }
}
