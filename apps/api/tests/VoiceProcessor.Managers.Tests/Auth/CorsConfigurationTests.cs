using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace VoiceProcessor.Managers.Tests.Auth;

public class CorsConfigurationTests
{
    [Fact]
    public async Task ConfiguredOrigin_ReturnsAccessControlAllowOriginHeader()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        
        client.DefaultRequestHeaders.Add("Origin", "https://voiceprocessor.com");
        
        var response = await client.GetAsync("/health");
        
        response.Headers.Should().Contain(h => 
            h.Key == "Access-Control-Allow-Origin" && 
            h.Value.Contains("https://voiceprocessor.com"));
    }

    [Fact]
    public async Task EnvironmentVariableOverride_ReturnsCorrectOrigin()
    {
        var productionOrigin = "https://voiceprocessor.vercel.app";
        
        await using var factory = new CustomWebApplicationFactoryWithOrigin(productionOrigin);
        var client = factory.CreateClient();
        
        client.DefaultRequestHeaders.Add("Origin", productionOrigin);
        
        var response = await client.GetAsync("/health");
        
        response.Headers.Should().Contain(h => 
            h.Key == "Access-Control-Allow-Origin" && 
            h.Value.Contains(productionOrigin));
    }
}

public class CustomWebApplicationFactoryWithOrigin : CustomWebApplicationFactory
{
    private readonly string _allowedOrigin;

    public CustomWebApplicationFactoryWithOrigin(string allowedOrigin)
    {
        _allowedOrigin = allowedOrigin;
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = _allowedOrigin
            });
        });
    }
}
