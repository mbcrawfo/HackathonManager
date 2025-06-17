using System;
using FluentValidation;
using HackathonManager;
using HackathonManager.Extensions;
using HackathonManager.Migrator;
using HackathonManager.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

Log.Logger = SerilogConfiguration.CreateBootstrapLogger(args);
var logger = Log.Logger.ForContext<Program>();
var assembly = typeof(Program).Assembly;
logger.Information(
    "{Application} version {Version} starting up...",
    assembly.GetName().Name,
    assembly.GetName().Version?.ToString() ?? "(unknown)"
);

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((_, config) => config.ConfigureAppLogger(builder.Configuration));

    var connectionString =
        builder.Configuration.GetConnectionString("HackathonDb")
        ?? throw new InvalidOperationException("Connection string 'HackathonDb' is not configured.");

    builder
        .Services.AddOptionsWithValidateOnStart<LogSettings>()
        .BindConfiguration(LogSettings.ConfigurationSection)
        .UseFluentValidation();

    builder.Services.AddValidatorsFromAssembly(assembly);

    builder.Services.AddHealthChecks();

    var app = builder.Build();

    var enableIntegratedSpa = builder.Configuration.GetValue<bool>("EnableIntegratedSpa");
    logger.Information("EnableIntegratedSpa={Value}", enableIntegratedSpa);

    if (enableIntegratedSpa)
    {
        app.UseStaticFiles();
    }

    app.UseHealthChecks("/health");

    app.MapGet("/api/hello", () => "Hello World!");

    if (enableIntegratedSpa)
    {
        app.MapFallbackToFile("index.html");
    }

    var enableStartupMigration = app.Configuration.GetValue<bool>("EnableStartupMigration");
    logger.Information("EnableStartupMigration={Value}", enableStartupMigration);
    if (enableStartupMigration)
    {
        using var scope = app.Services.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        MigrationRunner.UpdateDatabase(connectionString, loggerFactory);
    }

    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    logger.Fatal(ex, "Unhandled exception during application");
    return 1;
}
finally
{
    logger.Information("{Application} shut down", assembly.GetName().Name!);
    await Log.CloseAndFlushAsync();
}
