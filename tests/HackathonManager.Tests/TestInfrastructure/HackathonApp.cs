using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using FastEndpoints.Testing;
using HackathonManager.Extensions;
using HackathonManager.Persistence;
using HackathonManager.Settings;
using HackathonManager.Tests.TestInfrastructure.Database;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HackathonManager.Tests.TestInfrastructure;

[UsedImplicitly]
public class HackathonApp : AppFixture<Program>
{
    private readonly IEnumerable<KeyValuePair<string, string>> _additionalSettings;

    public HackathonApp()
        : this([]) { }

    protected HackathonApp(IEnumerable<KeyValuePair<string, string>> additionalSettings)
    {
        _additionalSettings = additionalSettings;
    }

    public IDatabaseFixture Database { get; protected init; } = new DatabaseFixture();

    /// <inheritdoc />
    protected override async ValueTask PreSetupAsync() => await Database.InitializeAsync();

    /// <inheritdoc />
    [SuppressMessage(
        "Critical Code Smell",
        "S927:Parameter names should match base declaration and other partial definitions"
    )]
    protected override void ConfigureApp(IWebHostBuilder builder)
    {
        builder.UseContentRoot(
            Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestInfrastructure", "IntegrationTestContentRoot")
        );

        builder.UseSetting(ConfigurationKeys.ConnectionStringKey, Database.ConnectionString);

        foreach (var (key, value) in _additionalSettings)
        {
            builder.UseSetting(key, value);
        }

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

    /// <inheritdoc />
    protected override async ValueTask TearDownAsync() => await Database.DisposeAsync();
}
