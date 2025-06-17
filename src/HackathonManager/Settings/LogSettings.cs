using System.Collections.Generic;
using FluentValidation;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;

namespace HackathonManager.Settings;

public sealed class LogSettings
{
    public const string ConfigurationSection = "Log";

    public bool EnableConsoleTextLogging { get; init; }

    public string ConsoleOutputTemplate { get; init; } =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {RequestId} {Message:lj}{NewLine}{Exception}";

    public bool EnableConsoleJsonLogging { get; init; }

    public LogEventLevel ConsoleLogLevel { get; init; } = LogEventLevel.Information;

    public bool EnableFileLogging { get; init; }

    public LogEventLevel FileLogLevel { get; init; } = LogEventLevel.Information;

    public bool EnableOpenTelemetryLogging { get; init; }

    public LogEventLevel OpenTelemetryLogLevel { get; init; } = LogEventLevel.Information;

    public string? OpenTelemetryEndpoint { get; init; }

    public OtlpProtocol? OpenTelemetryProtocol { get; init; }

    public Dictionary<string, string>? OpenTelemetryHeaders { get; init; }

    public sealed class Validator : AbstractValidator<LogSettings>
    {
        public Validator()
        {
            When(
                x => x.EnableConsoleTextLogging,
                () =>
                {
                    RuleFor(x => x.ConsoleOutputTemplate).NotEmpty();

                    RuleFor(x => x.EnableConsoleJsonLogging)
                        .Must(x => !x)
                        .WithMessage(
                            $"{nameof(EnableConsoleJsonLogging)} can not be enabled when {nameof(EnableConsoleTextLogging)} is enabled."
                        );
                }
            );

            When(
                x => x.EnableOpenTelemetryLogging,
                () =>
                {
                    RuleFor(x => x.OpenTelemetryEndpoint).NotEmpty();

                    RuleFor(x => x.OpenTelemetryProtocol).NotNull().IsInEnum();
                }
            );
        }
    }
}
