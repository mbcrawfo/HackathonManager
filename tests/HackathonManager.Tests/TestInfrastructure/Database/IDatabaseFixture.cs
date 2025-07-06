using System;
using System.Threading.Tasks;
using Npgsql;
using Xunit;

namespace HackathonManager.Tests.TestInfrastructure.Database;

public interface IDatabaseFixture : IAsyncLifetime
{
    bool CanResetData => false;

    string ConnectionString { get; }

    NpgsqlDataSource DataSource { get; }

    Task ResetData() => throw new NotImplementedException();

    static abstract IDatabaseFixture Create();
}
