using FluentAssertions;
using Microsoft.AspNetCore.Http;
using VoiceProcessor.Clients.Api.Extensions;

namespace VoiceProcessor.Managers.Tests.Auth;

public class ClearAuthCookiesTests
{
    [Fact]
    public void ClearAuthCookies_AccessTokenCookie_HasMatchingOptions()
    {
        var httpContext = new DefaultHttpContext();
        var response = httpContext.Response;
        
        response.ClearAuthCookies(isDevelopment: true);
        
        var cookies = response.Headers.SetCookie.ToString();
        
        cookies.Should().Contain("vp_access_token=;");
        cookies.Should().Contain("path=/", "access token delete cookie must have same path as set cookie");
        cookies.Should().Contain("samesite=lax", "access token delete cookie must have same samesite as set cookie");
        cookies.Should().MatchRegex("max-age=0|expires=", "access token delete cookie must expire immediately");
    }

    [Fact]
    public void ClearAuthCookies_RefreshTokenCookie_HasMatchingOptions()
    {
        var httpContext = new DefaultHttpContext();
        var response = httpContext.Response;
        
        response.ClearAuthCookies(isDevelopment: true);
        
        var cookies = response.Headers.SetCookie.ToString();
        
        cookies.Should().Contain("vp_refresh_token=;");
        cookies.Should().Contain("path=/api/v1/Auth", "refresh token delete cookie must have same path as set cookie");
        cookies.Should().Contain("samesite=lax", "refresh token delete cookie must have same samesite as set cookie");
        cookies.Should().MatchRegex("max-age=0|expires=", "refresh token delete cookie must expire immediately");
    }

    [Fact]
    public void ClearAuthCookies_Production_HasMatchingSameSiteNoneAndSecure()
    {
        var httpContext = new DefaultHttpContext();
        var response = httpContext.Response;
        
        response.ClearAuthCookies(isDevelopment: false);
        
        var cookies = response.Headers.SetCookie.ToString();
        
        cookies.Should().Contain("samesite=none");
        cookies.Should().Contain("secure");
    }
}
