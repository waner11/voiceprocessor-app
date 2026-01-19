namespace VoiceProcessor.Domain.DTOs.Requests.Auth;

public record RegisterRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? Name { get; init; }
}
