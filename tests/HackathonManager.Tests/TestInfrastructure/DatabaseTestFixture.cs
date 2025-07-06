using System.Threading.Tasks;
using HackathonManager.Tests.TestInfrastructure.Database;
using JetBrains.Annotations;
using Xunit;

namespace HackathonManager.Tests.TestInfrastructure;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class DatabaseTestFixture : IAsyncLifetime
{
    public IDatabaseFixture Database { get; } = DatabaseResetFixture.Create();

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await Database.DisposeAsync();

    /// <inheritdoc />
    public async ValueTask InitializeAsync() => await Database.InitializeAsync();
}
