using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;

namespace HackathonManager.Settings;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class LogSettings : IConfigurationSettings
{
    public bool EnableOpenTelemetryLogging { get; init; }

    public LogEventLevel OpenTelemetryLogLevel { get; init; } = LogEventLevel.Verbose;

    public string? OtlpEndpoint { get; init; }

    public OtlpProtocol? OtlpProtocol { get; init; }

    public Dictionary<string, string> OtlpHeaders { get; init; } = new();

    public static string ConfigurationSection => "Log";
}

public sealed class LogSettingsValidator : AbstractValidator<LogSettings>
{
    public LogSettingsValidator()
    {
        When(
            x => x.EnableOpenTelemetryLogging,
            () =>
            {
                RuleFor(x => x.OtlpEndpoint).NotEmpty();
                RuleFor(x => x.OtlpProtocol).NotNull().IsInEnum();
                RuleForEach(x => x.OtlpHeaders)
                    .Must(kvp => !string.IsNullOrWhiteSpace(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value))
                    .WithMessage($"{nameof(TraceSettings.OtlpHeaders)} must contain non-empty keys and values.");
            }
        );
    }
}
