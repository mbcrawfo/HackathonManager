using HackathonManager.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HackathonManager.Extensions;

public static class OptionsBuilderExtensions
{
    public static OptionsBuilder<T> UseFluentValidation<T>(this OptionsBuilder<T> builder)
        where T : class
    {
        builder.Services.AddSingleton<IValidateOptions<T>>(provider => new FluentValidateOptions<T>(
            provider,
            builder.Name
        ));

        return builder;
    }
}
