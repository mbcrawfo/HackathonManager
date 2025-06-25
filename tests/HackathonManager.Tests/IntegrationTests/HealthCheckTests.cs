using System.Threading.Tasks;
using HackathonManager.Tests.TestInfrastructure;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests;

public class HealthCheckTests : IntegrationTestBase<HackathonApp>
{
    /// <inheritdoc />
    public HealthCheckTests(HackathonApp hackathonApp)
        : base(hackathonApp) { }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // arrange
        // act
        var response = await App.Client.GetStringAsync("/health", Cancellation);

        // assert
        response.ShouldBe("Healthy");
    }
}
