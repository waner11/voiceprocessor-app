using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VoiceProcessor.Clients.Api.Controllers;
using VoiceProcessor.Clients.Api.Services;
using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Managers.Tests.Auth;

public class UsageControllerTests
{
    private readonly Mock<IAuthManager> _mockAuthManager;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly UsageController _controller;

    public UsageControllerTests()
    {
        _mockAuthManager = new Mock<IAuthManager>();
        _mockCurrentUser = new Mock<ICurrentUserService>();
        
        _controller = new UsageController(_mockAuthManager.Object, _mockCurrentUser.Object);
    }

    [Fact]
    public async Task GetUsage_ReturnsOk_WithUsageResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var usageResponse = new UsageResponse
        {
            CreditsUsedThisMonth = 5000,
            CreditsRemaining = 10000,
            GenerationsCount = 25,
            TotalAudioMinutes = 120
        };

        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockAuthManager.Setup(x => x.GetUsageAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usageResponse);

        // Act
        var result = await _controller.GetUsage(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<UsageResponse>().Subject;
        response.CreditsUsedThisMonth.Should().Be(5000);
        response.CreditsRemaining.Should().Be(10000);
        response.GenerationsCount.Should().Be(25);
        response.TotalAudioMinutes.Should().Be(120);
    }

    [Fact]
    public async Task GetUsage_CallsManagerWithCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var usageResponse = new UsageResponse
        {
            CreditsUsedThisMonth = 0,
            CreditsRemaining = 15000,
            GenerationsCount = 0,
            TotalAudioMinutes = 0
        };

        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockAuthManager.Setup(x => x.GetUsageAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usageResponse);

        // Act
        await _controller.GetUsage(CancellationToken.None);

        // Assert
        _mockAuthManager.Verify(x => x.GetUsageAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUsage_PassesCancellationToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();
        var usageResponse = new UsageResponse
        {
            CreditsUsedThisMonth = 0,
            CreditsRemaining = 15000,
            GenerationsCount = 0,
            TotalAudioMinutes = 0
        };

        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockAuthManager.Setup(x => x.GetUsageAsync(userId, cancellationToken))
            .ReturnsAsync(usageResponse);

        // Act
        await _controller.GetUsage(cancellationToken);

        // Assert
        _mockAuthManager.Verify(x => x.GetUsageAsync(userId, cancellationToken), Times.Once);
    }
}
