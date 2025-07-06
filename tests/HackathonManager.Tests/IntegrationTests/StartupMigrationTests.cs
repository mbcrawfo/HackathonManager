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

public class StartupMigrationTests : IntegrationTestEmptyDb
{
    /// <inheritdoc />
    public StartupMigrationTests(IntegrationTestEmptyDbFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task ShouldApplyAllMigrationsToDatabase_WhenStartupMigrationIsEnabled()
    {
        // arrange
        using var client = App.WithWebHostBuilder(b =>
                b.UseSetting(ConfigurationKeys.EnableStartupMigrationKey, "true")
            )
            .CreateClient();

        var expectedMigrations = typeof(MigrationRunner)
            .Assembly.GetManifestResourceNames()
            .Where(n =>
                n.StartsWith("HackathonManager.Migrator.Migrations")
                && n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)
            )
            .ToReadOnlyCollection();

        // act
        _ = await client.GetAsync("/health", CancellationToken);

        // assert
        await using var connection = await Database.DataSource.OpenConnectionAsync(CancellationToken);
        var actualMigrations = await connection.QueryAsync<string>(
            new CommandDefinition(
                "select scriptname from meta.migrations_history",
                cancellationToken: CancellationToken
            )
        );
        actualMigrations.ShouldBe(expectedMigrations, ignoreOrder: true);
    }
}
