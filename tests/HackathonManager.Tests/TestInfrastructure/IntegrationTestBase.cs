using System.Threading.Tasks;
using FastEndpoints.Testing;

namespace HackathonManager.Tests.TestInfrastructure;

public abstract class IntegrationTestBase<TApp> : TestBase<TApp>
    where TApp : HackathonApp
{
    protected readonly HackathonApp App;

    protected IntegrationTestBase(TApp hackathonApp)
    {
        App = hackathonApp;
    }

    /// <inheritdoc />
    protected override async ValueTask SetupAsync()
    {
        if (App.Database.CanResetData)
        {
            await App.Database.ResetData();
        }
    }
}
