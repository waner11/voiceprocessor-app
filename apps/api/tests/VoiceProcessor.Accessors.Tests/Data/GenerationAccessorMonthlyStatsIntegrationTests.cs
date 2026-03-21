using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VoiceProcessor.Accessors.Data;
using VoiceProcessor.Accessors.Data.DbContext;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Tests.Data;

[Collection("PostgreSQL")]
public class GenerationAccessorMonthlyStatsIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private VoiceProcessorDbContext _dbContext = null!;
    private GenerationAccessor _accessor = null!;
    private Guid _voiceId = Guid.Empty;

    public GenerationAccessorMonthlyStatsIntegrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _dbContext = _fixture.CreateDbContext();
        _accessor = CreateAccessor();

        await _dbContext.Feedbacks.ExecuteDeleteAsync();
        await _dbContext.GenerationChunks.ExecuteDeleteAsync();
        await _dbContext.Generations.ExecuteDeleteAsync();
        await _dbContext.Voices.ExecuteDeleteAsync();
        await _dbContext.Users.ExecuteDeleteAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task GetMonthlyStatsAsync_WithCompletedGenerations_ReturnsOnlyCompletedCountAndDuration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var monthStart = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc);
        await SeedUserAndVoice(userId);

        _dbContext.Generations.AddRange(
            CreateGeneration(userId, GenerationStatus.Completed, monthStart.AddDays(1), 60000),
            CreateGeneration(userId, GenerationStatus.Completed, monthStart.AddDays(2), 120000),
            CreateGeneration(userId, GenerationStatus.Failed, monthStart.AddDays(3), 300000),
            CreateGeneration(userId, GenerationStatus.Cancelled, monthStart.AddDays(4), 400000));
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _accessor.GetMonthlyStatsAsync(userId, monthStart, CancellationToken.None);

        // Assert
        result.GenerationCount.Should().Be(2);
        result.TotalAudioDurationMs.Should().Be(180000);
    }

    [Fact]
    public async Task GetMonthlyStatsAsync_WithFailedAndCancelledOnly_ReturnsZeroValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var monthStart = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc);
        await SeedUserAndVoice(userId);

        _dbContext.Generations.AddRange(
            CreateGeneration(userId, GenerationStatus.Failed, monthStart.AddDays(1), 100000),
            CreateGeneration(userId, GenerationStatus.Cancelled, monthStart.AddDays(2), 100000));
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _accessor.GetMonthlyStatsAsync(userId, monthStart, CancellationToken.None);

        // Assert
        result.Should().Be((0, 0));
    }

    [Fact]
    public async Task GetMonthlyStatsAsync_WithNullAudioDuration_TreatsNullAsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var monthStart = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc);
        await SeedUserAndVoice(userId);

        _dbContext.Generations.AddRange(
            CreateGeneration(userId, GenerationStatus.Completed, monthStart.AddDays(1), null),
            CreateGeneration(userId, GenerationStatus.Completed, monthStart.AddDays(2), 45000));
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _accessor.GetMonthlyStatsAsync(userId, monthStart, CancellationToken.None);

        // Assert
        result.GenerationCount.Should().Be(2);
        result.TotalAudioDurationMs.Should().Be(45000);
    }

    [Fact]
    public async Task GetMonthlyStatsAsync_WithNoUserGenerations_ReturnsZeroValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var monthStart = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc);
        await SeedUserAndVoice(userId);

        // Act
        var result = await _accessor.GetMonthlyStatsAsync(userId, monthStart, CancellationToken.None);

        // Assert
        result.Should().Be((0, 0));
    }

    private GenerationAccessor CreateAccessor()
    {
        return new GenerationAccessor(_dbContext);
    }

    private Generation CreateGeneration(Guid userId, GenerationStatus status, DateTime createdAt, int? audioDurationMs)
    {
        return new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "test input",
            CharacterCount = 10,
            Status = status,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            AudioDurationMs = audioDurationMs,
            CreatedAt = createdAt
        };
    }

    private async Task SeedUserAndVoice(Guid userId)
    {
        var user = new User
        {
            Id = userId,
            Email = $"usage-test-{userId}@example.com",
            Name = "Test User",
            Tier = SubscriptionTier.Free,
            CreditsRemaining = 1000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _voiceId = Guid.NewGuid();
        var voice = new Voice
        {
            Id = _voiceId,
            Name = "Test Voice",
            Provider = Provider.ElevenLabs,
            ProviderVoiceId = _voiceId.ToString(),
            CostPerThousandChars = 0.30m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        _dbContext.Voices.Add(voice);
        await _dbContext.SaveChangesAsync();
    }
}
