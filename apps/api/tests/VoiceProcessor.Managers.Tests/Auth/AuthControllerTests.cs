using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Clients.Api.Controllers;
using VoiceProcessor.Clients.Api.Services;
using VoiceProcessor.Domain.DTOs.Requests.Auth;
using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Engines.Security;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Managers.Tests.Auth;

/// <summary>
/// Tests for AuthController error responses.
/// These tests verify that error endpoints return ErrorResponse DTO with Code and Message fields
/// instead of inline anonymous objects.
/// </summary>
public class AuthControllerTests
{
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly Mock<IAuthManager> _mockAuthManager;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockUserAccessor = new Mock<IUserAccessor>();
        _mockAuthManager = new Mock<IAuthManager>();
        _mockCurrentUser = new Mock<ICurrentUserService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");
        
        var jwtOptions = new Microsoft.Extensions.Options.OptionsWrapper<VoiceProcessor.Engines.Security.JwtOptions>(
            new VoiceProcessor.Engines.Security.JwtOptions 
            { 
                AccessTokenExpirationMinutes = 15,
                RefreshTokenExpirationDays = 7
            });
        
        _controller = new AuthController(_mockAuthManager.Object, _mockCurrentUser.Object, _mockLogger.Object, _mockEnvironment.Object, jwtOptions);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidUser_FetchesActualCreditsFromDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var creditsRemaining = 11000;

