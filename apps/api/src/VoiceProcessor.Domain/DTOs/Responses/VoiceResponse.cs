using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Domain.DTOs.Responses;

public record VoiceResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required Provider Provider { get; init; }
    public string? Language { get; init; }
    public string? Accent { get; init; }
    public string? Gender { get; init; }
    public string? AgeGroup { get; init; }
    public string? UseCase { get; init; }
    public string? PreviewUrl { get; init; }
    public required decimal CostPerThousandChars { get; init; }
}
