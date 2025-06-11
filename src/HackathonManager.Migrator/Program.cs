using System;
using HackathonManager.Migrator;
using Microsoft.Extensions.Logging;

if (args is ["-h"] or ["--help"] || args is not [var connectionString])
{
    Console.WriteLine(
        """
        Usage:
        dotnet run <connectionString>
            Apply migrations to the database using the provided connection string.

        dotnet run -h | --help
            Print this help message.
        """
    );
    return 1;
}

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace));
var logger = loggerFactory.CreateLogger(nameof(Program));

try
{
    MigrationRunner.UpdateDatabase(connectionString, loggerFactory);
    return 0;
}
catch (MigrationException ex)
{
    logger.LogError(ex, "Failed to apply migrations");
    return 1;
}
catch (Exception ex)
{
    logger.LogError(ex, "Unexpected error in migration process");
    return 1;
}
