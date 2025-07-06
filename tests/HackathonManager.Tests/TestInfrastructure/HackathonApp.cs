using System;
using System.Data.Common;
using System.IO;
using HackathonManager.Extensions;
using HackathonManager.Persistence;
using HackathonManager.Settings;
using HackathonManager.Tests.TestInfrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HackathonManager.Tests.TestInfrastructure;

public sealed class HackathonApp : WebApplicationFactory<Program>
{
    public IDatabaseFixture? Database { get; set; }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (Database is null)
        {
            throw new InvalidOperationException(
                $"{nameof(Database)} must be set before {nameof(HackathonApp)} can be used."
            );
        }

        builder.UseContentRoot(
            Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestInfrastructure", "IntegrationTestContentRoot")
        );

        builder.UseSetting(ConfigurationKeys.ConnectionStringKey, Database.ConnectionString);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbDataSource>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<DbContextOptions<HackathonDbContext>>();
            services.RemoveAll<HackathonDbContext>();

            services.AddSingleton<DbDataSource>(Database.DataSource);
            services.AddDbContext<HackathonDbContext>(ob =>
                ob.ConfigureHackathonDbContext(
                    Database.DataSource,
                    new DatabaseLoggingSettings { EnableDetailedErrors = true, EnableSensitiveDataLogging = true }
                )
            );
        });
    }
}
