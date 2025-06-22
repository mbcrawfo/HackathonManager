using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using OpenTelemetry.Exporter;

namespace HackathonManager.Settings;

public sealed class TracerSettings : IConfigurationSettings
{
    public bool Enabled { get; init; }

    public string OtlpEndpoint { get; init; } = "";

    public OtlpExportProtocol OtlpProtocol { get; init; }

    public Dictionary<string, string> OtlpHeaders { get; init; } = new();

    public static string ConfigurationSection => "Tracer";
}

public sealed class TracerSettingsValidator : AbstractValidator<TracerSettings>
{
    public TracerSettingsValidator()
    {
        When(
            x => x.Enabled,
            () =>
            {
                RuleFor(x => x.OtlpEndpoint).NotEmpty();
                RuleFor(x => x.OtlpProtocol).NotNull().IsInEnum();
                RuleForEach(x => x.OtlpHeaders)
                    .Must(kvp => !string.IsNullOrWhiteSpace(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value))
                    .WithMessage($"{nameof(TracerSettings.OtlpHeaders)} must contain non-empty keys and values.");
            }
        );
    }
}
