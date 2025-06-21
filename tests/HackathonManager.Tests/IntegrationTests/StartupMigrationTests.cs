using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using HackathonManager.Extensions;
using HackathonManager.Migrator;
using HackathonManager.Tests.TestInfrastructure;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests;

// Does not use the normal integration test base class because it needs an uninitialized database.
public class StartupMigrationTests : IAsyncLifetime
{
    private readonly PostgresDatabaseFixture _databaseFixture = new();
    private readonly HackathonManagerWebApplicationFactory _factory = new();

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _databaseFixture.DisposeAsync();
    }

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();
    }

    [Fact]
    public async Task ShouldApplyAllMigrationsToDatabase_WhenStartupMigrationIsEnabled()
    {
        // arrange
        var expectedMigrations = typeof(MigrationRunner)
            .Assembly.GetManifestResourceNames()
            .Where(n => n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .ToReadOnlyCollection();

        using var client = _factory
            .WithWebHostBuilder(builder =>
                builder
                    .UseSetting(Constants.ConnectionStringKey, _databaseFixture.ConnectionString)
                    .UseSetting(Constants.EnableStartupMigrationKey, "true")
            )
            .CreateClient();

        // act
        _ = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        // assert
        await using var connection = await _databaseFixture.DataSource.OpenConnectionAsync(
            TestContext.Current.CancellationToken
        );
        var actualMigrations = await connection.QueryAsync<string>(
            new CommandDefinition(
                "select scriptname from meta.migrations_history",
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
        actualMigrations.ShouldBe(expectedMigrations, ignoreOrder: true);
    }
}
