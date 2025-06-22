using Npgsql;
using Xunit;

namespace HackathonManager.Tests.TestInfrastructure.Database;

public interface IDatabaseFixture : IAsyncLifetime
{
    string ConnectionString { get; }

    NpgsqlDataSource DataSource { get; }
}
