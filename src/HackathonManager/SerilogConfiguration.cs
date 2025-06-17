using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Destructurama;
using HackathonManager.Settings;
using HackathonManager.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
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
        var settings =
            appConfiguration.GetSection(LogSettings.ConfigurationSection).Get<LogSettings>() ?? new LogSettings();
        if (new LogSettings.Validator().Validate(settings) is { IsValid: false } validationResult)
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
                options.Endpoint = settings.OpenTelemetryEndpoint;
                options.Protocol = settings.OpenTelemetryProtocol!.Value; // Validation ensures this is not null
                options.Headers = settings.OpenTelemetryHeaders ?? new Dictionary<string, string>();
                // TODO: Enable after adding tracing to the project
                //options.OnBeginSuppressInstrumentation =

                options.ResourceAttributes = new Dictionary<string, object> { { "service.name", "HackathonManager" } };
            });
        }

        return loggerConfiguration;
    }
}
