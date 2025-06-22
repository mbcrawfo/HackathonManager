using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HackathonManager.Tests.TestInfrastructure;

[Collection("IntegrationTests")]
public abstract class IntegrationTestsBase : IAsyncLifetime
{
    private readonly IntegrationTestsFixture _fixture;

    protected IntegrationTestsBase(IntegrationTestsFixture fixture)
    {
        _fixture = fixture;
    }

    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    protected DbDataSource DataSource => _fixture.DataSource;

    protected HackathonManagerWebApplicationFactory Factory => _fixture.Factory;

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask InitializeAsync() => await _fixture.ResetData();
}

[CollectionDefinition("IntegrationTests")]
public sealed class IntegrationTestsCollection : ICollectionFixture<IntegrationTestsFixture>;
