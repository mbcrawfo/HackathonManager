using System.Threading.Tasks;
using HackathonManager.Tests.TestInfrastructure;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests;

public class HealthCheckTests : IntegrationTestsBase
{
    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // arrange
        using var client = Factory.CreateClient();

        // act
        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // assert
        content.ShouldBe("Healthy");
    }
}
