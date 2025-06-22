using System;
using System.Data.Common;
using System.Threading.Tasks;
using HackathonManager.Tests.TestInfrastructure.Database;
using Xunit;

namespace HackathonManager.Tests.TestInfrastructure;

public sealed class IntegrationTestsFixture : IAsyncLifetime
{
    private readonly IDatabaseFixture _databaseFixture;

    public IntegrationTestsFixture()
    {
        var fixture = new DatabaseResetDecorator(new DatabaseMigrationDecorator(new DatabaseFixture()));
        _databaseFixture = fixture;

        ResetData = fixture.ResetData;
    }

    public DbDataSource DataSource => _databaseFixture.DataSource;

    public HackathonManagerWebApplicationFactory Factory { get; } = new();

    public Func<Task> ResetData { get; }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _databaseFixture.DisposeAsync();
    }

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();
        Factory.DataSource = _databaseFixture.DataSource;
    }
}
