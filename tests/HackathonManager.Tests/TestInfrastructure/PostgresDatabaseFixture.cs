using System;
using System.Data.Common;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace HackathonManager.Tests.TestInfrastructure;

public sealed class PostgresDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:18beta1")
        .WithDatabase("hackathon")
        .Build();

    private DbDataSource? _dataSource;

    public string ConnectionString =>
        _container.State switch
        {
            TestcontainersStates.Running => _container.GetConnectionString(),
            _ => throw new InvalidOperationException($"{nameof(PostgresDatabaseFixture)} is not initialized"),
        };

    public DbDataSource DataSource =>
        _dataSource switch
        {
            not null => _dataSource,
            null => throw new InvalidOperationException($"{nameof(PostgresDatabaseFixture)} is not initialized"),
        };

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
            throw new InvalidOperationException($"{nameof(PostgresDatabaseFixture)} is already initialized");
        }

        await _container.StartAsync();
        _dataSource = NpgsqlDataSource.Create(_container.GetConnectionString());
    }
}
