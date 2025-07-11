using System;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Xunit;

namespace HackathonManager.Tests.TestInfrastructure;

public abstract class UnitTest : IAsyncLifetime
{
    protected Faker Faker { get; } = new();

    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    /// <inheritdoc />
    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;
}
