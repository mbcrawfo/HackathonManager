using System;
using System.Linq;
using DotNetEnv;
using FluentValidation;
using HackathonManager;
using HackathonManager.Extensions;
using HackathonManager.Migrator;
using HackathonManager.Settings;
using HackathonManager.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    builder.Services.AddSerilog(
        (provider, config) => config.ConfigureAppLogger(provider.GetRequiredService<IConfiguration>())
    );

    AddOpenTelemetry(builder);

    var connectionString =
        builder.Configuration.GetConnectionString("HackathonDb")
        ?? throw new InvalidOperationException("Connection string 'HackathonDb' is not configured.");

    builder
        .Services.AddOptionsWithValidateOnStart<LogSettings>()
        .BindConfiguration(LogSettings.ConfigurationSection)
        .UseFluentValidation();

    builder
        .Services.AddOptionsWithValidateOnStart<TraceSettings>()
        .BindConfiguration(TraceSettings.ConfigurationSection)
        .UseFluentValidation();

    builder.Services.AddValidatorsFromAssembly(AppInfo.Assembly);

    builder.Services.AddHealthChecks();

    var app = builder.Build();

    var enableIntegratedSpa = builder.Configuration.GetValue<bool>("EnableIntegratedSpa");
    logger.Information("EnableIntegratedSpa={Value}", enableIntegratedSpa);

    if (enableIntegratedSpa)
    {
        app.UseStaticFiles();
    }

    app.UseHealthChecks("/health");

    app.MapGet(
        "/api/hello",
        ([FromServices] ILogger<Program> l) =>
        {
            l.LogInformation("Hello endpoint called");
            return "Hello World!!";
        }
    );

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
    logger.Information("{Application} shut down", AppInfo.Name);
    await Log.CloseAndFlushAsync();
}

void AddOpenTelemetry(WebApplicationBuilder builder)
{
    var serviceName = builder.Configuration.GetValue<string>("ServiceName") ?? AppInfo.Name;
    var traceSettings =
        builder.Configuration.GetSection(TraceSettings.ConfigurationSection).Get<TraceSettings>()
        ?? new TraceSettings();
    if (new TraceSettingsValidator().Validate(traceSettings) is { IsValid: false } validationResult)
    {
        throw new OptionsValidationException(
            nameof(TraceSettings),
            typeof(TraceSettings),
            FluentValidateOptions<TraceSettings>.FormatValidationErrors(validationResult)
        );
    }

    if (!traceSettings.EnableUrlQueryRedaction)
    {
        // Redaction is enabled by default and can only be disabled via environment variables.
        // See https://github.com/open-telemetry/opentelemetry-dotnet-contrib/issues/1954
        Environment.SetEnvironmentVariable("OTEL_DOTNET_EXPERIMENTAL_ASPNETCORE_DISABLE_URL_QUERY_REDACTION", "true");
        Environment.SetEnvironmentVariable("OTEL_DOTNET_EXPERIMENTAL_HTTPCLIENT_DISABLE_URL_QUERY_REDACTION", "true");
    }

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
                        // Matches the properties Asp.Net adds to the request's logging scope.
                        activity.SetTag("ConnectionId", request.HttpContext.Connection.Id);
                        activity.SetTag("RequestId", request.HttpContext.TraceIdentifier);
                    };
                })
                .AddHttpClientInstrumentation();

            if (traceSettings.Enabled)
            {
                var headers = traceSettings.OtlpHeaders is { Count: > 0 }
                    ? string.Join(",", traceSettings.OtlpHeaders.Select(kvp => $"{kvp.Key}={kvp.Value}"))
                    : null;

                tracerBuilder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(traceSettings.OtlpEndpoint!);
                    options.Protocol = traceSettings.OtlpProtocol!.Value;
                    options.Headers = headers;
                });
            }
        });
}
