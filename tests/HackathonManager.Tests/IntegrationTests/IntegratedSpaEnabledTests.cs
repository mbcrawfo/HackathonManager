using System.Collections.Generic;
using System.Threading.Tasks;
using HackathonManager.Tests.TestInfrastructure;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests;

public class IntegratedSpaEnabledTests : IntegrationTestBase<IntegratedSpaEnabledTests.HackathonApp_SpaEnabled>
{
    /// <inheritdoc />
    public IntegratedSpaEnabledTests(HackathonApp_SpaEnabled hackathonApp)
        : base(hackathonApp) { }

    [Fact]
    public async Task ShouldServeStaticFiles_WhenIntegratedSpaIsEnabled()
    {
        // arrange
        // act
        var indexResponse = await App.Client.GetStringAsync("/index.html", Cancellation);
        var jsResponse = await App.Client.GetStringAsync("/test.js", Cancellation);

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
        var response = await App.Client.GetStringAsync(route, Cancellation);

        // assert
        response.ShouldContain("Hello test world");
    }

    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public sealed class HackathonApp_SpaEnabled()
        : HackathonApp_MigratedDatabase([new KeyValuePair<string, string>(Constants.EnableIntegratedSpaKey, "true")]);
}
