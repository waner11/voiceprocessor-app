namespace VoiceProcessor.Domain.DTOs.Responses.Auth;

public class OAuthUrlResponse
{
    public required string AuthorizationUrl { get; init; }
    public required string State { get; init; }
}
