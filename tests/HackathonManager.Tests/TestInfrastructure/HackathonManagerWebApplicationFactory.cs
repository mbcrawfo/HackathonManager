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

    public FileLogSettings? FileLogSettings { get; init; } = new() { Enabled = false };

    public OpenTelemetryLogSettings? OpenTelemetryLogSettings { get; init; } = new() { Enabled = false };

    public TracerSettings? TracerSettings { get; init; } = new() { Enabled = false };

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
        }

        if (FileLogSettings is not null)
        {
            var prefix = FileLogSettings.ConfigurationSection + ":";
            builder.UseSetting(prefix + nameof(FileLogSettings.Enabled), FileLogSettings.Enabled.ToString());
            builder.UseSetting(prefix + nameof(FileLogSettings.Level), FileLogSettings.Level.ToString());
            builder.UseSetting(prefix + nameof(FileLogSettings.Path), FileLogSettings.Path);
        }

        if (OpenTelemetryLogSettings is not null)
        {
            var prefix = OpenTelemetryLogSettings.ConfigurationSection + ":";
            builder.UseSetting(
                prefix + nameof(OpenTelemetryLogSettings.Enabled),
                OpenTelemetryLogSettings.Enabled.ToString()
            );
            builder.UseSetting(
                prefix + nameof(OpenTelemetryLogSettings.Level),
                OpenTelemetryLogSettings.Level.ToString()
            );
            builder.UseSetting(
                prefix + nameof(OpenTelemetryLogSettings.OtlpEndpoint),
                OpenTelemetryLogSettings.OtlpEndpoint
            );
            builder.UseSetting(
                prefix + nameof(OpenTelemetryLogSettings.OtlpProtocol),
                OpenTelemetryLogSettings.OtlpProtocol.ToString()
            );
            foreach (var header in OpenTelemetryLogSettings.OtlpHeaders)
            {
                builder.UseSetting(
                    $"{prefix}{nameof(OpenTelemetryLogSettings.OtlpHeaders)}:{header.Key}",
                    header.Value
                );
            }
        }

        if (TracerSettings is not null)
        {
            var prefix = TracerSettings.ConfigurationSection + ":";
            builder.UseSetting(prefix + nameof(TracerSettings.Enabled), TracerSettings.Enabled.ToString());
            builder.UseSetting(prefix + nameof(TracerSettings.OtlpEndpoint), TracerSettings.OtlpEndpoint);
            builder.UseSetting(prefix + nameof(TracerSettings.OtlpProtocol), TracerSettings.OtlpProtocol.ToString());
            foreach (var header in TracerSettings.OtlpHeaders)
            {
                builder.UseSetting($"{prefix}{nameof(TracerSettings.OtlpHeaders)}:{header.Key}", header.Value);
            }
        }

        builder.ConfigureAppConfiguration(config =>
            config.AddInMemoryCollection(
                [
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
