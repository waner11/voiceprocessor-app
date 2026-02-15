namespace VoiceProcessor.Clients.Api.Extensions;

public static class AuthCookieExtensions
{
    public const string AccessTokenCookieName = "vp_access_token";

    public const string RefreshTokenCookieName = "vp_refresh_token";

    private const string RefreshTokenCookiePath = "/api/v1/Auth";

    private static CookieOptions CreateAccessCookieOptions(bool isDevelopment, int expiryMinutes)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = !isDevelopment,
            MaxAge = TimeSpan.FromMinutes(expiryMinutes),
            Path = "/",
            IsEssential = true
        };
    }

    private static CookieOptions CreateRefreshCookieOptions(bool isDevelopment, int expiryDays)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = !isDevelopment,
            MaxAge = TimeSpan.FromDays(expiryDays),
            Path = RefreshTokenCookiePath,
            IsEssential = true
        };
    }

    private static CookieOptions CreateDeleteCookieOptions(string path)
    {
        return new CookieOptions
        {
            Path = path,
            SameSite = SameSiteMode.Lax
        };
    }

    public static void SetAuthCookies(
        this HttpResponse response,
        string accessToken,
        string refreshToken,
        int accessTokenExpiryMinutes,
        int refreshTokenExpiryDays,
        bool isDevelopment)
    {
        var accessCookieOptions = CreateAccessCookieOptions(isDevelopment, accessTokenExpiryMinutes);
        var refreshCookieOptions = CreateRefreshCookieOptions(isDevelopment, refreshTokenExpiryDays);

        response.Cookies.Append(AccessTokenCookieName, accessToken, accessCookieOptions);
        response.Cookies.Append(RefreshTokenCookieName, refreshToken, refreshCookieOptions);
    }

    public static void ClearAuthCookies(this HttpResponse response)
    {
        response.Cookies.Delete(AccessTokenCookieName, CreateDeleteCookieOptions("/"));
        response.Cookies.Delete(RefreshTokenCookieName, CreateDeleteCookieOptions(RefreshTokenCookiePath));
    }
}
