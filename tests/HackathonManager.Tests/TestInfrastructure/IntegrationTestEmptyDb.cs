using HackathonManager.Tests.TestInfrastructure.Database;
using JetBrains.Annotations;
using Xunit;
using Xunit.Sdk;

namespace HackathonManager.Tests.TestInfrastructure;

/// <summary>
///     Base for tests of the web app that require an empty database that does not reset.
/// </summary>
[Collection(nameof(IntegrationTestEmptyDbCollection))]
public abstract class IntegrationTestEmptyDb : IntegrationTestBase<DatabaseFixture>
{
    /// <inheritdoc />
    protected IntegrationTestEmptyDb(IntegrationTestEmptyDbFixture fixture)
        : base(fixture) { }
}

[CollectionDefinition(nameof(IntegrationTestEmptyDbCollection))]
public sealed class IntegrationTestEmptyDbCollection : ICollectionFixture<IntegrationTestEmptyDbFixture>;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class IntegrationTestEmptyDbFixture(IMessageSink _messageSink)
    : IntegrationTestFixture<DatabaseFixture>(_messageSink);
