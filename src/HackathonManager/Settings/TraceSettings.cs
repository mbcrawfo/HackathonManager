using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using OpenTelemetry.Exporter;

namespace HackathonManager.Settings;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public sealed class TraceSettings : IConfigurationSettings
{
    public bool Enabled { get; init; }

    public string OtlpEndpoint { get; init; } = "";

    public OtlpExportProtocol OtlpProtocol { get; init; }

    // ReSharper disable once CollectionNeverUpdated.Global
    public Dictionary<string, string> OtlpHeaders { get; init; } = new();

    public bool EnableUrlQueryRedaction { get; init; }

    public static string ConfigurationSection => "Trace";
}

public sealed class TraceSettingsValidator : AbstractValidator<TraceSettings>
{
    public TraceSettingsValidator()
    {
        When(
            x => x.Enabled,
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
