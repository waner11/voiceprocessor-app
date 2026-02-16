namespace VoiceProcessor.Clients.Api.Extensions;

/// <summary>
/// Extension methods for setting and clearing authentication cookies.
/// </summary>
public static class AuthCookieExtensions
{
    /// <summary>
    /// Cookie name for JWT access token.
    /// </summary>
    public const string AccessTokenCookieName = "vp_access_token";

    /// <summary>
    /// Cookie name for JWT refresh token.
    /// </summary>
    public const string RefreshTokenCookieName = "vp_refresh_token";

    private const string RefreshTokenCookiePath = "/api/v1/Auth";

    private static CookieOptions CreateAccessCookieOptions(bool isDevelopment, int expiryMinutes)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
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
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
            Secure = !isDevelopment,
            MaxAge = TimeSpan.FromDays(expiryDays),
            Path = RefreshTokenCookiePath,
            IsEssential = true
        };
    }

    private static CookieOptions CreateDeleteCookieOptions(string path, bool isDevelopment)
    {
        return new CookieOptions
        {
            Path = path,
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
            Secure = !isDevelopment
        };
    }

    /// <summary>
    /// Sets HttpOnly authentication cookies for access and refresh tokens.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="accessToken">The JWT access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="accessTokenExpiryMinutes">Access token expiry in minutes.</param>
    /// <param name="refreshTokenExpiryDays">Refresh token expiry in days.</param>
    /// <param name="isDevelopment">Whether the app is running in development mode.</param>
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

    /// <summary>
    /// Clears authentication cookies.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="isDevelopment">Whether the app is running in development mode.</param>
    public static void ClearAuthCookies(this HttpResponse response, bool isDevelopment)
    {
        response.Cookies.Delete(AccessTokenCookieName, CreateDeleteCookieOptions("/", isDevelopment));
        response.Cookies.Delete(RefreshTokenCookieName, CreateDeleteCookieOptions(RefreshTokenCookiePath, isDevelopment));
    }
}
