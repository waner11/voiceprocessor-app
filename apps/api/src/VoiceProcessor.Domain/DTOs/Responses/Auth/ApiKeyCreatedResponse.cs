namespace VoiceProcessor.Domain.DTOs.Responses.Auth;

public record ApiKeyCreatedResponse : ApiKeyResponse
{
    public required string ApiKey { get; init; }
}
