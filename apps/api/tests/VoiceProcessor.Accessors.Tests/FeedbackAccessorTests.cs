using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using VoiceProcessor.Accessors.Data;
using VoiceProcessor.Accessors.Data.DbContext;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Tests.Data;

public class FeedbackAccessorTests : IDisposable
{
    private readonly VoiceProcessorDbContext _dbContext;
    private readonly FeedbackAccessor _accessor;

    public FeedbackAccessorTests()
    {
        var options = new DbContextOptionsBuilder<VoiceProcessorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new VoiceProcessorDbContext(options);
        _accessor = new FeedbackAccessor(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task UpsertAsync_NewFeedback_CreatesRecord()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid();

        await SeedUserAndGeneration(userId, generationId);

        var feedback = new Feedback
        {
            Id = Guid.NewGuid(),
            GenerationId = generationId,
            UserId = userId,
            Rating = 5,
            Comment = "Excellent quality!",
            WasDownloaded = true,
            PlaybackCount = 3,
            PlaybackDurationMs = 15000,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _accessor.UpsertAsync(feedback);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(feedback.Id);
        result.GenerationId.Should().Be(generationId);
        result.UserId.Should().Be(userId);
        result.Rating.Should().Be(5);
        result.Comment.Should().Be("Excellent quality!");
        result.WasDownloaded.Should().BeTrue();
        result.PlaybackCount.Should().Be(3);
        result.PlaybackDurationMs.Should().Be(15000);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeNull();

        var dbFeedback = await _dbContext.Feedbacks.FindAsync(feedback.Id);
        dbFeedback.Should().NotBeNull();
        dbFeedback!.Rating.Should().Be(5);
    }

    [Fact]
    public async Task UpsertAsync_ExistingFeedback_UpdatesRecord()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid();

        await SeedUserAndGeneration(userId, generationId);

        var originalFeedback = new Feedback
        {
            Id = Guid.NewGuid(),
            GenerationId = generationId,
            UserId = userId,
            Rating = 3,
            Comment = "Good",
            WasDownloaded = false,
            PlaybackCount = 1,
            PlaybackDurationMs = 5000,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        _dbContext.Feedbacks.Add(originalFeedback);
        await _dbContext.SaveChangesAsync();

        var updatedFeedback = new Feedback
        {
            Id = originalFeedback.Id,
            GenerationId = generationId,
            UserId = userId,
            Rating = 5,
            Comment = "Actually excellent!",
            WasDownloaded = true,
            PlaybackCount = 5,
            PlaybackDurationMs = 20000,
            CreatedAt = originalFeedback.CreatedAt
        };

        // Act
        var result = await _accessor.UpsertAsync(updatedFeedback);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(originalFeedback.Id);
        result.Rating.Should().Be(5);
        result.Comment.Should().Be("Actually excellent!");
        result.WasDownloaded.Should().BeTrue();
        result.PlaybackCount.Should().Be(5);
        result.PlaybackDurationMs.Should().Be(20000);
        result.CreatedAt.Should().Be(originalFeedback.CreatedAt);
        result.UpdatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var dbFeedback = await _dbContext.Feedbacks.FindAsync(originalFeedback.Id);
        dbFeedback.Should().NotBeNull();
        dbFeedback!.Rating.Should().Be(5);
        dbFeedback.Comment.Should().Be("Actually excellent!");
    }

    [Fact]
    public async Task UpsertAsync_NullRatingAndComment_Accepted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid();

        await SeedUserAndGeneration(userId, generationId);

        var feedback = new Feedback
        {
            Id = Guid.NewGuid(),
            GenerationId = generationId,
            UserId = userId,
            Rating = null,
            Comment = null,
            WasDownloaded = true,
            PlaybackCount = 1,
            PlaybackDurationMs = 1000,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _accessor.UpsertAsync(feedback);

        // Assert
        result.Should().NotBeNull();
        result.Rating.Should().BeNull();
        result.Comment.Should().BeNull();
        result.WasDownloaded.Should().BeTrue();

        var dbFeedback = await _dbContext.Feedbacks.FindAsync(feedback.Id);
        dbFeedback.Should().NotBeNull();
        dbFeedback!.Rating.Should().BeNull();
        dbFeedback.Comment.Should().BeNull();
    }

    private async Task SeedUserAndGeneration(Guid userId, Guid generationId)
    {
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Name = "Test User",
            Tier = SubscriptionTier.Free,
            CreditsRemaining = 1000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var voice = new Voice
        {
            Id = Guid.NewGuid(),
            Name = "Test Voice",
            Provider = Provider.ElevenLabs,
            ProviderVoiceId = "voice_123",
            CostPerThousandChars = 0.30m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var generation = new Generation
        {
            Id = generationId,
            UserId = userId,
            VoiceId = voice.Id,
            InputText = "Test text",
            CharacterCount = 9,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            AudioFormat = "mp3",
            EstimatedCost = 0.01m,
            ChunkCount = 1,
            ChunksCompleted = 1,
            Progress = 100,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        _dbContext.Voices.Add(voice);
        _dbContext.Generations.Add(generation);
        await _dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task UpsertAsync_ConcurrentInsert_RetriesAsUpdate()
    {
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<VoiceProcessorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var testDbContext = new TestVoiceProcessorDbContext(options);

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Name = "Test User",
            Tier = SubscriptionTier.Free,
            CreditsRemaining = 1000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var voice = new Voice
        {
            Id = Guid.NewGuid(),
            Name = "Test Voice",
            Provider = Provider.ElevenLabs,
            ProviderVoiceId = "voice_123",
            CostPerThousandChars = 0.30m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var generation = new Generation
        {
            Id = generationId,
            UserId = userId,
            VoiceId = voice.Id,
            InputText = "Test text",
            CharacterCount = 9,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            AudioFormat = "mp3",
            EstimatedCost = 0.01m,
            ChunkCount = 1,
            ChunksCompleted = 1,
            Progress = 100,
            CreatedAt = DateTime.UtcNow
        };

        testDbContext.Users.Add(user);
        testDbContext.Voices.Add(voice);
        testDbContext.Generations.Add(generation);
        await testDbContext.SaveChangesAsync();

        var existingFeedback = new Feedback
        {
            Id = Guid.NewGuid(),
            GenerationId = generationId,
            UserId = userId,
            Rating = 3,
            Comment = "Good",
            WasDownloaded = false,
            PlaybackCount = 1,
            PlaybackDurationMs = 5000,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        testDbContext.Feedbacks.Add(existingFeedback);
        await testDbContext.SaveChangesAsync();

        testDbContext.SaveChangesCallCount = 0;

        var accessor = new FeedbackAccessor(testDbContext);

        var newFeedback = new Feedback
        {
            Id = Guid.NewGuid(),
            GenerationId = generationId,
            UserId = userId,
            Rating = 5,
            Comment = "Great!",
            WasDownloaded = true,
            PlaybackCount = 2,
            PlaybackDurationMs = 10000,
            CreatedAt = DateTime.UtcNow
        };

        testDbContext.ThrowOnNextSaveChanges = true;

        var result = await accessor.UpsertAsync(newFeedback);

        result.Should().NotBeNull();
        result.Id.Should().Be(existingFeedback.Id);
        result.Rating.Should().Be(5);
        result.Comment.Should().Be("Great!");
        result.PlaybackCount.Should().Be(2);
        result.WasDownloaded.Should().BeTrue();
        testDbContext.SaveChangesCallCount.Should().Be(2);
    }
}

public class TestVoiceProcessorDbContext : VoiceProcessorDbContext
{
    public bool ThrowOnNextSaveChanges { get; set; }
    public int SaveChangesCallCount { get; set; }

    public TestVoiceProcessorDbContext(DbContextOptions<VoiceProcessorDbContext> options) : base(options)
    {
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;

        if (ThrowOnNextSaveChanges)
        {
            ThrowOnNextSaveChanges = false;
            throw new DbUpdateException(
                "The database operation was expected to affect 1 row(s), but actually affected 0 row(s).",
                new Exception("Unique constraint violation on (GenerationId, UserId)"));
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
