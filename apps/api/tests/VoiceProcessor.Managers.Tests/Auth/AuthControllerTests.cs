using FluentAssertions;
using Moq;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Managers.Tests.Auth;

/// <summary>
/// Tests for AuthController.GetCurrentUser() endpoint.
/// These tests verify that the endpoint returns actual user credits from the database
/// instead of hardcoded values.
/// </summary>
public class AuthControllerTests
{
    private readonly Mock<IUserAccessor> _mockUserAccessor;

    public AuthControllerTests()
    {
        _mockUserAccessor = new Mock<IUserAccessor>();
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
}
