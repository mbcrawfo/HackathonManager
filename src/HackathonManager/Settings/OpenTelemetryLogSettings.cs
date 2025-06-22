using System.Collections.Generic;
using FluentValidation;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;

namespace HackathonManager.Settings;

public sealed class OpenTelemetryLogSettings : IConfigurationSettings
{
    public bool Enabled { get; init; }

    public LogEventLevel Level { get; init; } = LogEventLevel.Verbose;

    public string OtlpEndpoint { get; init; } = "";

    public OtlpProtocol OtlpProtocol { get; init; }

    public Dictionary<string, string> OtlpHeaders { get; init; } = new();

    public static string ConfigurationSection => "OpenTelemetryLog";
}

public sealed class OpenTelemetryLogSettingsValidator : AbstractValidator<OpenTelemetryLogSettings>
{
    public OpenTelemetryLogSettingsValidator()
    {
        When(
            x => x.Enabled,
            () =>
            {
                RuleFor(x => x.Level).IsInEnum();
                RuleFor(x => x.OtlpEndpoint).NotEmpty();
                RuleFor(x => x.OtlpProtocol).IsInEnum();
                RuleForEach(x => x.OtlpHeaders)
                    .Must(kvp => !string.IsNullOrWhiteSpace(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value))
                    .WithMessage($"{nameof(TracerSettings.OtlpHeaders)} must contain non-empty keys and values.");
            }
        );
    }
}
