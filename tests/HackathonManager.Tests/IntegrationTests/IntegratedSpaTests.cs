using System.Net;
using System.Threading.Tasks;
using HackathonManager.Tests.TestInfrastructure;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests;

public class IntegratedSpaTests : IntegrationTestWithReset
{
    /// <inheritdoc />
    public IntegratedSpaTests(IntegrationTestWithResetFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task ShouldNotServeStaticFiles_WhenIntegratedSpaIsDisabled()
    {
        // arrange
        using var client = App.WithWebHostBuilder(b => b.UseSetting(ConfigurationKeys.EnableIntegratedSpaKey, "false"))
            .CreateClient();

        // act
        // assert
        foreach (var url in new[] { "/", "/index.html", "/test.js" })
        {
            var response = await client.GetAsync(url, CancellationToken);
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task ShouldServeStaticFiles_WhenIntegratedSpaIsEnabled()
    {
        // arrange
        var client = App.WithWebHostBuilder(b => b.UseSetting(ConfigurationKeys.EnableIntegratedSpaKey, "true"))
            .CreateClient();

        // act
        var indexResponse = await client.GetStringAsync("/index.html", CancellationToken);
        var jsResponse = await client.GetStringAsync("/test.js", CancellationToken);

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
        var client = App.WithWebHostBuilder(b => b.UseSetting(ConfigurationKeys.EnableIntegratedSpaKey, "true"))
            .CreateClient();

        // act
        var response = await client.GetStringAsync(route, CancellationToken);

        // assert
        response.ShouldContain("Hello test world");
    }
}
