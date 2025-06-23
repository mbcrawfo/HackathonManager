using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotNetEnv;
using FluentValidation;
using HackathonManager;
using HackathonManager.Database;
using HackathonManager.Extensions;
using HackathonManager.Migrator;
using HackathonManager.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

Env.LoadMulti([".env", ".env.local"]);

var builder = WebApplication.CreateBuilder(args);

Log.Logger = SerilogConfiguration.CreateBootstrapLogger(builder.Configuration);
var logger = Log.Logger.ForContext<Program>();
logger.Information("{Application} version {Version} starting up...", AppInfo.Name, AppInfo.Version);

try
{
    ConfigureServices();

    var app = builder.Build();
    ConfigurePipeline(app);

    var enableStartupMigration = app.Configuration.GetValue<bool>(Constants.EnableStartupMigrationKey);
    logger.Information("{SettingKey}={SettingValue}", Constants.EnableStartupMigrationKey, enableStartupMigration);
    if (enableStartupMigration)
    {
        using var scope = app.Services.CreateScope();
        MigrationRunner.UpdateDatabase(
            app.Configuration.GetRequiredValue<string>(Constants.ConnectionStringKey),
            scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        );
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
    logger.Information("{Application} shut down", AppInfo.Name);
    await Log.CloseAndFlushAsync();
}

void ConfigureServices()
{
    builder.Services.AddSerilog(
        (provider, config) => config.ConfigureAppLogger(provider.GetRequiredService<IConfiguration>()),
        preserveStaticLogger: true
    );

    AddOpenTelemetryServices();

    builder.Services.AddConfigurationSettings<RequestLoggingSettings>();

    var (dataSource, databaseLoggingSettings) = builder.Configuration.BuildDataSource();
    builder.Services.AddSingleton<DbDataSource>(dataSource);
    builder.Services.AddDbContext<HackathonDbContext>(ob =>
        ob.ConfigureHackathonDbContext(dataSource, databaseLoggingSettings)
    );

    builder.Services.AddValidatorsFromAssembly(AppInfo.Assembly);

    builder.Services.AddHealthChecks();
}

void AddOpenTelemetryServices()
{
    var serviceName = builder.Configuration.GetValue<string>(Constants.ServiceNameKey) ?? AppInfo.Name;
    var traceSettings = builder.Configuration.GetConfigurationSettings<TracerSettings, TracerSettingsValidator>();

    builder.Services.AddSingleton(TracerProvider.Default.GetTracer(serviceName));

    builder
        .Services.AddOpenTelemetry()
        .WithTracing(tracerBuilder =>
        {
            tracerBuilder
                .AddSource(serviceName)
                .SetResourceBuilder(
                    ResourceBuilder
                        .CreateDefault()
                        .AddService(
                            serviceName,
                            serviceVersion: AppInfo.Version,
                            serviceInstanceId: Environment.MachineName
                        )
                )
                .AddAspNetCoreInstrumentation(config =>
                {
                    config.EnrichWithHttpRequest = (activity, request) =>
                    {
                        // Other log properties are automatically added by the instrumentation.
                        activity.SetTag(LogProperties.RequestId, request.HttpContext.TraceIdentifier);
                    };
                })
                .AddHttpClientInstrumentation()
                .AddNpgsql()
                .AddProcessor<HttpRouteProcessor>();

            if (traceSettings.Enabled)
            {
                tracerBuilder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(traceSettings.OtlpEndpoint);
                    options.Protocol = traceSettings.OtlpProtocol;
                    options.Headers = string.Join(
                        ",",
                        traceSettings.OtlpHeaders.Select(kvp => $"{kvp.Key}={kvp.Value}")
                    );
                });
            }
        });
}

void ConfigurePipeline(WebApplication app)
{
    var enableIntegratedSpa = app.Configuration.GetValue<bool>(Constants.EnableIntegratedSpaKey);
    logger.Information("{SettingKey}={SettingValue}", Constants.EnableIntegratedSpaKey, enableIntegratedSpa);

    if (enableIntegratedSpa)
    {
        app.UseStaticFiles();
    }

    app.UseHealthChecks("/health");

    app.UseMiddleware<RequestLoggingMiddleware>();

    app.MapGet(
        "/api/hello",
        async (ILogger<Program> l, HackathonDbContext db) =>
        {
            l.LogInformation("Hello endpoint called");
            return await db.Tests.ToListAsync();
        }
    );

    app.MapPost("/api/echo", (HttpContext context) => context.Request.Body.ToString());

    if (enableIntegratedSpa)
    {
        app.MapFallbackToFile("index.html");
    }
}

// Expose the Program class for integration testing.
[SuppressMessage("Major Code Smell", "S1118:Utility classes should not have public constructors")]
public partial class Program;
