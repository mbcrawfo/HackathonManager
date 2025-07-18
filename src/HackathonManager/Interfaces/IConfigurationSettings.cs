namespace HackathonManager.Interfaces;

/// <summary>
///     Marker interface to allow settings classes to be used with common helper methods.
/// </summary>
public interface IConfigurationSettings
{
    /// <summary>
    ///     Gets the name of the configuration section the settings are loaded from.
    /// </summary>
    public static abstract string ConfigurationSection { get; }
}
