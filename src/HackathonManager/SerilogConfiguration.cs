using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Destructurama;
using HackathonManager.Extensions;
using HackathonManager.Settings;
using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Formatting.Compact;

namespace HackathonManager;

public static class SerilogConfiguration
{
    public static ILogger CreateBootstrapLogger(IConfigurationRoot configuration) =>
        new LoggerConfiguration().ConfigureAppLogger(configuration).CreateBootstrapLogger();

    public static LoggerConfiguration ConfigureAppLogger(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration appConfiguration
    )
    {
        var serviceName = appConfiguration.GetValue<string>(Constants.ServiceNameKey) ?? AppInfo.Name;
        var settings = appConfiguration.GetConfigurationSettings<LogSettings, LogSettingsValidator>();

        loggerConfiguration
            .ReadFrom.Configuration(appConfiguration)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder().WithDefaultDestructurers())
            .Destructure.UsingAttributes();

        var console = appConfiguration.GetConfigurationSettings<ConsoleLogSettings, ConsoleLogSettingsValidator>();
        switch (console.Type)
        {
            case ConsoleLogType.Text:
                loggerConfiguration.WriteTo.Console(
                    console.Level,
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {RequestId} {Message:lj}{NewLine}{Exception}"
                );
                break;
            case ConsoleLogType.Json:
                loggerConfiguration.WriteTo.Console(new RenderedCompactJsonFormatter(), console.Level);
                break;
        }

        var file = appConfiguration.GetConfigurationSettings<FileLogSettings, FileLogSettingsValidator>();
        if (file.Enabled)
        {
            var path = file.Path;
            if (path.StartsWith('.') && !path.StartsWith(".."))
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), path[1..]);
            }

            loggerConfiguration.WriteTo.File(
                path,
                file.Level,
                "{Timestamp:HH:mm:ss.fff} | {Level:u3} | {RequestMethod} {RequestPath} | {RequestId} | {SourceContext} | {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day
            );
        }

        if (settings.EnableOpenTelemetryLogging)
        {
            loggerConfiguration.WriteTo.OpenTelemetry(options =>
            {
                options.RestrictedToMinimumLevel = settings.OpenTelemetryLogLevel;
                options.Endpoint = settings.OtlpEndpoint;
                options.Protocol = settings.OtlpProtocol!.Value; // Validation ensures this is not null
                options.Headers = settings.OtlpHeaders;
                options.OnBeginSuppressInstrumentation = SuppressInstrumentationScope.Begin;
                options.ResourceAttributes = new Dictionary<string, object>
                {
                    // Matches the OpenTelemetry tracing setup.
                    { "service.name", serviceName },
                    { "service.version", AppInfo.Version },
                    { "service.instance.id", Environment.MachineName },
                };
            });
        }

        return loggerConfiguration;
    }
}
