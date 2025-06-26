using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using HackathonManager.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HackathonManager.Utilities;

/// <summary>
///     Uses Fluent Validation to validate an options type.  Expects there to be an <see cref="IValidator{T}" /> type
///     available through the app's service provider.
/// </summary>
/// <typeparam name="TOptions"></typeparam>
public sealed class FluentValidateOptions<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
    private static readonly string TypeName = typeof(TOptions).Name;
    private readonly string? _optionsName;

    private readonly IServiceProvider _serviceProvider;

    public FluentValidateOptions(IServiceProvider serviceProvider, string? optionsName)
    {
        _serviceProvider = serviceProvider;
        _optionsName = optionsName;
    }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (name is not null && name != _optionsName)
        {
            return ValidateOptionsResult.Skip;
        }

        using var scope = _serviceProvider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IValidator<TOptions>>();

        if (validator.Validate(options) is { IsValid: false } result)
        {
            return ValidateOptionsResult.Fail(FormatValidationErrors(result));
        }

        return ValidateOptionsResult.Success;
    }

    public static IReadOnlyCollection<string> FormatValidationErrors(ValidationResult result) =>
        result.Errors.Select(f => $"{TypeName}: {f.ErrorMessage}").ToReadOnlyCollection();
}
