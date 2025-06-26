using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using HackathonManager.Extensions;
using HackathonManager.Migrator;
using HackathonManager.Tests.TestInfrastructure;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests;

public class StartupMigrationTests : IntegrationTestBase<StartupMigrationTests.HackathonApp_StartupMigration>
{
    /// <inheritdoc />
    public StartupMigrationTests(HackathonApp_StartupMigration hackathonApp)
        : base(hackathonApp) { }

    [Fact]
    public async Task ShouldApplyAllMigrationsToDatabase_WhenStartupMigrationIsEnabled()
    {
        // arrange
        var expectedMigrations = typeof(MigrationRunner)
            .Assembly.GetManifestResourceNames()
            .Where(n => n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .ToReadOnlyCollection();

        // act
        _ = await App.Client.GetAsync("/health", TestContext.Current.CancellationToken);

        // assert
        await using var connection = await App.Database.DataSource.OpenConnectionAsync(Cancellation);
        var actualMigrations = await connection.QueryAsync<string>(
            new CommandDefinition("select scriptname from meta.migrations_history", cancellationToken: Cancellation)
        );
        actualMigrations.ShouldBe(expectedMigrations, ignoreOrder: true);
    }

    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public sealed class HackathonApp_StartupMigration()
        : HackathonApp_MigratedDatabase(
            [new KeyValuePair<string, string>(ConfigurationKeys.EnableStartupMigrationKey, "true")]
        );
}
