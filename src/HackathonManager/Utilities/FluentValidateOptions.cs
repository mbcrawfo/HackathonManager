using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using HackathonManager.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HackathonManager.Utilities;

public sealed class FluentValidateOptions<T> : IValidateOptions<T>
    where T : class
{
    private static readonly string TypeName = typeof(T).Name;
    private readonly string? _optionsName;

    private readonly IServiceProvider _serviceProvider;

    public FluentValidateOptions(IServiceProvider serviceProvider, string? optionsName)
    {
        _serviceProvider = serviceProvider;
        _optionsName = optionsName;
    }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, T options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (name is not null && name != _optionsName)
        {
            return ValidateOptionsResult.Skip;
        }

        using var scope = _serviceProvider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IValidator<T>>();

        if (validator.Validate(options) is { IsValid: false } result)
        {
            return ValidateOptionsResult.Fail(FormatValidationErrors(result));
        }

        return ValidateOptionsResult.Success;
    }

    public static IReadOnlyCollection<string> FormatValidationErrors(ValidationResult result) =>
        result.Errors.Select(f => $"{TypeName}: {f.ErrorMessage}").ToReadOnlyCollection();
}
