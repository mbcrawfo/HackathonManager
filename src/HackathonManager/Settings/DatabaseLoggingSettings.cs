using FluentValidation;
using HackathonManager.Interfaces;

namespace HackathonManager.Settings;

public sealed class DatabaseLoggingSettings : IConfigurationSettings
{
    public bool EnableDetailedErrors { get; init; }

    public bool EnableSensitiveDataLogging { get; init; }

    /// <inheritdoc />
    public static string ConfigurationSection => "DatabaseLogging";
}

public sealed class DatabaseLoggingSettingsValidator : AbstractValidator<DatabaseLoggingSettings>;
