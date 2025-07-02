using System;
using FastIDs.TypeId;
using FluentValidation;
using Sqids;

namespace HackathonManager.Extensions;

public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, string> EmailAddressWithCode<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder.EmailAddress().WithErrorCode(ErrorCodes.EmailAddress);

    public static IRuleBuilderOptions<T, string> LengthWithCode<T>(
        this IRuleBuilder<T, string> builder,
        int min,
        int max
    ) => builder.Length(min, max).WithErrorCode(ErrorCodes.InvalidLength);

    public static IRuleBuilderOptions<T, string> MustBeValidETag<T>(
        this IRuleBuilder<T, string> builder,
        SqidsEncoder<uint> encoder
    ) =>
        builder
            .Must(s => encoder.Decode(s) is [var etag] && encoder.Encode(etag) == s)
            .WithErrorCode(ErrorCodes.InvalidETag);

    public static IRuleBuilderOptions<T, TypeId> MustBeIdOfType<T>(
        this IRuleBuilder<T, TypeId> builder,
        string typePrefix
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(typePrefix);

        return builder
            .Must(x => x.Type.SequenceEqual(typePrefix))
            .WithErrorCode(ErrorCodes.TypeIdInvalid)
            .WithMessage(InvalidTypeIdMessage(typePrefix));
    }

    public static IRuleBuilderOptions<T, TypeId?> MustBeIdOfType<T>(
        this IRuleBuilder<T, TypeId?> builder,
        string typePrefix
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(typePrefix);

        return builder
            .Must(x => x.HasValue && x.Value.Type.SequenceEqual(typePrefix))
            .WithErrorCode(ErrorCodes.TypeIdInvalid)
            .WithMessage(InvalidTypeIdMessage(typePrefix));
    }

    public static IRuleBuilderOptions<T, string?> MaximumLengthWithCode<T>(
        this IRuleBuilder<T, string?> builder,
        int maxLength
    ) => builder.MaximumLength(maxLength).WithErrorCode(ErrorCodes.MaxLength);

    public static IRuleBuilderOptions<T, TProperty> NotNullWithCode<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder
    ) => builder.NotNull().WithErrorCode(ErrorCodes.NotNull);

    public static IRuleBuilderOptions<T, TProperty> NotNullOrEmptyWithCode<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder
    ) => builder.NotNullWithCode().DependentRules(() => builder.NotEmpty().WithErrorCode(ErrorCodes.NotEmpty));

    private static string InvalidTypeIdMessage(string prefix) =>
        $"'{{PropertyName}}' must be a string in the format '{prefix}_01h93ech7jf5ktdwg6ye383x34'.";
}
