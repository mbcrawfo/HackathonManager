using System.Threading;
using System.Threading.Tasks;
using Bogus;
using HackathonManager.Tests.TestInfrastructure.Database;
using Npgsql;
using Xunit;

namespace HackathonManager.Tests.TestInfrastructure;

[Collection(nameof(DatabaseTestCollection))]
public abstract class DatabaseTestBase : IAsyncLifetime
{
    private readonly IDatabaseFixture _database;

    protected DatabaseTestBase(ResettingDatabaseFixture fixture)
    {
        _database = fixture.Database;
    }

    protected Faker Fake { get; } = new();

    protected NpgsqlDataSource DataSource => _database.DataSource;

    public static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        if (_database.CanResetData)
        {
            await _database.ResetData();
        }
    }
}

[CollectionDefinition(nameof(DatabaseTestCollection))]
public sealed class DatabaseTestCollection : ICollectionFixture<ResettingDatabaseFixture>;
