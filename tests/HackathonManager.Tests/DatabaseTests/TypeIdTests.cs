using System;
using System.Threading.Tasks;
using Dapper;
using FastIDs.TypeId;
using HackathonManager.Tests.TestInfrastructure;
using HackathonManager.Tests.TestInfrastructure.Database;
using Npgsql;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.DatabaseTests;

public class TypeIdTests : DatabaseTestBase
{
    /// <inheritdoc />
    public TypeIdTests(ResettingDatabaseFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task TypeIdEncode_ShouldReturnEncodedInputs()
    {
        // arrange
        var typeId = TypeId.New("test");
        var expected = typeId.ToString();

        // act
        await using var connection = await DataSource.OpenConnectionAsync(Cancellation);
        var actual = await connection.QuerySingleAsync<string>(
            new CommandDefinition(
                "select typeid_encode(@Prefix, @Id);",
                new { Prefix = typeId.Type, typeId.Id },
                cancellationToken: Cancellation
            )
        );

        // assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public async Task UuidFromTypeId_ShouldRaiseException_WhenInputHasInvalidFormat()
    {
        // arrange
        var typeId = TypeId.New("test");

        // act
        await using var connection = await DataSource.OpenConnectionAsync(Cancellation);

        async Task Act() =>
            // ReSharper disable once AccessToDisposedClosure
            await connection.QuerySingleAsync<Guid>(
                new CommandDefinition(
                    "select uuid_from_typeid(@Text);",
                    new { Text = typeId.GetSuffix() },
                    cancellationToken: Cancellation
                )
            );

        // assert
        Act().ShouldThrow<NpgsqlException>().Message.ShouldContain("invalid typeid format");
    }

    [Fact]
    public async Task UuidFromTypeId_ReturnDecodedUuid()
    {
        // arrange
        var typeId = TypeId.New("test");
        var expected = typeId.Id;

        // act
        await using var connection = await DataSource.OpenConnectionAsync(Cancellation);
        var actual = await connection.QuerySingleAsync<Guid>(
            new CommandDefinition(
                "select uuid_from_typeid(@Text);",
                new { Text = typeId.Encode().ToString() },
                cancellationToken: Cancellation
            )
        );

        // assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public async Task UuidTypeIdEqualityOperator_ShouldReturnFalse_WhenOperandsAreNotEqual()
    {
        // arrange
        var uuid = Guid.CreateVersion7();
        var typeId = TypeId.New("test");

        // act
        await using var connection = await DataSource.OpenConnectionAsync(Cancellation);
        var result = await connection.QuerySingleAsync<bool>(
            new CommandDefinition(
                "select @Uuid === @TypeId;",
                new { Uuid = uuid, TypeId = typeId.Encode().ToString() },
                cancellationToken: Cancellation
            )
        );

        // assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task UuidTypeIdEqualityOperator_ShouldReturnTrue_WhenOperandsAreEqual()
    {
        // arrange
        var typeId = TypeId.New("test");

        // act
        await using var connection = await DataSource.OpenConnectionAsync(Cancellation);
        var result = await connection.QuerySingleAsync<bool>(
            new CommandDefinition(
                "select @Uuid === @TypeId;",
                new { Uuid = typeId.Id, TypeId = typeId.Encode().ToString() },
                cancellationToken: Cancellation
            )
        );

        // assert
        result.ShouldBeTrue();
    }
}
