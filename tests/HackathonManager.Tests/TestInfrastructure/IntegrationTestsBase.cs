using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HackathonManager.Tests.TestInfrastructure;

public abstract class IntegrationTestsBase : IAsyncLifetime
{
    protected HackathonManagerWebApplicationFactory Factory { get; } = new();

    public static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await Factory.DisposeAsync();

    /// <inheritdoc />
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;
}
