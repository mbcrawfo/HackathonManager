using FluentValidation;

namespace HackathonManager.Settings;

public sealed class EntityFrameworkSettings : IConfigurationSettings
{
    public bool EnableDetailedErrors { get; init; }

    public bool EnableSensitiveDataLogging { get; init; }

    /// <inheritdoc />
    public static string ConfigurationSection => "EntityFramework";
}

public sealed class EntityFrameworkSettingsValidator : AbstractValidator<EntityFrameworkSettings>
{
    public EntityFrameworkSettingsValidator() { }
}
