using System;
using System.Threading.Tasks;
using HackathonManager.Tests.TestInfrastructure.Database;

namespace HackathonManager.Tests.TestInfrastructure;

public class AppWithDatabaseReset : AppFixtureBase
{
    public AppWithDatabaseReset()
        : base(new DatabaseResetDecorator(new DatabaseMigrationDecorator(new DatabaseFixture())))
    {
        ResetData = ((DatabaseResetDecorator)DatabaseFixture).ResetData;
    }

    public Func<Task> ResetData { get; }
}
