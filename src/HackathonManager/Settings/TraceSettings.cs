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

    public string? OtlpHeaders { get; init; }
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
            }
        );
    }
}
