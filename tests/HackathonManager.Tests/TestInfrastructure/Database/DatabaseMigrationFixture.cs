using System;
using System.Threading.Tasks;
using HackathonManager.Migrator;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace HackathonManager.Tests.TestInfrastructure.Database;

/// <summary>
///     Decorates a database fixture, applying migrations after the database is initialized.
/// </summary>
public sealed class DatabaseMigrationFixture : IDatabaseFixture
{
    private readonly IDatabaseFixture _fixture;

    public DatabaseMigrationFixture(IDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await _fixture.DisposeAsync();

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await _fixture.InitializeAsync();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace));
        MigrationRunner.UpdateDatabase(ConnectionString, loggerFactory);
    }

    /// <inheritdoc />
    public string ConnectionString => _fixture.ConnectionString;

    /// <inheritdoc />
    public NpgsqlDataSource DataSource => _fixture.DataSource;

    /// <inheritdoc />
    public static IDatabaseFixture Create() => new DatabaseMigrationFixture(new DatabaseFixture());
}
