using System.Net;
using System.Threading.Tasks;
using HackathonManager.Tests.TestInfrastructure;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests;

public class IntegratedSpaDisabledTests : IntegrationTestBase
{
    /// <inheritdoc />
    public IntegratedSpaDisabledTests(AppWithDatabaseReset app)
        : base(app) { }

    [Theory]
    [InlineData("/")]
    [InlineData("/index.html")]
    [InlineData("/test.js")]
    public async Task ShouldNotServeStaticFiles_WhenIntegratedSpaIsDisabled(string url)
    {
        // arrange
        // act
        var response = await App.Client.GetAsync(url, Cancellation);

        // assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
