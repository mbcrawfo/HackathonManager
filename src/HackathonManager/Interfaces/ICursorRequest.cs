using FluentValidation;
using HackathonManager.Extensions;

namespace HackathonManager.Interfaces;

public interface ICursorRequest
{
    public const int CursorMaxLength = 1000;

    /// <summary>
    ///     Search cursor to retrieve results.  When provided, the cursor overrides all other search, sort, and
    ///     pagination parameters.
    /// </summary>
    string? Cursor { get; }
}

public static class CursorRequestValidatorExtensions
{
    public static void AddCursorRules<T>(this AbstractValidator<T> validator)
        where T : ICursorRequest
    {
        validator
            .RuleFor(x => x.Cursor)
            .NotEmptyWithCode()
            .MaximumLengthWithCode(ICursorRequest.CursorMaxLength)
            .When(x => x.Cursor is not null);
    }
}
