using FluentValidation;
using Serilog.Events;

namespace HackathonManager.Settings;

public sealed class OpenTelemetrySettings : IConfigurationSettings
{
    public LogEventLevel LogLevel { get; init; } = LogEventLevel.Verbose;

    public OpenTelemetryExporterSettings? AllExporters { get; init; }

    public OpenTelemetryExporterSettings? LogExporter { get; init; }

    public OpenTelemetryExporterSettings? MetricsExporter { get; init; }

    public OpenTelemetryExporterSettings? TraceExporter { get; init; }

    /// <inheritdoc />
    public static string ConfigurationSection => "OpenTelemetry";
}

public sealed class OpenTelemetrySettingsValidator : AbstractValidator<OpenTelemetrySettings>
{
    public OpenTelemetrySettingsValidator()
    {
        RuleFor(x => x.LogLevel).IsInEnum();

        var exporterSettingsValidator = new OpenTelemetryExporterSettingsValidator();

        RuleFor(x => x.AllExporters).SetValidator(exporterSettingsValidator!).When(x => x.AllExporters is not null);

        RuleFor(x => x.LogExporter).SetValidator(exporterSettingsValidator!).When(x => x.LogExporter is not null);

        RuleFor(x => x.MetricsExporter)
            .SetValidator(exporterSettingsValidator!)
            .When(x => x.MetricsExporter is not null);

        RuleFor(x => x.TraceExporter).SetValidator(exporterSettingsValidator!).When(x => x.TraceExporter is not null);
    }
}
