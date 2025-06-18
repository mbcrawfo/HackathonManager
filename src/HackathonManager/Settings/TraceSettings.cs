using System.Collections.Generic;
using FluentValidation;
using JetBrains.Annotations;
using OpenTelemetry.Exporter;

namespace HackathonManager.Settings;

public class TraceSettings
{
    public const string ConfigurationSection = "Trace";

    public bool Enabled { get; init; }

    public string? OtlpEndpoint { get; init; }

    public OtlpExportProtocol? OtlpProtocol { get; init; }

    public Dictionary<string, string>? OtlpHeaders { get; init; }

    public bool EnableUrlQueryRedaction { get; init; }
}

[UsedImplicitly]
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
                    .WithMessage($"{nameof(TraceSettings.OtlpHeaders)} must contain non-empty keys and values.")
                    .When(x => x.OtlpHeaders is { Count: > 0 });
            }
        );
    }
}
