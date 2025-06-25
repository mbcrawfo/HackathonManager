using System;
using System.Collections.Generic;
using FluentValidation;
using OpenTelemetry.Exporter;

namespace HackathonManager.Settings;

public sealed class OpenTelemetryExporterSettings
{
    public bool Enabled { get; init; }

    public string Endpoint { get; init; } = "";

    public OtlpExportProtocol Protocol { get; init; }

    public Dictionary<string, string> Headers { get; init; } = new();
}

public sealed class OpenTelemetryExporterSettingsValidator : AbstractValidator<OpenTelemetryExporterSettings>
{
    public OpenTelemetryExporterSettingsValidator()
    {
        When(
            x => x.Enabled,
            () =>
            {
                RuleFor(x => x.Endpoint)
                    .Must(s => Uri.TryCreate(s, UriKind.Absolute, out _))
                    .WithMessage("{PropertyName} must be a valid URI.");

                RuleFor(x => x.Protocol).NotNull().IsInEnum();

                RuleForEach(x => x.Headers)
                    .Must(kvp => !string.IsNullOrWhiteSpace(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value))
                    .WithMessage("{PropertyName} must contain non-empty keys and values.");
            }
        );
    }
}
