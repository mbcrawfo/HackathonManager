using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HackathonManager.Exceptions;

/// <summary>
///     Exception to be thrown as a guard against unexpected enum values.  Use in the default case of switch statements
///     that should handle all enum values.
/// </summary>
/// <typeparam name="T"></typeparam>
[SuppressMessage(
    "Roslynator",
    "RCS1194:Implement exception constructors",
    Justification = "Exception uses fixed messages."
)]
public sealed class UnexpectedEnumValueException<T> : Exception
    where T : Enum
{
    public UnexpectedEnumValueException(
        object? value,
        Exception? innerException = null,
        [CallerArgumentExpression(nameof(value))] string paramName = ""
    )
        : base(GetMessage(value, paramName), innerException) { }

    private static string GetMessage(object? value, string paramName)
    {
        if (value is null || !Enum.IsDefined(typeof(T), value))
        {
            return $"Value '{value}' is not valid for enum '{typeof(T).Name}'.  Value source: {paramName}";
        }

        return $"'{Enum.GetName(typeof(T), value)}' is valid for enum '{typeof(T).Name}' but was not handled.  Value source: {paramName}";
    }
}
