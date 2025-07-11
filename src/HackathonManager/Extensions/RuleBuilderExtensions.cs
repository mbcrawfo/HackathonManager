using System;
using FastIDs.TypeId;
using FluentValidation;

namespace HackathonManager.Extensions;

public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, TEnum> IsInEnumWithCode<T, TEnum>(this IRuleBuilder<T, TEnum> ruleBuilder) =>
        ruleBuilder.IsInEnum().WithErrorCode(ErrorCodes.InvalidEnumValue);

    public static IRuleBuilderOptions<T, string> EmailAddressWithCode<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder.EmailAddress().WithErrorCode(ErrorCodes.EmailAddress);

    public static IRuleBuilderOptions<T, TProperty> InclusiveBetweenWithCode<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        TProperty min,
        TProperty max
    )
        where TProperty : IComparable<TProperty>, IComparable =>
        ruleBuilder.InclusiveBetween(min, max).WithErrorCode(ErrorCodes.MustBeInRange);

    public static IRuleBuilderOptions<T, string> LengthWithCode<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int min,
        int max
    ) => ruleBuilder.Length(min, max).WithErrorCode(ErrorCodes.InvalidLength);

    public static IRuleBuilderOptions<T, TypeId> MustBeIdOfType<T>(
        this IRuleBuilder<T, TypeId> ruleBuilder,
        string typePrefix
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(typePrefix);

        return ruleBuilder
            .Must(x => x.Type.SequenceEqual(typePrefix))
            .WithErrorCode(ErrorCodes.TypeIdInvalid)
            .WithMessage(InvalidTypeIdMessage(typePrefix));
    }

    public static IRuleBuilderOptions<T, TypeId?> MustBeIdOfType<T>(
        this IRuleBuilder<T, TypeId?> ruleBuilder,
        string typePrefix
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(typePrefix);

        return ruleBuilder
            .Must(x => x.HasValue && x.Value.Type.SequenceEqual(typePrefix))
            .WithErrorCode(ErrorCodes.TypeIdInvalid)
            .WithMessage(InvalidTypeIdMessage(typePrefix));
    }

    public static IRuleBuilderOptions<T, string?> MaximumLengthWithCode<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        int maxLength
    ) => ruleBuilder.MaximumLength(maxLength).WithErrorCode(ErrorCodes.MaxLength);

    public static IRuleBuilderOptions<T, TProperty> NotEmptyWithCode<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder
    ) => ruleBuilder.NotEmpty().WithErrorCode(ErrorCodes.NotEmpty);

    public static IRuleBuilderOptions<T, TProperty> NotNullWithCode<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder
    ) => ruleBuilder.NotNull().WithErrorCode(ErrorCodes.NotNull);

    public static IRuleBuilderOptions<T, TProperty> NullWithCode<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder
    ) => ruleBuilder.Null().WithErrorCode(ErrorCodes.MustBeNull);

    public static IRuleBuilderOptions<T, TProperty> NotNullOrEmptyWithCode<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder
    ) => ruleBuilder.NotNullWithCode().DependentRules(() => ruleBuilder.NotEmpty().WithErrorCode(ErrorCodes.NotEmpty));

    private static string InvalidTypeIdMessage(string prefix) =>
        $"'{{PropertyName}}' must be a string in the format '{prefix}_01h93ech7jf5ktdwg6ye383x34'.";
}
