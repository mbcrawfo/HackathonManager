using System;
using FluentValidation;
using HackathonManager.Settings;
using HackathonManager.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HackathonManager.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    ///     Gets a value from configuration, throwing an exception if the key is not set.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configuration" /> is null or <paramref name="key" /> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     <paramref name="key" /> was not found in the <paramref name="configuration" />.
    /// </exception>
    public static T GetRequiredValue<T>(this IConfiguration configuration, string key)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(key);

        return configuration.GetValue<T>(key)
            ?? throw new InvalidOperationException($"Required configuration '{key}' is missing.");
    }

    /// <summary>
    ///     Reads an <see cref="IConfigurationSettings" /> from configuration, using default values if its section is
    ///     not set.
    /// </summary>
    /// <param name="configuration"></param>
    /// <typeparam name="TSettings"></typeparam>
    /// <typeparam name="TValidator"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configuration" /> is null.
    /// </exception>
    /// <exception cref="OptionsValidationException">
    ///     The loaded settings failed validation.
    /// </exception>
    public static TSettings GetConfigurationSettings<TSettings, TValidator>(this IConfiguration configuration)
        where TSettings : class, IConfigurationSettings, new()
        where TValidator : AbstractValidator<TSettings>, new()
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var settings = configuration.GetSection(TSettings.ConfigurationSection).Get<TSettings>();
        if (settings is null)
        {
            return new TSettings();
        }

        if (new TValidator().Validate(settings) is { IsValid: false } validationResult)
        {
            throw new OptionsValidationException(
                typeof(TSettings).Name,
                typeof(TSettings),
                FluentValidateOptions<TSettings>.FormatValidationErrors(validationResult)
            );
        }

        return settings;
    }
}
