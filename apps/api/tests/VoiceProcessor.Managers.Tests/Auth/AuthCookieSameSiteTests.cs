using FluentAssertions;
using Microsoft.AspNetCore.Http;
using VoiceProcessor.Clients.Api.Extensions;

namespace VoiceProcessor.Managers.Tests.Auth;

public class AuthCookieSameSiteTests
{
    [Fact]
    public void SetAuthCookies_Development_ShouldUseSameSiteLaxAndSecureFalse()
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
        
        // Both cookies should have SameSite=Lax in development
        cookies.Should().Contain("samesite=lax", "development environment should use SameSite=Lax for localhost HTTP");
        
        // Both cookies should NOT have Secure flag in development (HTTP localhost)
        cookies.Should().NotContain("secure", "development environment should not set Secure flag for HTTP localhost");
    }

    [Fact]
    public void SetAuthCookies_Production_ShouldUseSameSiteNoneAndSecureTrue()
    {
        var httpContext = new DefaultHttpContext();
        var response = httpContext.Response;
        
        response.SetAuthCookies(
            accessToken: "test-access-token",
            refreshToken: "test-refresh-token",
            accessTokenExpiryMinutes: 15,
            refreshTokenExpiryDays: 7,
            isDevelopment: false
        );
        
        var cookies = response.Headers.SetCookie.ToString();
        
        // Both cookies should have SameSite=None in production (cross-site cookies)
        cookies.Should().Contain("samesite=none", "production environment should use SameSite=None for cross-origin requests");
        
        // Both cookies should have Secure flag in production (HTTPS required for SameSite=None)
        cookies.Should().Contain("secure", "production environment must set Secure flag when using SameSite=None");
    }
}
