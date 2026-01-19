using System.Security.Cryptography;
using System.Text;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines.Security;

public class ApiKeyEngine : IApiKeyEngine
{
    private const string KeyPrefix = "vp_";
    private const int KeyLength = 32;

    public ApiKeyGenerationResult GenerateApiKey()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(KeyLength);
        var base64Key = Convert.ToBase64String(randomBytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")[..KeyLength];

        var fullKey = $"{KeyPrefix}{base64Key}";
        var prefix = fullKey[..11];
        var hash = HashApiKey(fullKey);

        return new ApiKeyGenerationResult
        {
            FullKey = fullKey,
            Prefix = prefix,
            Hash = hash
        };
    }

    public string HashApiKey(string apiKey)
    {
        var bytes = Encoding.UTF8.GetBytes(apiKey);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyApiKey(string apiKey, string hash)
    {
        var computedHash = HashApiKey(apiKey);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(hash));
    }

    public string ExtractPrefix(string apiKey)
    {
        return apiKey.Length >= 11 ? apiKey[..11] : apiKey;
    }
}
