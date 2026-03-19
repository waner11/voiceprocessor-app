using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using VoiceProcessor.Accessors.Data.DbContext;

namespace VoiceProcessor.Accessors.Tests.Data;

/// <summary>
/// Shared PostgreSQL container fixture for integration tests.
/// Implements IAsyncLifetime to manage container lifecycle with xUnit.
/// Shared across all test classes in the "PostgreSQL" collection via ICollectionFixture.
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    /// <summary>
    /// Gets the connection string for the running PostgreSQL container.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    public VoiceProcessorDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<VoiceProcessorDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new VoiceProcessorDbContext(options);
    }

    /// <summary>
    /// Starts the container and runs migrations once for the entire collection.
    /// Called automatically by xUnit before any test in the collection executes.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Cleans up the fixture by stopping and disposing the container.
    /// Called automatically by xUnit after test execution.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("PostgreSQL")]
public class PostgresCollection : ICollectionFixture<PostgresFixture> { }
