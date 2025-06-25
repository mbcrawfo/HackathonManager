using System;
using System.Collections.Generic;
using System.IO;
using Destructurama;
using HackathonManager.Extensions;
using HackathonManager.Settings;
using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Formatting.Compact;
using Serilog.Sinks.OpenTelemetry;

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

        var otelSettings = appConfiguration.GetConfigurationSettings<
            OpenTelemetrySettings,
            OpenTelemetrySettingsValidator
        >();
        var exporterSettings = otelSettings.AllExporters ?? otelSettings.LogExporter;

        if (exporterSettings?.Enabled is true)
        {
            loggerConfiguration.WriteTo.OpenTelemetry(options =>
            {
                options.RestrictedToMinimumLevel = otelSettings.LogLevel;
                options.Endpoint = exporterSettings.Endpoint;
                options.Protocol = exporterSettings.Protocol switch
                {
                    OtlpExportProtocol.Grpc => OtlpProtocol.Grpc,
                    OtlpExportProtocol.HttpProtobuf => OtlpProtocol.HttpProtobuf,
                    _ => throw new InvalidOperationException(
                        $"Unexpected export protocol: {exporterSettings.Protocol}"
                    ),
                };
                options.Headers = exporterSettings.Headers;
                options.OnBeginSuppressInstrumentation = SuppressInstrumentationScope.Begin;
                options.ResourceAttributes = new Dictionary<string, object>
                {
                    // Matches the OpenTelemetry tracing setup.
                    { "service.name", appConfiguration.GetValue<string>(Constants.ServiceNameKey) ?? AppInfo.Name },
                    { "service.version", AppInfo.Version },
                    { "service.instance.id", Environment.MachineName },
                };
            });
        }

        return loggerConfiguration;
    }
}
