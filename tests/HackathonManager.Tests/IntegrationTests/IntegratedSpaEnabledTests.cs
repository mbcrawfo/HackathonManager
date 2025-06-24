using System.Threading.Tasks;
using FastEndpoints.Testing;
using HackathonManager.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests;

public class IntegratedSpaEnabledTests : TestBase<IntegratedSpaEnabledTests.AppWithSpaEnabled>
{
    private readonly AppWithSpaEnabled _app;

    public IntegratedSpaEnabledTests(AppWithSpaEnabled app)
    {
        _app = app;
    }

    [Fact]
    public async Task ShouldServeStaticFiles_WhenIntegratedSpaIsEnabled()
    {
        // arrange
        // act
        var indexResponse = await _app.Client.GetStringAsync("/index.html", Cancellation);
        var jsResponse = await _app.Client.GetStringAsync("/test.js", Cancellation);

        // assert
        indexResponse.ShouldContain("Hello test world");
        jsResponse.ShouldContain("Placeholder javascript");
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/path/to/page")]
    [InlineData("/path?queryParams=true")]
    public async Task ShouldUseFallbackRoute_WhenIntegratedSpaIsEnabled(string route)
    {
        // arrange
        // act
        var response = await _app.Client.GetStringAsync(route, Cancellation);

        // assert
        response.ShouldContain("Hello test world");
    }

    public sealed class AppWithSpaEnabled : AppWithDatabase
    {
        /// <inheritdoc />
        protected override void ConfigureApp(IWebHostBuilder builder)
        {
            base.ConfigureApp(builder);
            builder.UseSetting(Constants.EnableIntegratedSpaKey, "true");
        }
    }
}
