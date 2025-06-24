using HackathonManager.Tests.TestInfrastructure.Database;

namespace HackathonManager.Tests.TestInfrastructure;

public class AppWithDatabase : AppFixtureBase
{
    public AppWithDatabase()
        : base(new DatabaseFixture()) { }
}
