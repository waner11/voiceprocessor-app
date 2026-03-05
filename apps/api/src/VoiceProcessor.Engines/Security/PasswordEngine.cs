using System.Security.Cryptography;
using System.Text;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines.Security;

public class PasswordEngine : IPasswordEngine
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);

        var computedHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, computedHash);
    }

    public string GenerateResetToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var base64 = Convert.ToBase64String(bytes);
        // Convert to URL-safe Base64: replace + with -, / with _, remove =
        return base64.Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    public string HashToken(string rawToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
