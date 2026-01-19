namespace VoiceProcessor.Engines.Contracts;

public interface IApiKeyEngine
{
    ApiKeyGenerationResult GenerateApiKey();
    string HashApiKey(string apiKey);
    bool VerifyApiKey(string apiKey, string hash);
    string ExtractPrefix(string apiKey);
}

public record ApiKeyGenerationResult
{
    public required string FullKey { get; init; }
    public required string Prefix { get; init; }
    public required string Hash { get; init; }
}
