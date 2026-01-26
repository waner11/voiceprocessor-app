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
        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = !isDevelopment,
            MaxAge = TimeSpan.FromMinutes(accessTokenExpiryMinutes),
            Path = "/",
            IsEssential = true
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = !isDevelopment,
            MaxAge = TimeSpan.FromDays(refreshTokenExpiryDays),
            Path = "/api/v1/auth",
            IsEssential = true
        };

        response.Cookies.Append(AccessTokenCookieName, accessToken, accessCookieOptions);
        response.Cookies.Append(RefreshTokenCookieName, refreshToken, refreshCookieOptions);
    }

    /// <summary>
    /// Clears authentication cookies.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    public static void ClearAuthCookies(this HttpResponse response)
    {
        response.Cookies.Delete(AccessTokenCookieName);
        response.Cookies.Delete(RefreshTokenCookieName);
    }
}
