using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HackathonManager.Tests.TestInfrastructure;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests;

public class IntegratedSpaDisabledTests : IntegrationTestBase<IntegratedSpaDisabledTests.HackathonApp_SpaDisabled>
{
    /// <inheritdoc />
    public IntegratedSpaDisabledTests(HackathonApp_SpaDisabled hackathonApp)
        : base(hackathonApp) { }

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

    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public sealed class HackathonApp_SpaDisabled()
        : HackathonApp_MigratedDatabase([new KeyValuePair<string, string>(Constants.EnableIntegratedSpaKey, "false")]);
}
