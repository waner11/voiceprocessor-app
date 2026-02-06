using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using VoiceProcessor.Accessors.Data.DbContext;

namespace VoiceProcessor.Accessors.Tests.Data;

/// <summary>
/// PostgreSQL container fixture for integration tests.
/// Implements IAsyncLifetime to manage container lifecycle with xUnit.
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    /// <summary>
    /// Gets the connection string for the running PostgreSQL container.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <summary>
    /// Initializes the fixture by starting the container and running migrations.
    /// Called automatically by xUnit before test execution.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Create DbContext with the container's connection string
        var options = new DbContextOptionsBuilder<VoiceProcessorDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        // Run migrations to set up the database schema
        await using var context = new VoiceProcessorDbContext(options);
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
