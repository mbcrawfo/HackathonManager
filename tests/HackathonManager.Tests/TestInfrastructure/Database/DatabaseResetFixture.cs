using System;
using System.Threading.Tasks;
using Npgsql;
using Respawn;
using Xunit;

namespace HackathonManager.Tests.TestInfrastructure.Database;

/// <summary>
///     Decorates a database fixture, adding the ability to reset it between tests.
/// </summary>
public sealed class DatabaseResetFixture : IDatabaseFixture
{
    private readonly IDatabaseFixture _fixture;
    private Respawner? _respawner;

    public DatabaseResetFixture(IDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await _fixture.DisposeAsync();

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await _fixture.InitializeAsync();

        await using var connection = await DataSource.OpenConnectionAsync(TestContext.Current.CancellationToken);
        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions { DbAdapter = DbAdapter.Postgres, SchemasToExclude = ["meta"] }
        );
    }

    /// <inheritdoc />
    public bool CanResetData => true;

    /// <inheritdoc />
    public string ConnectionString => _fixture.ConnectionString;

    /// <inheritdoc />
    public NpgsqlDataSource DataSource => _fixture.DataSource;

    public async Task ResetData()
    {
        if (_respawner is null)
        {
            throw new InvalidOperationException($"{nameof(DatabaseResetFixture)} is not initialized.");
        }

        await using var connection = await DataSource.OpenConnectionAsync(TestContext.Current.CancellationToken);
        await _respawner.ResetAsync(connection);
    }

    /// <inheritdoc />
    public static IDatabaseFixture Create() =>
        new DatabaseResetFixture(new DatabaseMigrationFixture(new DatabaseFixture()));
}
