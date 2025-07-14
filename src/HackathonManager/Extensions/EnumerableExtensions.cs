using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HackathonManager.Interfaces;

namespace HackathonManager.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    ///     Generates an ETag from a collection of versioned resources.
    /// </summary>
    /// <param name="enumerable"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static uint GenerateETag<T>(this IEnumerable<T> enumerable)
        where T : IRowVersion
    {
        var hash = new HashCode();
        foreach (var item in enumerable)
        {
            hash.Add(item.RowVersion);
        }

        return (uint)hash.ToHashCode();
    }

    /// <summary>
    ///     Converts an <see cref="IEnumerable{T}" /> to <see cref="IReadOnlyCollection{T}" />, avoiding a redundant
    ///     copy of the data when possible.
    /// </summary>
    /// <param name="enumerable"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
    {
        return enumerable switch
        {
            ReadOnlyCollection<T> collection => collection,
            ICollection<T> { Count: 0 } => Array.Empty<T>(),
            IList<T> list => new ReadOnlyCollection<T>(list),
            _ => enumerable.ToArray(),
        };
    }

    /// <summary>
    ///     Filters an <see cref="IEnumerable{T}" /> of reference types to exclude the null items.
    /// </summary>
    /// <param name="enumerable"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [SuppressMessage(
        "Minor Code Smell",
        "S1905:Redundant casts should not be used",
        Justification = "Cast is required to remove the nullability of collection items."
    )]
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
        where T : class => (IEnumerable<T>)enumerable.Where(x => x is not null);

    /// <summary>
    ///     Filters an <see cref="IEnumerable{T}" /> of nullable value types to exclude the null items.
    /// </summary>
    /// <param name="enumerable"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
        where T : struct => enumerable.Where(x => x.HasValue).Select(x => x!.Value);
}
