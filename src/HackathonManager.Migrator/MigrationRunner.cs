using DbUp;
using DbUp.Helpers;
using Microsoft.Extensions.Logging;

namespace HackathonManager.Migrator;

public static class MigrationRunner
{
    public static void UpdateDatabase(string connectionString, ILoggerFactory loggerFactory)
    {
        // DbUp will auto-create the journal table but does not create its schema.
        var journalSchemaMigrator = DeployChanges
            .To.PostgresqlDatabase(connectionString)
            .WithScript("CreateJournalSchema", "create schema if not exists meta;")
            .JournalTo(new NullJournal())
            .LogTo(loggerFactory)
            .Build();

        if (journalSchemaMigrator.PerformUpgrade() is { Successful: false } journalResult)
        {
            throw new MigrationException("Failed to create journal schema.", journalResult);
        }

        var everytimeMigrator = DeployChanges
            .To.PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(
                typeof(MigrationRunner).Assembly,
                name => name.StartsWith("HackathonManager.Migrator.Everytime") && name.EndsWith(".sql")
            )
            .WithTransactionPerScript()
            .JournalTo(new NullJournal())
            .LogTo(loggerFactory)
            .Build();

        if (everytimeMigrator.PerformUpgrade() is { Successful: false } everytimeResult)
        {
            throw new MigrationException("Failed to apply everytime scripts.", everytimeResult);
        }

        var scriptMigrator = DeployChanges
            .To.PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(
                typeof(MigrationRunner).Assembly,
                name => name.StartsWith("HackathonManager.Migrator.Migrations") && name.EndsWith(".sql")
            )
            .WithTransactionPerScript()
            .JournalToPostgresqlTable("meta", "migrations_history")
            .LogTo(loggerFactory)
            .Build();

        if (scriptMigrator.PerformUpgrade() is { Successful: false } scriptResult)
        {
            throw new MigrationException("Failed to apply migration scripts.", scriptResult);
        }
    }
}
