using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using VoiceProcessor.Accessors.Data;
using VoiceProcessor.Accessors.Data.DbContext;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Tests.Data;

public class PasswordResetTokenAccessorIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer _container = null!;
    private VoiceProcessorDbContext _dbContext = null!;
    private PasswordResetTokenAccessor _accessor = null!;
    private Guid _userId = Guid.Empty;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder("postgres:16-alpine").Build();
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<VoiceProcessorDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        _dbContext = new VoiceProcessorDbContext(options);
        await _dbContext.Database.MigrateAsync();

        _accessor = new PasswordResetTokenAccessor(_dbContext);

        _userId = await SeedUser();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task CreateAsync_ValidToken_PersistsToDatabase()
    {
        // Arrange
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            TokenHash = "hash_abc123",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _accessor.CreateAsync(token);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(token.Id);
        result.UserId.Should().Be(_userId);
        result.TokenHash.Should().Be("hash_abc123");

        var dbToken = await _dbContext.PasswordResetTokens.FindAsync(token.Id);
        dbToken.Should().NotBeNull();
        dbToken!.TokenHash.Should().Be("hash_abc123");
    }

    [Fact]
    public async Task GetByTokenHashAsync_ValidActiveToken_ReturnsToken()
    {
        // Arrange
        var tokenHash = "hash_valid_active";
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UsedAt = null
        };
        _dbContext.PasswordResetTokens.Add(token);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _accessor.GetByTokenHashAsync(tokenHash);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(token.Id);
        result.TokenHash.Should().Be(tokenHash);
    }

    [Fact]
    public async Task GetByTokenHashAsync_ExpiredToken_ReturnsNull()
    {
        // Arrange
        var tokenHash = "hash_expired";
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // expired
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            UsedAt = null
        };
        _dbContext.PasswordResetTokens.Add(token);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _accessor.GetByTokenHashAsync(tokenHash);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByTokenHashAsync_UsedToken_ReturnsNull()
    {
        // Arrange
        var tokenHash = "hash_used";
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            UsedAt = DateTime.UtcNow.AddMinutes(-5) // already used
        };
        _dbContext.PasswordResetTokens.Add(token);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _accessor.GetByTokenHashAsync(tokenHash);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByTokenHashAsync_NonExistentHash_ReturnsNull()
    {
        // Act
        var result = await _accessor.GetByTokenHashAsync("nonexistent_hash");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task MarkAsUsedAsync_ExistingToken_SetsUsedAt()
    {
        // Arrange
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            TokenHash = "hash_to_mark_used",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UsedAt = null
        };
        _dbContext.PasswordResetTokens.Add(token);
        await _dbContext.SaveChangesAsync();

        // Act
        await _accessor.MarkAsUsedAsync(token.Id);

        // Assert
        var dbToken = await _dbContext.PasswordResetTokens.FindAsync(token.Id);
        dbToken.Should().NotBeNull();
        dbToken!.UsedAt.Should().NotBeNull();
        dbToken.UsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task InvalidateAllForUserAsync_MultipleTokens_DeletesAllForUser()
    {
        // Arrange
        var otherUserId = await SeedUser("other@example.com");

        var token1 = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            TokenHash = "hash_user_token_1",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };
        var token2 = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            TokenHash = "hash_user_token_2",
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            CreatedAt = DateTime.UtcNow
        };
        var otherUserToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId,
            TokenHash = "hash_other_user_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.PasswordResetTokens.AddRange(token1, token2, otherUserToken);
        await _dbContext.SaveChangesAsync();

        // Act
        await _accessor.InvalidateAllForUserAsync(_userId);

        // Assert
        var remainingUserTokens = await _dbContext.PasswordResetTokens
            .Where(t => t.UserId == _userId)
            .ToListAsync();
        remainingUserTokens.Should().BeEmpty();

        var otherUserTokens = await _dbContext.PasswordResetTokens
            .Where(t => t.UserId == otherUserId)
            .ToListAsync();
        otherUserTokens.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteExpiredAsync_ExpiredTokens_DeletesOnlyExpired()
    {
        // Arrange
        var expiredToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            TokenHash = "hash_expired_to_delete",
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // expired
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };
        var activeToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            TokenHash = "hash_active_to_keep",
            ExpiresAt = DateTime.UtcNow.AddHours(1), // still valid
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.PasswordResetTokens.AddRange(expiredToken, activeToken);
        await _dbContext.SaveChangesAsync();

        // Act
        await _accessor.DeleteExpiredAsync();

        // Assert
        var remaining = await _dbContext.PasswordResetTokens.ToListAsync();
        remaining.Should().HaveCount(1);
        remaining[0].Id.Should().Be(activeToken.Id);
    }

    private async Task<Guid> SeedUser(string email = "test@example.com")
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = email,
            Name = "Test User",
            Tier = SubscriptionTier.Free,
            CreditsRemaining = 1000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return userId;
    }
}
