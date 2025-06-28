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

public class Base32FunctionTests : DatabaseTestBase
{
    /// <inheritdoc />
    public Base32FunctionTests(ResettingDatabaseFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task Base32Decode_ShouldRaiseException_WhenInputIsWrongLength()
    {
        // arrange
        var text = Fake.Random.AlphaNumeric(30);

        // act
        await using var connection = await DataSource.OpenConnectionAsync(Cancellation);
        async Task Act() =>
            // ReSharper disable once AccessToDisposedClosure
            await connection.QuerySingleAsync<Guid>(
                new CommandDefinition(
                    "select base32_decode(@Text);",
                    new { Text = text },
                    cancellationToken: Cancellation
                )
            );

        // assert
        Act().ShouldThrow<NpgsqlException>().Message.ShouldContain("must be 26 characters");
    }

    [Fact]
    public async Task Base32Decode_ShouldRaiseException_WhenInputContainsInvalidCharacters()
    {
        // arrange
        var typeId = TypeId.New("test");
        var invalidBase32 = typeId.GetSuffix()[..^1] + "L";

        // act
        await using var connection = await DataSource.OpenConnectionAsync(Cancellation);
        async Task Act() =>
            // ReSharper disable once AccessToDisposedClosure
            await connection.QuerySingleAsync<Guid>(
                new CommandDefinition(
                    "select base32_decode(@Text);",
                    new { Text = invalidBase32 },
                    cancellationToken: Cancellation
                )
            );

        // assert
        Act().ShouldThrow<NpgsqlException>().Message.ShouldContain("contain characters in the base32 alphabet");
    }

    [Fact]
    public async Task Base32Decode_ShouldRaiseException_WhenInputFirstCharacterIsNotValid()
    {
        // arrange
        var typeId = TypeId.New("test");
        var invalidBase32 = "8" + typeId.GetSuffix()[1..];

        // act
        await using var connection = await DataSource.OpenConnectionAsync(Cancellation);
        async Task Act() =>
            // ReSharper disable once AccessToDisposedClosure
            await connection.QuerySingleAsync<Guid>(
                new CommandDefinition(
                    "select base32_decode(@Text);",
                    new { Text = invalidBase32 },
                    cancellationToken: Cancellation
                )
            );

        // assert
        Act().ShouldThrow<NpgsqlException>().Message.ShouldContain("must start with 0-7");
    }

    [Fact]
    public async Task Base32Decode_ShouldConvertTextToUuid()
    {
        // arrange
        var typeId = TypeId.New("test");
        var expected = typeId.Id;

        // act
        await using var connection = await DataSource.OpenConnectionAsync(Cancellation);
        var actual = await connection.QuerySingleAsync<Guid>(
            new CommandDefinition(
                "select base32_decode(@Text);",
                new { Text = typeId.GetSuffix() },
                cancellationToken: Cancellation
            )
        );

        // assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public async Task Base32Encode_ShouldConvertUuidToText()
    {
        // arrange
        var typeId = TypeId.New("test");
        var expected = typeId.GetSuffix();

        // act
        await using var connection = await DataSource.OpenConnectionAsync(Cancellation);
        var actual = await connection.QuerySingleAsync<string>(
            new CommandDefinition(
                "select base32_encode(@Uuid);",
                new { Uuid = typeId.Id },
                cancellationToken: Cancellation
            )
        );

        // assert
        actual.ShouldBe(expected);
    }
}
