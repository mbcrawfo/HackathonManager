using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Asp.Versioning;
using DotNetEnv;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using FastEndpoints.Swagger;
using HackathonManager;
using HackathonManager.Database;
using HackathonManager.Extensions;
using HackathonManager.Migrator;
using HackathonManager.Settings;
using HackathonManager.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Sqids;

Env.LoadMulti([".env", ".env.local"]);

var builder = WebApplication.CreateBuilder(args);

// Configuration system doesn't allow removing keys defined in earlier sources.  This little hack lets us provide a
// setting with a list of keys that we can null out (which is equivalent to removing them).
var keysToRemove = (builder.Configuration.GetValue<string>("KeysToRemove")?.Split(';') ?? []).SelectMany(k =>
    builder
        .Configuration.AsEnumerable()
        .Where(kvp => kvp.Key.StartsWith(k, StringComparison.OrdinalIgnoreCase))
        .Select(kvp => kvp.Key)
);
foreach (var key in keysToRemove)
{
    builder.Configuration[key] = null;
}

Log.Logger = SerilogConfiguration.CreateBootstrapLogger(builder.Configuration);
var logger = Log.Logger.ForContext<Program>();
logger.Information("{Application} version {Version} starting up...", AppInfo.Name, AppInfo.Version);

try
{
    VersionSets.CreateApi("Test", v => v.HasApiVersion(new ApiVersion(1.0)));

    ConfigureServices();

    var app = builder.Build();
    ConfigurePipeline(app);

    var enableStartupMigration = app.Configuration.GetValue<bool>(ConfigurationKeys.EnableStartupMigrationKey);
    logger.Information(
        "{SettingKey}={SettingValue}",
        ConfigurationKeys.EnableStartupMigrationKey,
        enableStartupMigration
    );
    if (enableStartupMigration)
    {
        using var scope = app.Services.CreateScope();
        MigrationRunner.UpdateDatabase(
            app.Configuration.GetRequiredValue<string>(ConfigurationKeys.ConnectionStringKey),
            scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        );
    }

    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    logger.Fatal(ex, "Unhandled exception during application");
    return 1;
}
finally
{
    logger.Information("{Application} shut down", AppInfo.Name);
    await Log.CloseAndFlushAsync();
}

void ConfigureServices()
{
    builder.Services.AddSerilog(
        (provider, config) => config.ConfigureAppLogger(provider.GetRequiredService<IConfiguration>()),
        preserveStaticLogger: true
    );

    AddOpenTelemetryServices();

    builder.Services.AddConfigurationSettings<RequestLoggingSettings>();

    builder.Services.AddSingleton<IClock>(SystemClock.Instance);

    var sqidsOptions = new SqidsOptions
    {
        // No need for this to be configuration, we are just providing a shuffled alphabet unique to our app.
        Alphabet = "1WDJVS5qETAiad0Fohz2OQyRH3vfbX9UkPLGwg8psrMIlK74BetZNYc6muxnjC",
        MinLength = 10,
    };
    builder.Services.AddSingleton(new SqidsEncoder<int>(sqidsOptions));
    builder.Services.AddSingleton(new SqidsEncoder<uint>(sqidsOptions));

    var connectionString = builder.Configuration.GetRequiredValue<string>(ConfigurationKeys.ConnectionStringKey);
    var databaseLoggingSettings = builder.Configuration.GetConfigurationSettings<
        DatabaseLoggingSettings,
        DatabaseLoggingSettingsValidator
    >();
    var dataSource = DataSourceFactory.Create(connectionString, databaseLoggingSettings);

    builder.Services.AddSingleton<DbDataSource>(dataSource);
    builder.Services.AddDbContext<HackathonDbContext>(ob =>
        ob.ConfigureHackathonDbContext(dataSource, databaseLoggingSettings)
    );

    builder.Services.AddHealthChecks();

    builder
        .Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            // TODO: Cookie settings
            options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
            options.SlidingExpiration = true;
        })
        .AddJwtBearer(_ =>
        {
            // TODO: Configure JWT Auth
        });

    builder.Services.AddAuthorization();

    builder.Services.AddFastEndpoints(o => o.IncludeAbstractValidators = true);

    builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.ConfigureSerializerOptions());

    builder.Services.AddVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1.0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new QueryStringApiVersionReader(),
            new HeaderApiVersionReader(),
            new MediaTypeApiVersionReader()
        );
    });

    builder.Services.SwaggerDocument(options =>
    {
        options.DocumentSettings = settings =>
        {
            settings.ApiVersion(new ApiVersion(1.0));
            settings.DocumentName = "v1";
            settings.SchemaSettings.TypeMappers.Add(new SwaggerTypeIdTypeMapper());
        };
        options.AutoTagPathSegmentIndex = 0;
        options.SerializerSettings = o => o.ConfigureSerializerOptions();
        options.ShortSchemaNames = true;
    });
}

