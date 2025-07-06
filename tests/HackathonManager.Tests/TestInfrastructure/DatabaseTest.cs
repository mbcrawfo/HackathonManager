using System.Threading.Tasks;
using HackathonManager.Tests.TestInfrastructure.Database;
using Npgsql;
using Xunit;

namespace HackathonManager.Tests.TestInfrastructure;

[Collection(nameof(DatabaseTestCollection))]
public abstract class DatabaseTest : TestBase
{
    private readonly IDatabaseFixture _database;

    protected DatabaseTest(DatabaseTestFixture fixture)
    {
        _database = fixture.Database;
    }

    protected NpgsqlDataSource DataSource => _database.DataSource;

    /// <inheritdoc />
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        if (_database.CanResetData)
        {
            await _database.ResetData();
        }
    }
}

[CollectionDefinition(nameof(DatabaseTestCollection))]
public sealed class DatabaseTestCollection : ICollectionFixture<DatabaseTestFixture>;
