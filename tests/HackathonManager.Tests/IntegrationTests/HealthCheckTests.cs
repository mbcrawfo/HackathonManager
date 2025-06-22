using System.Threading.Tasks;
using HackathonManager.Tests.TestInfrastructure;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests;

public class HealthCheckTests : IntegrationTestsBase
{
    /// <inheritdoc />
    public HealthCheckTests(IntegrationTestsFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // arrange
        using var client = Factory.CreateClient();

        // act
        var response = await client.GetStringAsync("/health", CancellationToken);

        // assert
        response.ShouldBe("Healthy");
    }
}
