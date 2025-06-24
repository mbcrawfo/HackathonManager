using System.Threading.Tasks;
using FastEndpoints.Testing;
using HackathonManager.Tests.TestInfrastructure;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests;

public class HealthCheckTests : TestBase<AppWithDatabase>
{
    private readonly AppWithDatabase _app;

    public HealthCheckTests(AppWithDatabase app)
    {
        _app = app;
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // arrange
        // act
        var response = await _app.Client.GetStringAsync("/health", Cancellation);

        // assert
        response.ShouldBe("Healthy");
    }
}
