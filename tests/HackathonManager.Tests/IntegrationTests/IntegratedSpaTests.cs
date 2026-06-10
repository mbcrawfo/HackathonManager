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
    public async Task ShouldServeStaticFiles()
    {
        // arrange
        var client = App.CreateClient();

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
    public async Task ShouldUseFallbackRoute(string route)
    {
        // arrange
        var client = App.CreateClient();

        // act
        var response = await client.GetStringAsync(route, CancellationToken);

        // assert
        response.ShouldContain("Hello test world");
    }
}
