using System.Net;
using System.Threading.Tasks;
using HackathonManager.Tests.TestInfrastructure;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests;

public class IntegratedSpaTests : IntegrationTestsBase
{
    [Theory]
    [InlineData("/")]
    [InlineData("/index.html")]
    [InlineData("/test.js")]
    public async Task ShouldNotServeStaticFiles_WhenIntegratedSpaIsDisabled(string url)
    {
        // arrange
        using var client = Factory
            .WithWebHostBuilder(builder => builder.UseSetting("EnableIntegratedSpa", "false"))
            .CreateClient();

        // act
        var response = await client.GetAsync(url, CancellationToken);

        // assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldServeStaticFiles_WhenIntegratedSpaIsEnabled()
    {
        // arrange
        using var client = Factory
            .WithWebHostBuilder(builder => builder.UseSetting(Constants.EnableIntegratedSpaKey, "true"))
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
        using var client = Factory
            .WithWebHostBuilder(builder => builder.UseSetting(Constants.EnableIntegratedSpaKey, "true"))
            .CreateClient();

        // act
        var response = await client.GetStringAsync(route, CancellationToken);

        // assert
        response.ShouldContain("Hello test world");
    }
}
