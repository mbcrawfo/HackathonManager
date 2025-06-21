using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotNetEnv;
using FluentValidation;
using HackathonManager;
using HackathonManager.Extensions;
using HackathonManager.Migrator;
using HackathonManager.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

Env.LoadMulti([".env", ".env.local"]);

Log.Logger = SerilogConfiguration.CreateBootstrapLogger(args);
var logger = Log.Logger.ForContext<Program>();
logger.Information("{Application} version {Version} starting up...", AppInfo.Name, AppInfo.Version);

try
{
    var builder = WebApplication.CreateBuilder(args);
    ConfigureServices(builder);

    var app = builder.Build();
    ConfigurePipeline(app);

    var enableStartupMigration = app.Configuration.GetValue<bool>(Constants.EnableStartupMigrationKey);
    logger.Information("{SettingKey}={SettingValue}", Constants.EnableStartupMigrationKey, enableStartupMigration);
    if (enableStartupMigration)
    {
        using var scope = app.Services.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        MigrationRunner.UpdateDatabase(
            app.Configuration.GetRequiredValue<string>(Constants.ConnectionStringKey),
            loggerFactory
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

void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddSerilog(
        (provider, config) => config.ConfigureAppLogger(provider.GetRequiredService<IConfiguration>()),
        preserveStaticLogger: true
    );

    AddOpenTelemetryServices(builder);

    builder.Services.AddConfigurationSettings<LogSettings>();
    builder.Services.AddConfigurationSettings<RequestLoggingSettings>();
    builder.Services.AddConfigurationSettings<TraceSettings>();

    builder.Services.AddValidatorsFromAssembly(AppInfo.Assembly);

    builder.Services.AddHealthChecks();
}

void AddOpenTelemetryServices(WebApplicationBuilder builder)
{
    var serviceName = builder.Configuration.GetValue<string>(Constants.ServiceNameKey) ?? AppInfo.Name;
    var traceSettings = builder.Configuration.GetConfigurationSettings<TraceSettings, TraceSettingsValidator>();

    if (!traceSettings.EnableUrlQueryRedaction)
    {
        // Redaction is enabled by default and can only be disabled via environment variables.
        // See https://github.com/open-telemetry/opentelemetry-dotnet-contrib/issues/1954
        Environment.SetEnvironmentVariable("OTEL_DOTNET_EXPERIMENTAL_ASPNETCORE_DISABLE_URL_QUERY_REDACTION", "true");
        Environment.SetEnvironmentVariable("OTEL_DOTNET_EXPERIMENTAL_HTTPCLIENT_DISABLE_URL_QUERY_REDACTION", "true");
    }

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
                .AddHttpClientInstrumentation();

            if (traceSettings.Enabled)
            {
                tracerBuilder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(traceSettings.OtlpEndpoint!);
                    options.Protocol = traceSettings.OtlpProtocol!.Value;
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
        ([FromServices] ILogger<Program> l) =>
        {
            l.LogInformation("Hello endpoint called");
            return "Hello World!!";
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