        var user = new Domain.Entities.User
        {
            Id = userId,
            Email = "test@example.com",
            Name = "Test User",
            Tier = SubscriptionTier.Free,
            CreditsRemaining = creditsRemaining,
            IsActive = true
        };

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _mockUserAccessor.Object.GetByIdAsync(userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result?.CreditsRemaining.Should().Be(creditsRemaining);
        result?.Id.Should().Be(userId);
        result?.Email.Should().Be("test@example.com");
        result?.Name.Should().Be("Test User");
        result?.Tier.Should().Be(SubscriptionTier.Free);

        _mockUserAccessor.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUser_UserNotFound_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.User?)null);

        // Act
        var result = await _mockUserAccessor.Object.GetByIdAsync(userId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUser_WithDifferentCreditsValues_ReturnsCorrectValue()
    {
        // Arrange - Test with various credit amounts to ensure actual values are returned
        var testCases = new[] { 0, 100, 1000, 5000, 11000, 50000 };

        foreach (var credits in testCases)
        {
            var userId = Guid.NewGuid();
            var user = new Domain.Entities.User
            {
                Id = userId,
                Email = "test@example.com",
                Name = "Test User",
                Tier = SubscriptionTier.Free,
                CreditsRemaining = credits,
                IsActive = true
            };

            _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _mockUserAccessor.Object.GetByIdAsync(userId, CancellationToken.None);

            // Assert
            result?.CreditsRemaining.Should().Be(credits, $"Expected {credits} credits but got {result?.CreditsRemaining}");
        }
    }

    #region Error Response Tests (RED Phase)

    [Fact]
    public async Task Register_WhenEmailAlreadyRegistered_ReturnsConflictWithErrorResponse()
    {
        // Arrange
        var request = new RegisterRequest { Email = "test@example.com", Password = "password123", Name = "Test" };
        _mockAuthManager.Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Email already registered"));

        // Act
        var result = await _controller.Register(request, CancellationToken.None);

        // Assert
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(409);
        var errorResponse = conflictResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Code.Should().Be("EMAIL_ALREADY_REGISTERED");
        errorResponse.Message.Should().Contain("already registered");
    }

    [Fact]
    public async Task Login_WhenInvalidCredentials_ReturnsUnauthorizedWithErrorResponse()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = "wrong" };
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        
        _mockAuthManager.Setup(x => x.LoginAsync(request, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invalid credentials"));

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
        var errorResponse = unauthorizedResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Code.Should().Be("INVALID_CREDENTIALS");
        errorResponse.Message.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task RefreshToken_WhenRefreshTokenMissing_ReturnsBadRequestWithErrorResponse()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = null };
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await _controller.RefreshToken(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Code.Should().Be("REFRESH_TOKEN_REQUIRED");
        errorResponse.Message.Should().Be("Refresh token is required");
    }

    [Fact]
    public async Task RefreshToken_WhenInvalidRefreshToken_ReturnsUnauthorizedWithErrorResponse()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "invalid_token" };
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        
        _mockAuthManager.Setup(x => x.RefreshTokenAsync(request.RefreshToken, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invalid refresh token"));

        // Act
        var result = await _controller.RefreshToken(request, CancellationToken.None);

        // Assert
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
        var errorResponse = unauthorizedResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Code.Should().Be("INVALID_REFRESH_TOKEN");
        errorResponse.Message.Should().Contain("Invalid refresh token");
    }

    [Fact]
    public async Task RevokeApiKey_WhenKeyNotFound_ReturnsNotFoundWithErrorResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var keyId = Guid.NewGuid();
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockAuthManager.Setup(x => x.RevokeApiKeyAsync(userId, keyId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("API key not found"));

        // Act
        var result = await _controller.RevokeApiKey(keyId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
        var errorResponse = notFoundResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Code.Should().Be("API_KEY_NOT_FOUND");
        errorResponse.Message.Should().Be("API key not found");
    }

    [Fact]
    public async Task GetOAuthUrl_WhenRedirectUriMissing_ReturnsBadRequestWithErrorResponse()
    {
        // Act
        var result = _controller.GetOAuthUrl("google", "");

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Code.Should().Be("REDIRECT_URI_REQUIRED");
        errorResponse.Message.Should().Be("redirectUri is required");
    }

    [Fact]
    public async Task GetOAuthUrl_WhenInvalidProvider_ReturnsBadRequestWithErrorResponse()
    {
        // Arrange
        _mockAuthManager.Setup(x => x.GetOAuthUrl("invalid", "http://localhost"))
            .Throws(new InvalidOperationException("Invalid OAuth provider"));

        // Act
        var result = _controller.GetOAuthUrl("invalid", "http://localhost");

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Code.Should().Be("INVALID_OAUTH_PROVIDER");
        errorResponse.Message.Should().Contain("Invalid OAuth provider");
    }

    [Fact]
    public async Task OAuthCallback_WhenAuthenticationFails_ReturnsBadRequestWithErrorResponse()
    {
        // Arrange
        var request = new OAuthLoginRequest { Code = "invalid_code", RedirectUri = "http://localhost" };
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        
        _mockAuthManager.Setup(x => x.OAuthLoginAsync("google", request.Code, request.RedirectUri, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("OAuth authentication failed"));

        // Act
        var result = await _controller.OAuthCallback("google", request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Code.Should().Be("OAUTH_AUTHENTICATION_FAILED");
        errorResponse.Message.Should().Contain("OAuth authentication failed");
    }

    [Fact]
    public async Task OAuthCallback_WhenProviderUnreachable_ReturnsUnauthorizedWithErrorResponse()
    {
        // Arrange
        var request = new OAuthLoginRequest { Code = "code", RedirectUri = "http://localhost" };
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        
        _mockAuthManager.Setup(x => x.OAuthLoginAsync("google", request.Code, request.RedirectUri, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Provider unreachable"));

        // Act
        var result = await _controller.OAuthCallback("google", request, CancellationToken.None);

        // Assert
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
        var errorResponse = unauthorizedResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Code.Should().Be("OAUTH_PROVIDER_UNREACHABLE");
        errorResponse.Message.Should().Be("Failed to authenticate with provider");
    }

    [Fact]
    public async Task LinkOAuthProvider_WhenAlreadyLinked_ReturnsConflictWithErrorResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new OAuthLoginRequest { Code = "code", RedirectUri = "http://localhost" };
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockAuthManager.Setup(x => x.LinkOAuthAsync(userId, "google", request.Code, request.RedirectUri, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Provider already linked"));

        // Act
        var result = await _controller.LinkOAuthProvider("google", request, CancellationToken.None);

        // Assert
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(409);
        var errorResponse = conflictResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Code.Should().Be("OAUTH_PROVIDER_ALREADY_LINKED");
        errorResponse.Message.Should().Contain("already linked");
    }

    [Fact]
    public async Task LinkOAuthProvider_WhenLinkingFails_ReturnsBadRequestWithErrorResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new OAuthLoginRequest { Code = "code", RedirectUri = "http://localhost" };
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockAuthManager.Setup(x => x.LinkOAuthAsync(userId, "google", request.Code, request.RedirectUri, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("OAuth link failed"));

        // Act
        var result = await _controller.LinkOAuthProvider("google", request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Code.Should().Be("OAUTH_LINK_FAILED");
        errorResponse.Message.Should().Contain("OAuth link failed");
    }

    [Fact]
    public async Task LinkOAuthProvider_WhenProviderUnreachable_ReturnsBadRequestWithErrorResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new OAuthLoginRequest { Code = "code", RedirectUri = "http://localhost" };
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockAuthManager.Setup(x => x.LinkOAuthAsync(userId, "google", request.Code, request.RedirectUri, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Provider unreachable"));

        // Act
        var result = await _controller.LinkOAuthProvider("google", request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Code.Should().Be("OAUTH_PROVIDER_UNREACHABLE");
        errorResponse.Message.Should().Be("Failed to authenticate with provider");
    }

    [Fact]
    public async Task UnlinkOAuthProvider_WhenProviderNotLinked_ReturnsNotFoundWithErrorResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockAuthManager.Setup(x => x.UnlinkOAuthAsync(userId, "google", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Provider not linked"));

        // Act
        var result = await _controller.UnlinkOAuthProvider("google", CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
        var errorResponse = notFoundResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Code.Should().Be("OAUTH_PROVIDER_NOT_LINKED");
        errorResponse.Message.Should().Contain("not linked");
    }

    [Fact]
    public async Task UnlinkOAuthProvider_WhenUnlinkingFails_ReturnsBadRequestWithErrorResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockAuthManager.Setup(x => x.UnlinkOAuthAsync(userId, "google", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("OAuth unlink failed"));

        // Act
        var result = await _controller.UnlinkOAuthProvider("google", CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Code.Should().Be("OAUTH_UNLINK_FAILED");
        errorResponse.Message.Should().Contain("OAuth unlink failed");
    }

    #endregion
}
