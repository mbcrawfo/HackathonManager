using System;
using System.Collections.Generic;
using System.IO;
using HackathonManager.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog.Events;

namespace HackathonManager.Tests.TestInfrastructure;

public sealed class HackathonManagerWebApplicationFactory : WebApplicationFactory<Program>
{
    public ConsoleLogSettings? ConsoleLogSettings { get; init; } =
        new() { Level = LogEventLevel.Verbose, Type = ConsoleLogType.Text };

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.UseContentRoot(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "IntegrationTests"));

        builder.UseSetting("Serilog:MinimumLevel:Default", nameof(LogEventLevel.Verbose));

        if (ConsoleLogSettings is not null)
        {
            var prefix = ConsoleLogSettings.ConfigurationSection + ":";
            builder.UseSetting(prefix + nameof(ConsoleLogSettings.Type), ConsoleLogSettings.Type.ToString());
            builder.UseSetting(prefix + nameof(ConsoleLogSettings.Level), ConsoleLogSettings.Level.ToString());
            builder.UseSetting(prefix + nameof(ConsoleLogSettings.TextTemplate), ConsoleLogSettings.TextTemplate);
        }

        // Log settings are configured with environment variable so they will be picked up by the bootstrap logger.
        var logSettings = LogSettings.ConfigurationSection.ToUpperInvariant();

        // Disable all other loggers.
        Environment.SetEnvironmentVariable(
            $"{logSettings}__{nameof(LogSettings.EnableFileLogging).ToUpperInvariant()}",
            "false"
        );
        Environment.SetEnvironmentVariable(
            $"{logSettings}__{nameof(LogSettings.EnableOpenTelemetryLogging).ToUpperInvariant()}",
            "false"
        );

        builder.ConfigureAppConfiguration(config =>
            config.AddInMemoryCollection(
                [
                    // Disable tracing.
                    new KeyValuePair<string, string?>(
                        $"{TraceSettings.ConfigurationSection}:{nameof(TraceSettings.Enabled)}",
                        "false"
                    ),
                    // Enable logging of all request bodies.
                    new KeyValuePair<string, string?>(
                        $"{RequestLoggingSettings.ConfigurationSection}:{nameof(RequestLoggingSettings.LogBody)}",
                        "true"
                    ),
                    new KeyValuePair<string, string?>(
                        $"{RequestLoggingSettings.ConfigurationSection}:{nameof(RequestLoggingSettings.ContentTypes)}:0",
                        "*/*"
                    ),
                ]
            )
        );
    }
}
