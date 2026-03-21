using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Sentry;
using VoiceProcessor.Clients.Api.Middleware;
using VoiceProcessor.Clients.Api.Services;

namespace VoiceProcessor.Managers.Tests.Middleware;

[Collection("SentryTests")]
public class SentryUserContextMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_AuthenticatedUser_ConfiguresSentryScopeWithUserData()
    {
        // Arrange
        using var sentryHandle = SentrySdk.Init(options =>
        {
            options.Dsn = "https://examplePublicKey@o0.ingest.sentry.io/0";
            options.IsGlobalModeEnabled = true;
        });

        var userId = Guid.NewGuid();
        var mockCurrentUser = new Mock<ICurrentUserService>();
        mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        mockCurrentUser.Setup(x => x.Email).Returns("user@example.com");
        mockCurrentUser.Setup(x => x.Tier).Returns("free");
        mockCurrentUser.Setup(x => x.AuthMethod).Returns("password");

        var services = new ServiceCollection();
        services.AddSingleton(mockCurrentUser.Object);
        var sp = services.BuildServiceProvider();

        var context = new DefaultHttpContext { RequestServices = sp };
        var nextCalled = false;
        var middleware = new SentryUserContextMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();

        SentryUser? capturedUser = null;
        SentrySdk.ConfigureScope(scope => capturedUser = scope.User);

        capturedUser.Should().NotBeNull("scope.User should be set for authenticated users");
        capturedUser!.Id.Should().Be(userId.ToString());
        capturedUser.Email.Should().Be("user@example.com");
        capturedUser.Other.Should().Contain("Tier", "free");
        capturedUser.Other.Should().Contain("AuthMethod", "password");
    }

    [Fact]
    public async Task InvokeAsync_UnauthenticatedUser_DoesNotConfigureSentryScope()
    {
        // Arrange
        using var sentryHandle = SentrySdk.Init(options =>
        {
            options.Dsn = "https://examplePublicKey@o0.ingest.sentry.io/0";
            options.IsGlobalModeEnabled = true;
        });

        var mockCurrentUser = new Mock<ICurrentUserService>();
        mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

        var services = new ServiceCollection();
        services.AddSingleton(mockCurrentUser.Object);
        var sp = services.BuildServiceProvider();

        var context = new DefaultHttpContext { RequestServices = sp };
        var nextCalled = false;
        var middleware = new SentryUserContextMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();

        SentryUser? capturedUser = null;
        SentrySdk.ConfigureScope(scope => capturedUser = scope.User);

        capturedUser.Should().BeNull("scope.User must not be set for unauthenticated requests");
    }
}

[Collection("SentryTests")]
public class SentryGracefulDegradationTests
{
    [Fact]
    public async Task AppStarts_WhenSentryDsnIsEmptyString_HealthCheckReturns200()
    {
        // Arrange
        await using var factory = new SentryEmptyDsnWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK,
            "app must start and serve /health even when Sentry:Dsn is an empty string");
    }

    [Fact]
    public async Task AppStarts_WhenSentrySectionMissing_HealthCheckReturns200()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK,
            "app must start and serve /health even when the Sentry config section is entirely missing");
    }
}

public sealed class SentryEmptyDsnWebApplicationFactory : CustomWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Sentry:Dsn"] = ""
            });
        });
    }
}
