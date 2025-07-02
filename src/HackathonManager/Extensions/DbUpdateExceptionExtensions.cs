using System.Diagnostics;
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

    public static bool IsUniqueConstraintViolation(this DbUpdateException ex) =>
        ex.GetPostgresException().SqlState == "23505";
}
