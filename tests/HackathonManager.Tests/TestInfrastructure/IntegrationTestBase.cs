using System.Threading.Tasks;
using HackathonManager.Tests.TestInfrastructure.Database;

namespace HackathonManager.Tests.TestInfrastructure;

/// <summary>
///     Common base for all integration tests of the web app.
/// </summary>
/// <typeparam name="TDatabase"></typeparam>
public abstract class IntegrationTestBase<TDatabase> : TestBase
    where TDatabase : IDatabaseFixture
{
    private readonly IntegrationTestFixture<TDatabase> _fixture;

    protected IntegrationTestBase(IntegrationTestFixture<TDatabase> fixture)
    {
        _fixture = fixture;
    }

    protected HackathonApp App => _fixture.App;

    protected IDatabaseFixture Database => _fixture.Database;

    /// <inheritdoc />
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        if (Database.CanResetData)
        {
            await Database.ResetData();
        }
    }
}
