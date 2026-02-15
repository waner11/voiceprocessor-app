using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace VoiceProcessor.Managers.Tests.Auth;

public class CorsConfigurationTests
{
    [Fact]
    public async Task CorsPolicy_ShouldReadFromConfiguration()
    {
        var testOrigin = "https://test.example.com";
        
        var configData = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = testOrigin
        };

        var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(configData);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        var allowedOrigins = context.Configuration
                            .GetSection("Cors:AllowedOrigins")
                            .Get<string[]>() ?? new[] { "http://localhost:3000" };

                        services.AddCors(options =>
                        {
                            options.AddPolicy("Frontend", policy =>
                            {
                                policy.WithOrigins(allowedOrigins)
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials();
                            });
                        });
                    })
                    .Configure(app =>
                    {
                        app.UseCors("Frontend");
                        app.UseRouting();
                    });
            })
            .StartAsync();

        var server = host.GetTestServer();
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Add("Origin", testOrigin);

        var response = await client.GetAsync("/");

        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin").Should().Contain(testOrigin);
    }

    [Fact]
    public async Task CorsPolicy_ShouldSupportEnvironmentVariableOverride()
    {
        var productionOrigin = "https://voiceprocessor.vercel.app";
        
        var configData = new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "http://localhost:3000",
            ["Cors:AllowedOrigins:1"] = productionOrigin
        };

        var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(configData);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        var allowedOrigins = context.Configuration
                            .GetSection("Cors:AllowedOrigins")
                            .Get<string[]>() ?? new[] { "http://localhost:3000" };

                        services.AddCors(options =>
                        {
                            options.AddPolicy("Frontend", policy =>
                            {
                                policy.WithOrigins(allowedOrigins)
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials();
                            });
                        });
                    })
                    .Configure(app =>
                    {
                        app.UseCors("Frontend");
                        app.UseRouting();
                    });
            })
            .StartAsync();

        var server = host.GetTestServer();
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Add("Origin", productionOrigin);

        var response = await client.GetAsync("/");

        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin").Should().Contain(productionOrigin);
    }
}
