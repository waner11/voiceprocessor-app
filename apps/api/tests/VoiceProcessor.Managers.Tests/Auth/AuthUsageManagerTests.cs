using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Engines.Contracts;
using VoiceProcessor.Engines.Security;
using VoiceProcessor.Managers.Auth;
using AppOptions = VoiceProcessor.Managers.Options.AppOptions;

namespace VoiceProcessor.Managers.Tests.Auth;

public class AuthUsageManagerTests
{
    private readonly Mock<IUserAccessor> _mockUserAccessor = new();
    private readonly Mock<IGenerationAccessor> _mockGenerationAccessor = new();
    private readonly Mock<IRefreshTokenAccessor> _mockRefreshTokenAccessor = new();
    private readonly Mock<IApiKeyAccessor> _mockApiKeyAccessor = new();
    private readonly Mock<IExternalLoginAccessor> _mockExternalLoginAccessor = new();
    private readonly Mock<IPasswordResetTokenAccessor> _mockPasswordResetTokenAccessor = new();
    private readonly Mock<IEmailAccessor> _mockEmailAccessor = new();
    private readonly Mock<IJwtEngine> _mockJwtEngine = new();
    private readonly Mock<IPasswordEngine> _mockPasswordEngine = new();
    private readonly Mock<IApiKeyEngine> _mockApiKeyEngine = new();
    private readonly Mock<ILogger<AuthManager>> _mockLogger = new();

    private AuthManager CreateManager()
    {
        return new AuthManager(
            _mockUserAccessor.Object,
            _mockGenerationAccessor.Object,
            _mockRefreshTokenAccessor.Object,
            _mockApiKeyAccessor.Object,
            _mockExternalLoginAccessor.Object,
            _mockPasswordResetTokenAccessor.Object,
            _mockEmailAccessor.Object,
            _mockJwtEngine.Object,
            _mockPasswordEngine.Object,
            _mockApiKeyEngine.Object,
            [],
            Microsoft.Extensions.Options.Options.Create(new JwtOptions
            {
                SecretKey = "test-secret-key-minimum-32-chars-long",
                Issuer = "VoiceProcessor",
                Audience = "VoiceProcessor",
                AccessTokenExpirationMinutes = 15,
                RefreshTokenExpirationDays = 7
            }),
            Microsoft.Extensions.Options.Options.Create(new AppOptions
            {
                FrontendBaseUrl = "http://localhost:3000"
            }),
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetUsageAsync_ReturnsComposedUsageResponseFromUserAndGenerationStats()
    {
        var manager = CreateManager();
        var userId = Guid.NewGuid();

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, CreditsUsedThisMonth = 42, CreditsRemaining = 58 });
        _mockGenerationAccessor.Setup(x => x.GetMonthlyStatsAsync(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((7, 120_000L));

        var result = await manager.GetUsageAsync(userId, CancellationToken.None);

        result.CreditsUsedThisMonth.Should().Be(42);
        result.CreditsRemaining.Should().Be(58);
        result.GenerationsCount.Should().Be(7);
        result.TotalAudioMinutes.Should().Be(2);
    }

    [Fact]
    public async Task GetUsageAsync_ConvertsAudioDurationMillisecondsToRoundedDownMinutes()
    {
        var manager = CreateManager();
        var userId = Guid.NewGuid();

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, CreditsUsedThisMonth = 1, CreditsRemaining = 99 });
        _mockGenerationAccessor.Setup(x => x.GetMonthlyStatsAsync(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((1, 90_000L));

        var result = await manager.GetUsageAsync(userId, CancellationToken.None);

        result.TotalAudioMinutes.Should().Be(1);
    }

    [Fact]
    public async Task GetUsageAsync_PassesCancellationTokenToAccessorCalls()
    {
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, cancellationToken))
            .ReturnsAsync(new User { Id = userId, CreditsUsedThisMonth = 5, CreditsRemaining = 95 });
        _mockGenerationAccessor.Setup(x => x.GetMonthlyStatsAsync(userId, It.IsAny<DateTime>(), cancellationToken))
            .ReturnsAsync((3, 180_000L));

        await manager.GetUsageAsync(userId, cancellationToken);

        _mockUserAccessor.Verify(x => x.GetByIdAsync(userId, cancellationToken), Times.Once);
        _mockGenerationAccessor.Verify(x => x.GetMonthlyStatsAsync(userId, It.IsAny<DateTime>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetUsageAsync_ReturnsZeroAndRemainingCreditsValuesFromUser()
    {
        var manager = CreateManager();
        var userId = Guid.NewGuid();

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, CreditsUsedThisMonth = 0, CreditsRemaining = 100 });
        _mockGenerationAccessor.Setup(x => x.GetMonthlyStatsAsync(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0L));

        var result = await manager.GetUsageAsync(userId, CancellationToken.None);

        result.CreditsUsedThisMonth.Should().Be(0);
        result.CreditsRemaining.Should().Be(100);
    }
}
