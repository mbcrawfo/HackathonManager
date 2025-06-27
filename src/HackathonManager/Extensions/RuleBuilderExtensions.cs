using System;
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

        var expectedPrefix = $"{prefix}_";
        var expectedLength = expectedPrefix.Length + 26;
        var message = $"'{{PropertyName}}' must be a string in the format '{prefix}_01h93ech7jf5ktdwg6ye383x34'.";

        return builder
            .NotNullWithCode()
            .DependentRules(() =>
                builder
                    .Must(x => x?.Length == expectedLength && x.StartsWith(prefix))
                    .WithErrorCode(ErrorCodes.Validation.TypeIdInvalidFormat)
                    .WithMessage(message)
            );
    }

    public static IRuleBuilderOptions<T, string> MaxLengthWithCode<T>(
        this IRuleBuilder<T, string> builder,
        int maxLength
    ) => builder.MaximumLength(maxLength).WithErrorCode(ErrorCodes.Validation.MaxLength);

    public static IRuleBuilderOptions<T, TProperty> NotNullWithCode<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder
    ) => builder.NotNull().WithErrorCode(ErrorCodes.Validation.NotNull);

    public static IRuleBuilderOptions<T, TProperty> NotNullOrEmptyWithCode<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder
    ) =>
        builder
            .NotNullWithCode()
            .DependentRules(() => builder.NotEmpty().WithErrorCode(ErrorCodes.Validation.NotEmpty));
}
