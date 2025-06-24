using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using FastEndpoints.Testing;
using HackathonManager.Database;
using HackathonManager.Extensions;
using HackathonManager.Settings;
using HackathonManager.Tests.TestInfrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HackathonManager.Tests.TestInfrastructure;

[SuppressMessage(
    "Critical Code Smell",
    "S927:Parameter names should match base declaration and other partial definitions"
)]
public abstract class AppFixtureBase : AppFixture<Program>
{
    protected AppFixtureBase(IDatabaseFixture databaseFixture)
    {
        DatabaseFixture = databaseFixture;
    }

    protected IDatabaseFixture DatabaseFixture { get; }

    public DbDataSource DataSource => DatabaseFixture.DataSource;

    /// <inheritdoc />
    protected override async ValueTask PreSetupAsync() => await DatabaseFixture.InitializeAsync();

    /// <inheritdoc />
    protected override void ConfigureApp(IWebHostBuilder builder)
    {
        builder.UseContentRoot(
            Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestInfrastructure", "IntegrationTestContentRoot")
        );

        builder.UseSetting(Constants.ConnectionStringKey, DatabaseFixture.ConnectionString);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbDataSource>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<DbContextOptions<HackathonDbContext>>();
            services.RemoveAll<HackathonDbContext>();

            services.AddSingleton<DbDataSource>(DatabaseFixture.DataSource);
            services.AddDbContext<HackathonDbContext>(ob =>
                ob.ConfigureHackathonDbContext(
                    DatabaseFixture.DataSource,
                    new DatabaseLoggingSettings { EnableDetailedErrors = true, EnableSensitiveDataLogging = true }
                )
            );
        });
    }

    /// <inheritdoc />
    protected override async ValueTask TearDownAsync() => await DatabaseFixture.DisposeAsync();
}
