using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Managers.User;

namespace VoiceProcessor.Managers.Tests.User;

public class UserManagerTests
{
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly Mock<ILogger<UserManager>> _mockLogger;

    public UserManagerTests()
    {
        _mockUserAccessor = new Mock<IUserAccessor>();
        _mockLogger = new Mock<ILogger<UserManager>>();
    }

    private UserManager CreateManager()
    {
        return new UserManager(
            _mockUserAccessor.Object,
            _mockLogger.Object
        );
    }

    #region GetUserAsync Tests

    [Fact]
    public async Task GetUserAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var user = new Domain.Entities.User
        {
            Id = userId,
            Email = "test@example.com",
            Name = "Test User",
            CreditsRemaining = 100,
            IsActive = true
        };

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await manager.GetUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Email.Should().Be("test@example.com");
        result.Name.Should().Be("Test User");
        result.CreditsRemaining.Should().Be(100);
    }

    [Fact]
    public async Task GetUserAsync_NonExistentUser_ReturnsNull()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.User?)null);

        // Act
        var result = await manager.GetUserAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetUserByEmailAsync Tests

    [Fact]
    public async Task GetUserByEmailAsync_ExistingEmail_ReturnsUser()
    {
        // Arrange
        var manager = CreateManager();
        var email = "test@example.com";
        var user = new Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = "Test User",
            CreditsRemaining = 100,
            IsActive = true
        };

        _mockUserAccessor.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await manager.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.Name.Should().Be("Test User");
    }

    [Fact]
    public async Task GetUserByEmailAsync_NonExistentEmail_ReturnsNull()
    {
        // Arrange
        var manager = CreateManager();
        var email = "nonexistent@example.com";

        _mockUserAccessor.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.User?)null);

        // Act
        var result = await manager.GetUserByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region HasSufficientCreditsAsync Tests

    [Fact]
    public async Task HasSufficientCreditsAsync_SufficientCredits_ReturnsTrue()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var user = new Domain.Entities.User
        {
            Id = userId,
            Email = "test@example.com",
            CreditsRemaining = 100,
            IsActive = true
        };

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await manager.HasSufficientCreditsAsync(userId, 50);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasSufficientCreditsAsync_InsufficientCredits_ReturnsFalse()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var user = new Domain.Entities.User
        {
            Id = userId,
            Email = "test@example.com",
            CreditsRemaining = 30,
            IsActive = true
        };

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await manager.HasSufficientCreditsAsync(userId, 50);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasSufficientCreditsAsync_UserNotFound_ReturnsFalse()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.User?)null);

        // Act
        var result = await manager.HasSufficientCreditsAsync(userId, 50);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region DeductCreditsAsync Tests

    [Fact]
    public async Task DeductCreditsAsync_Success_DeductsAndUpdates()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var user = new Domain.Entities.User
        {
            Id = userId,
            Email = "test@example.com",
            CreditsRemaining = 100,
            CreditsUsedThisMonth = 50,
            IsActive = true
        };

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await manager.DeductCreditsAsync(userId, 30);

        // Assert
        user.CreditsRemaining.Should().Be(70);
        user.CreditsUsedThisMonth.Should().Be(80);
        _mockUserAccessor.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeductCreditsAsync_UserNotFound_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.User?)null);

        // Act
        var act = async () => await manager.DeductCreditsAsync(userId, 30);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"User {userId} not found");

        _mockUserAccessor.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeductCreditsAsync_InsufficientCredits_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var user = new Domain.Entities.User
        {
            Id = userId,
            Email = "test@example.com",
            CreditsRemaining = 20,
            CreditsUsedThisMonth = 50,
            IsActive = true
        };

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = async () => await manager.DeductCreditsAsync(userId, 30);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Insufficient credits");

        _mockUserAccessor.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region UpdateLastActivityAsync Tests

    [Fact]
    public async Task UpdateLastActivityAsync_ExistingUser_UpdatesLastActiveAt()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var user = new Domain.Entities.User
        {
            Id = userId,
            Email = "test@example.com",
            LastActiveAt = null,
            IsActive = true
        };

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var beforeCall = DateTime.UtcNow;

        // Act
        await manager.UpdateLastActivityAsync(userId);

        var afterCall = DateTime.UtcNow;

        // Assert
        user.LastActiveAt.Should().NotBeNull();
        user.LastActiveAt.Should().BeOnOrAfter(beforeCall);
        user.LastActiveAt.Should().BeOnOrBefore(afterCall);
        _mockUserAccessor.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLastActivityAsync_UserNotFound_ReturnsSilently()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.User?)null);

        // Act
        await manager.UpdateLastActivityAsync(userId);

        // Assert
        _mockUserAccessor.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
