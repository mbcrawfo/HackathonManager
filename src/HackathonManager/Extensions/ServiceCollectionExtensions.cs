using HackathonManager.Interfaces;
using HackathonManager.Settings;
using HackathonManager.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HackathonManager.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers a settings type that implements <see cref="IConfigurationSettings" /> to be loaded from
    ///     configuration and validated on start using FluentValidation.
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TSettings"></typeparam>
    public static void AddConfigurationSettings<TSettings>(this IServiceCollection services)
        where TSettings : class, IConfigurationSettings, new()
    {
        services.AddOptions<TSettings>().BindConfiguration(TSettings.ConfigurationSection).ValidateOnStart();

        services.AddSingleton<IValidateOptions<TSettings>>(provider => new FluentValidateOptions<TSettings>(
            provider,
            optionsName: null
        ));
    }
}
