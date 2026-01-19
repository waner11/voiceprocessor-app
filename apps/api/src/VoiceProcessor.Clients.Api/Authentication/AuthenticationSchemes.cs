namespace VoiceProcessor.Clients.Api.Authentication;

public static class AuthenticationSchemes
{
    public const string JwtBearer = "Bearer";
    public const string ApiKey = "ApiKey";
    public const string JwtOrApiKey = "JwtOrApiKey";

    public const string ApiKeyHeaderName = "X-API-Key";
}
