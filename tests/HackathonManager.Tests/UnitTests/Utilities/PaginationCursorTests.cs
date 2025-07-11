using HackathonManager.Tests.TestInfrastructure;
using HackathonManager.Utilities;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.UnitTests.Utilities;

public class PaginationCursorTests : UnitTest
{
    [Fact]
    public void ShouldSerializeAndDeserialize()
    {
        // arrange
        var cursor = new TestCursor(Faker.Lorem.Word(), Faker.Lorem.Word());

        // act
        var encoded = PaginationCursor.Encode(cursor);
        var decoded = PaginationCursor.Decode<TestCursor>(encoded);

        // assert
        decoded.ShouldBe(cursor);
    }

    [Fact]
    public void ShouldSerializeAndDeserialize_WhenInputIsLarge()
    {
        // arrange
        var cursor = new TestCursor(Faker.Lorem.Paragraphs(20), Faker.Lorem.Paragraphs(20));

        // act
        var encoded = PaginationCursor.Encode(cursor);
        var decoded = PaginationCursor.Decode<TestCursor>(encoded);

        // assert
        decoded.ShouldBe(cursor);
    }

    private record TestCursor(string Name, string Value);
}
