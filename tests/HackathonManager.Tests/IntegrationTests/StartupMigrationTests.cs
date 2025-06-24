using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FastEndpoints.Testing;
using HackathonManager.Extensions;
using HackathonManager.Migrator;
using HackathonManager.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests;

public class StartupMigrationTests : TestBase<StartupMigrationTests.AppWithStartupMigration>
{
    private readonly AppWithStartupMigration _app;

    public StartupMigrationTests(AppWithStartupMigration app)
    {
        _app = app;
    }

    [Fact]
    public async Task ShouldApplyAllMigrationsToDatabase_WhenStartupMigrationIsEnabled()
    {
        // arrange
        var expectedMigrations = typeof(MigrationRunner)
            .Assembly.GetManifestResourceNames()
            .Where(n => n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .ToReadOnlyCollection();

        // act
        _ = await _app.Client.GetAsync("/health", TestContext.Current.CancellationToken);

        // assert
        await using var connection = await _app.DataSource.OpenConnectionAsync(TestContext.Current.CancellationToken);
        var actualMigrations = await connection.QueryAsync<string>(
            new CommandDefinition(
                "select scriptname from meta.migrations_history",
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
        actualMigrations.ShouldBe(expectedMigrations, ignoreOrder: true);
    }

    public sealed class AppWithStartupMigration : AppWithDatabase
    {
        /// <inheritdoc />
        protected override void ConfigureApp(IWebHostBuilder builder)
        {
            base.ConfigureApp(builder);
            builder.UseSetting(Constants.EnableStartupMigrationKey, "true");
        }
    }
}
