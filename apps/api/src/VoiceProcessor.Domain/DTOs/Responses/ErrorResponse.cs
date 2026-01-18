namespace VoiceProcessor.Domain.DTOs.Responses;

public record ErrorResponse
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public string? Detail { get; init; }
    public string? TraceId { get; init; }
    public IReadOnlyList<ValidationError>? ValidationErrors { get; init; }
}

public record ValidationError
{
    public required string Field { get; init; }
    public required string Message { get; init; }
}
