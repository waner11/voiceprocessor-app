namespace VoiceProcessor.Domain.DTOs.Requests.Auth;

public record DeleteAccountRequest
{
    public string? Password { get; init; }
}
