using System;
using FastIDs.TypeId;
using FluentValidation;

namespace HackathonManager.Extensions;

public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, string?> MustBeValidTypeIdWithCode<T>(
        this IRuleBuilder<T, string?> builder,
        string prefix
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(prefix);

        var message = $"'{{PropertyName}}' must be a string in the format '{prefix}_01h93ech7jf5ktdwg6ye383x34'.";

        return builder
            .NotEmptyWithCode()
            .DependentRules(() =>
                builder
                    .Must(x => x!.StartsWith($"{prefix}_") && TypeId.TryParse(x, out _))
                    .WithErrorCode(ValidationErrorCodes.TypeIdInvalid)
                    .WithMessage(message)
            );
    }

    public static IRuleBuilderOptions<T, string> MaxLengthWithCode<T>(
        this IRuleBuilder<T, string> builder,
        int maxLength
    ) => builder.MaximumLength(maxLength).WithErrorCode(ValidationErrorCodes.MaxLength);

    public static IRuleBuilderOptions<T, TProperty> NotEmptyWithCode<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder
    ) => builder.NotEmpty().WithErrorCode(ValidationErrorCodes.NotEmpty);
}
