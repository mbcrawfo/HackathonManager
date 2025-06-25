using System.Collections.Generic;
using HackathonManager.Tests.TestInfrastructure.Database;

namespace HackathonManager.Tests.TestInfrastructure;

// ReSharper disable once InconsistentNaming
public class HackathonApp_MigratedDatabase : HackathonApp
{
    // ReSharper disable once MemberCanBeProtected.Global
    public HackathonApp_MigratedDatabase()
        : this([]) { }

    protected HackathonApp_MigratedDatabase(IEnumerable<KeyValuePair<string, string>> additionalSettings)
        : base(additionalSettings)
    {
        Database = new DatabaseMigrationDecorator(Database);
    }
}
