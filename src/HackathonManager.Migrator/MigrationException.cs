using System;
using System.Diagnostics.CodeAnalysis;
using DbUp.Engine;

namespace HackathonManager.Migrator;

/// <summary>
///     Thrown when a failure occurs during database migrations.
/// </summary>
[SuppressMessage(
    "Roslynator",
    "RCS1194:Implement exception constructors",
    Justification = "Exception is used in specific migration context that does not require all constructors."
)]
public sealed class MigrationException : Exception
{
    /// <summary>
    ///     Constructs a new instance of <see cref="MigrationException" />.
    /// </summary>
    /// <param name="message">
    ///     Message describing the context of the failure.
    /// </param>
    /// <param name="upgradeResult">
    ///     The <see cref="DatabaseUpgradeResult" /> that contains details about the failure.
    /// </param>
    public MigrationException(string message, DatabaseUpgradeResult upgradeResult)
        : base(message, upgradeResult.Error)
    {
        FailedScript = upgradeResult.ErrorScript;
    }

    /// <summary>
    ///     Gets the SQL script that caused the migration failure. See <see cref="DatabaseUpgradeResult.ErrorScript" />.
    /// </summary>
    public SqlScript FailedScript { get; }
}
