using System;
using FastIDs.TypeId;
using FluentValidation;

namespace HackathonManager.Extensions;

public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, string?> MustBeValidTypeId<T>(
        this IRuleBuilder<T, string?> builder,
        string prefix
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(prefix);

        const string errorCode = "typeid_invalid";
        var message = $"'{{PropertyName}}' must be a valid type id for '{prefix}'.";

        return builder
            .NotEmpty()
            .DependentRules(() =>
            {
                builder
                    .Must(x => x!.StartsWith($"{prefix}_"))
                    .WithErrorCode(errorCode)
                    .WithMessage(message)
                    .DependentRules(() =>
                    {
                        builder.Must(x => TypeId.TryParse(x!, out _)).WithErrorCode(errorCode).WithMessage(message);
                    });
            });
    }
}
