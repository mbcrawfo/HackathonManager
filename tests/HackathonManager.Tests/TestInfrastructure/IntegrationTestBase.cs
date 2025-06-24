using System.Threading.Tasks;
using FastEndpoints.Testing;

namespace HackathonManager.Tests.TestInfrastructure;

public abstract class IntegrationTestBase : TestBase<AppWithDatabaseReset>
{
    protected readonly AppWithDatabaseReset App;

    protected IntegrationTestBase(AppWithDatabaseReset app)
    {
        App = app;
    }

    /// <inheritdoc />
    protected override async ValueTask SetupAsync() => await App.ResetData();
}