void AddOpenTelemetryServices()
{
    var serviceName = builder.Configuration.GetValue<string>(ConfigurationKeys.ServiceNameKey) ?? AppInfo.Name;

    var settings = builder.Configuration.GetConfigurationSettings<
        OpenTelemetrySettings,
        OpenTelemetrySettingsValidator
    >();

    builder.Services.AddSingleton(TracerProvider.Default.GetTracer(serviceName));

    var openTelemetryBuilder = builder.Services.AddOpenTelemetry();

    openTelemetryBuilder.WithMetrics(mpb =>
    {
        var metricsExporter = settings.AllExporters ?? settings.MetricsExporter;
        if (metricsExporter?.Enabled is not true)
        {
            return;
        }

        mpb.ConfigureResource(rb =>
                rb.AddService(serviceName, serviceVersion: AppInfo.Version, serviceInstanceId: Environment.MachineName)
            )
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddNpgsqlInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(metricsExporter.Endpoint);
                options.Protocol = metricsExporter.Protocol;
                options.Headers = string.Join(",", metricsExporter.Headers.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            });
    });

    openTelemetryBuilder.WithTracing(tpb =>
    {
        var traceExporter = settings.AllExporters ?? settings.TraceExporter;
        if (traceExporter?.Enabled is not true)
        {
            return;
        }

        tpb.AddSource(serviceName)
            .ConfigureResource(rb =>
                rb.AddService(serviceName, serviceVersion: AppInfo.Version, serviceInstanceId: Environment.MachineName)
            )
            .AddAspNetCoreInstrumentation(config =>
            {
                config.EnrichWithHttpRequest = (activity, request) =>
                {
                    // Other log properties are automatically added by the instrumentation.
                    activity.SetTag(LogPropertyNames.RequestId, request.HttpContext.TraceIdentifier);
                };
            })
            .AddHttpClientInstrumentation()
            .AddNpgsql()
            .AddProcessor<HttpRouteTraceProcessor>()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(traceExporter.Endpoint);
                options.Protocol = traceExporter.Protocol;
                options.Headers = string.Join(",", traceExporter.Headers.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            });
    });
}

void ConfigurePipeline(WebApplication app)
{
    var enableIntegratedSpa = app.Configuration.GetValue<bool>(ConfigurationKeys.EnableIntegratedSpaKey);
    logger.Information("{SettingKey}={SettingValue}", ConfigurationKeys.EnableIntegratedSpaKey, enableIntegratedSpa);

    if (enableIntegratedSpa)
    {
        app.UseStaticFiles();
    }

    app.UseHealthChecks("/health");

    app.UseMiddleware<RequestLoggingMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseFastEndpoints(config =>
    {
        config.Binding.UsePropertyNamingPolicy = true;
        config.Endpoints.RoutePrefix = "api";
        config.Endpoints.ShortNames = true;
        config.Errors.StatusCode = StatusCodes.Status422UnprocessableEntity;
        config.Errors.UseProblemDetails(c => c.IndicateErrorCode = true);
        config.Validation.UsePropertyNamingPolicy = true;
        config.Versioning.PrependToRoute = true;
    });

    app.UseSwaggerGen();

    if (enableIntegratedSpa)
    {
        app.MapFallbackToFile("index.html");
    }
}

// Expose the Program class for integration testing.
[SuppressMessage("Major Code Smell", "S1118:Utility classes should not have public constructors")]
public partial class Program;
