using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Sqids;

namespace HackathonManager.Extensions;

public static class SqidsEncoderExtensions
{
    /// <summary>
    ///     Decodes a sqid containing a single value and validates that it is canonical (the output encodes back to the
    ///     same value as the input).
    /// </summary>
    /// <param name="encoder"></param>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool DecodeAndValidate<T>(
        this SqidsEncoder<T> encoder,
        ReadOnlySpan<char> input,
        [NotNullWhen(true)] out T? output
    )
        where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T>
    {
        output = null;

        if (encoder.Decode(input) is not [var value])
        {
            return false;
        }

        if (!input.SequenceEqual(encoder.Encode(value).AsSpan()))
        {
            return false;
        }

        output = value;
        return true;
    }
}
