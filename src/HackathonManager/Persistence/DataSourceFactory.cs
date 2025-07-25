using HackathonManager.Settings;
using Npgsql;

namespace HackathonManager.Persistence;

public static class DataSourceFactory
{
    public static NpgsqlDataSource Create(string connectionString, DatabaseLoggingSettings loggingSettings) =>
        new NpgsqlDataSourceBuilder(connectionString)
            .UseNodaTime()
            .EnableParameterLogging(loggingSettings.EnableSensitiveDataLogging)
            .ConfigureTracing(ob =>
                ob.EnableFirstResponseEvent(false).ConfigureCommandSpanNameProvider(c => c.CommandText)
            )
            .Build();
}
