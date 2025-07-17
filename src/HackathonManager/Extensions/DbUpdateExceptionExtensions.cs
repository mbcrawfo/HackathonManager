using System;
using System.Diagnostics;
using HackathonManager.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HackathonManager.Extensions;

public static class DbUpdateExceptionExtensions
{
    public static PostgresException GetPostgresException(this DbUpdateException ex)
    {
        Debug.Assert(ex.InnerException is not null);
        return (PostgresException)ex.InnerException;
    }

    public static string GetUniqueConstraintPropertyName<T>(this DbUpdateException ex)
        where T : IUniqueConstraintMapping
    {
        var constrainName = ex.GetPostgresException().ConstraintName;
        if (
            string.IsNullOrEmpty(constrainName)
            || !T.UniqueConstraintMappings.TryGetValue(constrainName, out var result)
        )
        {
            throw new InvalidOperationException("Unexpected unique constraint error", ex);
        }

        return result;
    }

    public static bool IsUniqueConstraintViolation(this DbUpdateException ex) =>
        ex.GetPostgresException().SqlState == "23505";
}
