using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using HackathonManager.Persistence;
using HackathonManager.Settings;
using Npgsql;
using Testcontainers.PostgreSql;

namespace HackathonManager.Tests.TestInfrastructure.Database;

/// <summary>
///     Base fixture that manages a database container.
/// </summary>
public sealed class DatabaseFixture : IDatabaseFixture
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:18beta1")
        .WithDatabase("hackathon")
        .Build();

    private NpgsqlDataSource? _dataSource;

    public string ConnectionString =>
        _container.State switch
        {
            TestcontainersStates.Running => _container.GetConnectionString() + ";Include Error Detail=True",
            _ => throw new InvalidOperationException($"{nameof(DatabaseFixture)} is not initialized"),
        };

    public NpgsqlDataSource DataSource =>
        _dataSource switch
        {
            not null => _dataSource,
            null => throw new InvalidOperationException($"{nameof(DatabaseFixture)} is not initialized"),
        };

    /// <inheritdoc />
    public static IDatabaseFixture Create() => new DatabaseFixture();

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_dataSource is not null)
        {
            await _dataSource.DisposeAsync();
        }

        await _container.DisposeAsync();
    }

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        if (_container.State is not TestcontainersStates.Undefined)
        {
            throw new InvalidOperationException($"{nameof(DatabaseFixture)} is already initialized");
        }

        await _container.StartAsync();

        _dataSource = DataSourceFactory.Create(
            ConnectionString,
            new DatabaseLoggingSettings { EnableDetailedErrors = true, EnableSensitiveDataLogging = true }
        );
    }
}
