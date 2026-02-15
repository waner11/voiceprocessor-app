using FluentAssertions;
using Microsoft.AspNetCore.Http;
using VoiceProcessor.Clients.Api.Extensions;

namespace VoiceProcessor.Managers.Tests.Auth;

public class AuthCookiePathTests
{
    [Fact]
    public void SetAuthCookies_RefreshTokenCookie_HasCorrectPath()
    {
        var httpContext = new DefaultHttpContext();
        var response = httpContext.Response;
        
        response.SetAuthCookies(
            accessToken: "test-access-token",
            refreshToken: "test-refresh-token",
            accessTokenExpiryMinutes: 15,
            refreshTokenExpiryDays: 7,
            isDevelopment: true
        );
        
        var cookies = response.Headers.SetCookie.ToString();
        
        cookies.Should().Contain("vp_refresh_token=test-refresh-token");
        cookies.Should().Contain("path=/api/v1/Auth", "refresh token cookie path must match AuthController route with capital A");
    }

    [Fact]
    public void SetAuthCookies_AccessTokenCookie_HasRootPath()
    {
        var httpContext = new DefaultHttpContext();
        var response = httpContext.Response;
        
        response.SetAuthCookies(
            accessToken: "test-access-token",
            refreshToken: "test-refresh-token",
            accessTokenExpiryMinutes: 15,
            refreshTokenExpiryDays: 7,
            isDevelopment: true
        );
        
        var cookies = response.Headers.SetCookie.ToString();
        
        cookies.Should().Contain("vp_access_token=test-access-token");
        cookies.Should().Contain("path=/", "access token cookie should be available for all paths");
    }
}
