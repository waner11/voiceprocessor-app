using System.Text.Json;
using FluentAssertions;
using VoiceProcessor.Domain.DTOs.Requests.Auth;

namespace VoiceProcessor.Managers.Tests.Auth;

public class RefreshTokenCookieFallbackTests
{
    [Fact]
    public void RefreshTokenRequest_CanDeserializeEmptyJson()
    {
        var json = "{}";
        
        var act = () => JsonSerializer.Deserialize<RefreshTokenRequest>(json);
        
        act.Should().NotThrow<JsonException>("empty JSON should be deserializable when RefreshToken is nullable");
        
        var result = act();
        result.Should().NotBeNull();
        result!.RefreshToken.Should().BeNull();
    }

    [Fact]
    public void RefreshTokenRequest_CanDeserializeNullRefreshToken()
    {
        var json = "{\"RefreshToken\":null}";
        
        var result = JsonSerializer.Deserialize<RefreshTokenRequest>(json);
        
        result.Should().NotBeNull();
        result!.RefreshToken.Should().BeNull();
    }

    [Fact]
    public void RefreshTokenRequest_CanDeserializeWithRefreshToken()
    {
        var json = "{\"RefreshToken\":\"test-token\"}";
        
        var result = JsonSerializer.Deserialize<RefreshTokenRequest>(json);
        
        result.Should().NotBeNull();
        result!.RefreshToken.Should().Be("test-token");
    }
}
