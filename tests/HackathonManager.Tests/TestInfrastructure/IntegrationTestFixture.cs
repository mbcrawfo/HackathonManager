using System;
using System.Threading.Tasks;
using HackathonManager.Tests.TestInfrastructure.Database;
using JetBrains.Annotations;
using Xunit;

namespace HackathonManager.Tests.TestInfrastructure;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class IntegrationTestFixture<TDatabase> : IAsyncLifetime
    where TDatabase : IDatabaseFixture
{
    public HackathonApp App { get; } = new();

    public IDatabaseFixture Database { get; } = TDatabase.Create();

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await App.DisposeAsync();
        await Database.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await Database.InitializeAsync();
        App.Database = Database;
    }
}
