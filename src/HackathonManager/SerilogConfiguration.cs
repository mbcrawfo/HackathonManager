using System;
using System.Collections.Generic;
using System.IO;
using Destructurama;
using HackathonManager.Settings;
using HackathonManager.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Formatting.Compact;

namespace HackathonManager;

public static class SerilogConfiguration
{
    public static ILogger CreateBootstrapLogger(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production;

        // Emulate the way that the host builder structures configuration so that we can use values from configuration
        // to bootstrap the logging setup.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddUserSecrets(AppInfo.Assembly, optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        return new LoggerConfiguration().ConfigureAppLogger(configuration).CreateBootstrapLogger();
    }

    public static LoggerConfiguration ConfigureAppLogger(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration appConfiguration
    )
    {
        var serviceName = appConfiguration.GetValue<string>("ServiceName") ?? AppInfo.Name;

        var settings =
            appConfiguration.GetSection(LogSettings.ConfigurationSection).Get<LogSettings>() ?? new LogSettings();
        if (new LogSettingsValidator().Validate(settings) is { IsValid: false } validationResult)
        {
            throw new OptionsValidationException(
                nameof(LogSettings),
                typeof(LogSettings),
                FluentValidateOptions<LogSettings>.FormatValidationErrors(validationResult)
            );
        }

        loggerConfiguration
            .ReadFrom.Configuration(appConfiguration)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder().WithDefaultDestructurers())
            .Destructure.UsingAttributes();

        if (settings.EnableConsoleTextLogging)
        {
            loggerConfiguration.WriteTo.Console(settings.ConsoleLogLevel, settings.ConsoleOutputTemplate);
        }

        if (settings.EnableConsoleJsonLogging)
        {
            loggerConfiguration.WriteTo.Console(new RenderedCompactJsonFormatter(), settings.ConsoleLogLevel);
        }

        if (settings.EnableFileLogging)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "log.txt");
            loggerConfiguration.WriteTo.File(
                filePath,
                outputTemplate: "{Timestamp:HH:mm:ss.fff} | {Level:u3} | {RequestMethod} {RequestPath} | {RequestId} | {SourceContext} | {Message:lj}{NewLine}{Exception}",
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
                options.Headers = settings.OtlpHeaders ?? new Dictionary<string, string>();
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
