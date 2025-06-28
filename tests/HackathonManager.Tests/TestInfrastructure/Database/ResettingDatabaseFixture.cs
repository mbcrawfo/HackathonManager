using System.Threading.Tasks;
using Xunit;

namespace HackathonManager.Tests.TestInfrastructure.Database;

public sealed class ResettingDatabaseFixture : IAsyncLifetime
{
    public IDatabaseFixture Database { get; } =
        new DatabaseResetDecorator(new DatabaseMigrationDecorator(new DatabaseFixture()));

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await Database.DisposeAsync();

    /// <inheritdoc />
    public async ValueTask InitializeAsync() => await Database.InitializeAsync();
}
