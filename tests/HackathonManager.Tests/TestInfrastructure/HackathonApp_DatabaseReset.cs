using System.Collections.Generic;
using HackathonManager.Tests.TestInfrastructure.Database;
using JetBrains.Annotations;

namespace HackathonManager.Tests.TestInfrastructure;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class HackathonApp_DatabaseReset : HackathonApp_MigratedDatabase
{
    public HackathonApp_DatabaseReset()
        : this([]) { }

    protected HackathonApp_DatabaseReset(IEnumerable<KeyValuePair<string, string>> additionalSettings)
        : base(additionalSettings)
    {
        Database = new DatabaseResetDecorator(Database);
    }
}
