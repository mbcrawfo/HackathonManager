using System;
using HackathonManager.Migrator;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("HackathonDb")
    ?? throw new InvalidOperationException("Connection string 'HackathonDb' is not configured.");

var app = builder.Build();

var enableIntegratedSpa = builder.Configuration.GetValue<bool>("EnableIntegratedSpa");

if (enableIntegratedSpa)
{
    app.UseStaticFiles();
}

app.MapGet("/api/hello", () => "Hello World!");

if (enableIntegratedSpa)
{
    app.MapFallbackToFile("index.html");
}

if (app.Configuration.GetValue<bool>("MigrateOnStart"))
{
    using var scope = app.Services.CreateScope();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    MigrationRunner.UpdateDatabase(connectionString, loggerFactory);
}

await app.RunAsync();
