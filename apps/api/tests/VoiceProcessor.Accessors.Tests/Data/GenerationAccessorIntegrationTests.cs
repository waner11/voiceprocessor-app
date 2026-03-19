using FluentAssertions;
using VoiceProcessor.Accessors.Data;
using VoiceProcessor.Accessors.Data.DbContext;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Tests.Data;

[Collection("PostgreSQL")]
public class GenerationAccessorIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private VoiceProcessorDbContext _dbContext = null!;
    private GenerationAccessor _accessor = null!;
    private Guid _voiceId = Guid.Empty;

    public GenerationAccessorIntegrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        _dbContext = _fixture.CreateDbContext();
        _accessor = new GenerationAccessor(_dbContext);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task GetByUserPagedAsync_WithSearchFilter_ReturnsMatchingGenerations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedUserAndVoice(userId);

        var gen1 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "Hello world, this is a test",
            CharacterCount = 28,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        var gen2 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "The quick brown fox jumps over the lazy dog",
            CharacterCount = 44,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.OpenAI,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var gen3 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "Another generation with different content",
            CharacterCount = 41,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.GoogleCloud,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Generations.AddRange(gen1, gen2, gen3);
        await _dbContext.SaveChangesAsync();

        // Act - Search for "hello"
        var (items, totalCount) = await _accessor.GetByUserPagedAsync(
            userId, 1, 20, null, "hello", null, CancellationToken.None);

        // Assert
        items.Should().HaveCount(1);
        totalCount.Should().Be(1);
        items[0].Id.Should().Be(gen1.Id);
        items[0].InputText.Should().Contain("Hello");
    }

    [Fact]
    public async Task GetByUserPagedAsync_WithSearchFilter_CaseInsensitive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedUserAndVoice(userId);

        var generation = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "UPPERCASE TEXT FOR TESTING",
            CharacterCount = 27,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Generations.Add(generation);
        await _dbContext.SaveChangesAsync();

        // Act - Search with lowercase
        var (items, totalCount) = await _accessor.GetByUserPagedAsync(
            userId, 1, 20, null, "uppercase", null, CancellationToken.None);

        // Assert
        items.Should().HaveCount(1);
        totalCount.Should().Be(1);
        items[0].Id.Should().Be(generation.Id);
    }

    [Fact]
    public async Task GetByUserPagedAsync_WithSearchFilter_SearchesFirst200Chars()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedUserAndVoice(userId);

        // Create text longer than 200 chars with search term at position 250
        var longText = new string('a', 250) + "searchterm" + new string('b', 250);
        var generation = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = longText,
            CharacterCount = longText.Length,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Generations.Add(generation);
        await _dbContext.SaveChangesAsync();

        // Act - Search for term that's beyond 200 chars
        var (items, _) = await _accessor.GetByUserPagedAsync(
            userId, 1, 20, null, "searchterm", null, CancellationToken.None);

        // Assert - Should NOT find it because it's beyond 200 chars
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserPagedAsync_WithProviderFilter_ReturnsMatchingProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedUserAndVoice(userId);

        var gen1 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "ElevenLabs generation",
            CharacterCount = 21,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var gen2 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "OpenAI generation",
            CharacterCount = 17,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.OpenAI,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Generations.AddRange(gen1, gen2);
        await _dbContext.SaveChangesAsync();

        // Act - Filter by ElevenLabs
        var (items, totalCount) = await _accessor.GetByUserPagedAsync(
            userId, 1, 20, null, null, Provider.ElevenLabs, CancellationToken.None);

        // Assert
        items.Should().HaveCount(1);
        totalCount.Should().Be(1);
        items[0].Id.Should().Be(gen1.Id);
        items[0].SelectedProvider.Should().Be(Provider.ElevenLabs);
    }

    [Fact]
    public async Task GetByUserPagedAsync_WithBothFilters_ReturnsCombinedResults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedUserAndVoice(userId);

        var gen1 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "Hello from ElevenLabs",
            CharacterCount = 21,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        var gen2 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "Hello from OpenAI",
            CharacterCount = 17,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.OpenAI,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var gen3 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "Goodbye from ElevenLabs",
            CharacterCount = 24,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Generations.AddRange(gen1, gen2, gen3);
        await _dbContext.SaveChangesAsync();

        // Act - Search for "Hello" AND filter by ElevenLabs
        var (items, totalCount) = await _accessor.GetByUserPagedAsync(
            userId, 1, 20, null, "Hello", Provider.ElevenLabs, CancellationToken.None);

        // Assert
        items.Should().HaveCount(1);
        totalCount.Should().Be(1);
        items[0].Id.Should().Be(gen1.Id);
        items[0].InputText.Should().Contain("Hello");
        items[0].SelectedProvider.Should().Be(Provider.ElevenLabs);
    }

    [Fact]
    public async Task GetByUserPagedAsync_WithStatusAndSearchFilters_MaintainsExistingBehavior()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedUserAndVoice(userId);

        var gen1 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "Completed generation",
            CharacterCount = 20,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var gen2 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "Pending generation",
            CharacterCount = 18,
            Status = GenerationStatus.Pending,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.OpenAI,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Generations.AddRange(gen1, gen2);
        await _dbContext.SaveChangesAsync();

        // Act - Filter by status AND search
        var (items, totalCount) = await _accessor.GetByUserPagedAsync(
            userId, 1, 20, GenerationStatus.Completed, "generation", null, CancellationToken.None);

        // Assert
        items.Should().HaveCount(1);
        totalCount.Should().Be(1);
        items[0].Id.Should().Be(gen1.Id);
        items[0].Status.Should().Be(GenerationStatus.Completed);
    }

    [Fact]
    public async Task GetByUserPagedAsync_WithoutFilters_ReturnAllUserGenerations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedUserAndVoice(userId);

        var gen1 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "First generation",
            CharacterCount = 16,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var gen2 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "Second generation",
            CharacterCount = 17,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.OpenAI,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Generations.AddRange(gen1, gen2);
        await _dbContext.SaveChangesAsync();

        // Act - No filters
        var (items, totalCount) = await _accessor.GetByUserPagedAsync(
            userId, 1, 20, null, null, null, CancellationToken.None);

        // Assert
        items.Should().HaveCount(2);
        totalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetByUserPagedAsync_WithSearchFilter_EscapesPercentWildcard()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedUserAndVoice(userId);

        var gen1 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "50% off",
            CharacterCount = 7,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            CreatedAt = DateTime.UtcNow
        };

        var gen2 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "50 off",
            CharacterCount = 6,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        _dbContext.Generations.AddRange(gen1, gen2);
        await _dbContext.SaveChangesAsync();

        // Act - Search for literal "50%"
        var (items, totalCount) = await _accessor.GetByUserPagedAsync(
            userId, 1, 20, null, "50%", null, CancellationToken.None);

        // Assert - Should match ONLY gen1 (literal %), not gen2 (which has "50 " but not "50%")
        items.Should().HaveCount(1);
        totalCount.Should().Be(1);
        items[0].Id.Should().Be(gen1.Id);
        items[0].InputText.Should().Contain("50%");
    }

    [Fact]
    public async Task GetByUserPagedAsync_WithSearchFilter_EscapesUnderscoreWildcard()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedUserAndVoice(userId);

        var gen1 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "under_score test",
            CharacterCount = 16,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            CreatedAt = DateTime.UtcNow
        };

        var gen2 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "underscore test",
            CharacterCount = 15,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        _dbContext.Generations.AddRange(gen1, gen2);
        await _dbContext.SaveChangesAsync();

        // Act - Search for literal "under_score"
        var (items, totalCount) = await _accessor.GetByUserPagedAsync(
            userId, 1, 20, null, "under_score", null, CancellationToken.None);

        // Assert - Should match ONLY gen1 (literal _), not underscore as wildcard
        items.Should().HaveCount(1);
        totalCount.Should().Be(1);
        items[0].Id.Should().Be(gen1.Id);
    }

    [Fact]
    public async Task GetByUserPagedAsync_WithSearchFilter_EscapesBackslashWildcard()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedUserAndVoice(userId);

        var gen1 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "back\\slash test",
            CharacterCount = 15,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            CreatedAt = DateTime.UtcNow
        };

        var gen2 = new Generation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VoiceId = _voiceId,
            InputText = "backslash test",
            CharacterCount = 14,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        _dbContext.Generations.AddRange(gen1, gen2);
        await _dbContext.SaveChangesAsync();

        // Act - Search for literal "back\slash"
        var (items, totalCount) = await _accessor.GetByUserPagedAsync(
            userId, 1, 20, null, "back\\slash", null, CancellationToken.None);

        // Assert - Should match ONLY gen1 (literal \)
        items.Should().HaveCount(1);
        totalCount.Should().Be(1);
        items[0].Id.Should().Be(gen1.Id);
    }

    private async Task SeedUserAndVoice(Guid userId)
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

        _voiceId = Guid.NewGuid();
        var voice = new Voice
        {
            Id = _voiceId,
            Name = "Test Voice",
            Provider = Provider.ElevenLabs,
            ProviderVoiceId = "voice_123",
            CostPerThousandChars = 0.30m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        _dbContext.Voices.Add(voice);
        await _dbContext.SaveChangesAsync();
    }
}
