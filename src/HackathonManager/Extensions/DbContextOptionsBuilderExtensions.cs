using EntityFramework.Exceptions.PostgreSQL;
using HackathonManager.Settings;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HackathonManager.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder ConfigureHackathonDbContext(
        this DbContextOptionsBuilder builder,
        NpgsqlDataSource dataSource,
        DatabaseLoggingSettings settings
    )
    {
        builder.UseNpgsql(dataSource, options => options.UseNodaTime());
        builder.UseSnakeCaseNamingConvention();
        builder.UseExceptionProcessor();

        builder.EnableSensitiveDataLogging(settings.EnableSensitiveDataLogging);
        builder.EnableDetailedErrors(settings.EnableDetailedErrors);

        return builder;
    }
}
