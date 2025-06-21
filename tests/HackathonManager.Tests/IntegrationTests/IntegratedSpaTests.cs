using System.Net;
using System.Threading.Tasks;
using HackathonManager.Tests.TestInfrastructure;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests;

public class IntegratedSpaTests : IntegrationTestsBase
{
    [Fact]
    public async Task ShouldNotServeStaticFiles_WhenIntegratedSpaIsDisabled()
    {
        // arrange
        using var client = Factory
            .WithWebHostBuilder(builder => builder.UseSetting("EnableIntegratedSpa", "false"))
            .CreateClient();

        // act
        var defaultResponse = await client.GetAsync("/", TestContext.Current.CancellationToken);
        var indexResponse = await client.GetAsync("/index.html", TestContext.Current.CancellationToken);
        var jsResponse = await client.GetAsync("/test.js", TestContext.Current.CancellationToken);

        // assert
        defaultResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        indexResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        jsResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldServeStaticFiles_WhenIntegratedSpaIsEnabled()
    {
        // arrange
        using var client = Factory
            .WithWebHostBuilder(builder => builder.UseSetting("EnableIntegratedSpa", "true"))
            .CreateClient();

        // act
        var indexResponse = await client.GetAsync("/index.html", TestContext.Current.CancellationToken);
        indexResponse.EnsureSuccessStatusCode();
        var indexContent = await indexResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var jsResponse = await client.GetAsync("/test.js", TestContext.Current.CancellationToken);
        jsResponse.EnsureSuccessStatusCode();
        var jsonContent = await jsResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // assert
        indexContent.ShouldContain("Hello test world");
        jsonContent.ShouldContain("Placeholder javascript");
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/path/to/page")]
    [InlineData("/path?queryParams=true")]
    public async Task ShouldUseFallbackRoute_WhenIntegratedSpaIsEnabled(string route)
    {
        // arrange
        using var client = Factory
            .WithWebHostBuilder(builder => builder.UseSetting("EnableIntegratedSpa", "true"))
            .CreateClient();

        // act
        var response = await client.GetAsync(route, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // assert
        content.ShouldContain("Hello test world");
    }
}
