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

    public LogEventLevel FileLogLevel { get; init; } = LogEventLevel.Verbose;

    public bool EnableOpenTelemetryLogging { get; init; }

    public LogEventLevel OpenTelemetryLogLevel { get; init; } = LogEventLevel.Verbose;

    public string? OtlpEndpoint { get; init; }

    public OtlpProtocol? OtlpProtocol { get; init; }

    public Dictionary<string, string>? OtlpHeaders { get; init; }
}

public sealed class LogSettingsValidator : AbstractValidator<LogSettings>
{
    public LogSettingsValidator()
    {
        When(
            x => x.EnableConsoleTextLogging,
            () =>
            {
                RuleFor(x => x.ConsoleOutputTemplate).NotEmpty();

                RuleFor(x => x.EnableConsoleJsonLogging)
                    .Must(x => !x)
                    .WithMessage(
                        $"{nameof(LogSettings.EnableConsoleJsonLogging)} can not be enabled when {nameof(LogSettings.EnableConsoleTextLogging)} is enabled."
                    );
            }
        );

        When(
            x => x.EnableOpenTelemetryLogging,
            () =>
            {
                RuleFor(x => x.OtlpEndpoint).NotEmpty();
                RuleFor(x => x.OtlpProtocol).NotNull().IsInEnum();
                RuleForEach(x => x.OtlpHeaders)
                    .Must(kvp => !string.IsNullOrWhiteSpace(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value))
                    .WithMessage($"{nameof(TraceSettings.OtlpHeaders)} must contain non-empty keys and values.")
                    .When(x => x.OtlpHeaders is { Count: > 0 });
            }
        );
    }
}
