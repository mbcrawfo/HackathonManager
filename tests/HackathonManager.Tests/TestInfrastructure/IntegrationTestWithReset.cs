using HackathonManager.Tests.TestInfrastructure.Database;
using JetBrains.Annotations;
using Xunit;
using Xunit.Sdk;

namespace HackathonManager.Tests.TestInfrastructure;

/// <summary>
///     Base for tests of the web app that require a migrated web app that resets between tests.
/// </summary>
[Collection(nameof(IntegrationTestWithResetCollection))]
public abstract class IntegrationTestWithReset : IntegrationTestBase<DatabaseResetFixture>
{
    /// <inheritdoc />
    protected IntegrationTestWithReset(IntegrationTestWithResetFixture fixture)
        : base(fixture) { }
}

[CollectionDefinition(nameof(IntegrationTestWithResetCollection))]
public sealed class IntegrationTestWithResetCollection : ICollectionFixture<IntegrationTestWithResetFixture>;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class IntegrationTestWithResetFixture(IMessageSink _messageSink)
    : IntegrationTestFixture<DatabaseResetFixture>(_messageSink);
