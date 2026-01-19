namespace VoiceProcessor.Engines.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "VoiceProcessor";
    public string Audience { get; set; } = "VoiceProcessor";
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
