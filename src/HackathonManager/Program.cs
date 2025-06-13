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

app.MapGet("/", () => "Hello World!");

if (app.Configuration.GetValue<bool>("MigrateOnStart"))
{
    using var scope = app.Services.CreateScope();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    MigrationRunner.UpdateDatabase(connectionString, loggerFactory);
}

await app.RunAsync();
