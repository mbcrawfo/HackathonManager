using Bogus;
using HackathonManager.Utilities.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.UnitTests.Utilities.JsonPatch;

public class PatchRequestValidatorTests
{
    private readonly Faker _faker = new();

    public static TheoryData<Operation<object>[]> ValidOperationsSequences =>
        new()
        {
            {
                [
                    new Operation<object>("test", "/foo", from: null),
                    new Operation<object>("add", "/foo", from: null, value: 1),
                ]
            },
            {
                [new Operation<object>("test", "/foo", from: null), new Operation<object>("remove", "/foo", from: null)]
            },
            {
                [
                    new Operation<object>("test", "/foo", from: null),
                    new Operation<object>("test", "/bar", from: null),
                    new Operation<object>("move", "/foo", "/bar"),
                ]
            },
            {
                [
                    new Operation<object>("test", "/bar", from: null),
                    new Operation<object>("test", "/foo", from: null),
                    new Operation<object>("move", "/foo", "/bar"),
                ]
            },
            {
                [
                    new Operation<object>("test", "/foo", from: null),
                    new Operation<object>("replace", "/foo", from: null),
                ]
            },
            {
                [
                    new Operation<object>("test", "/foo", from: null),
                    new Operation<object>("remove", "/foo", from: null),
                    new Operation<object>("add", "/foo", from: null),
                ]
            },
        };

    [Fact]
    public void ShouldFailValidation_WhenInvalidOperations()
    {
        // arrange
        var request = new TestRequest
        {
            Operations = [new Operation<object>("foo", "/bar", from: null, _faker.Lorem.Word())],
        };
        var sut = new TestRequestValidator(requireTestBeforeMutate: false);

        // act
        var result = sut.Validate(request);

        // assert
        result.Errors.ShouldHaveSingleItem().ErrorCode.ShouldBe(ErrorCodes.JsonPatchInvalidOperation);
    }

    [Fact]
    public void ShouldFailValidation_WithOneError_WhenInvalidOperations_AndMutationsRequireTest()
    {
        // arrange
        var request = new TestRequest
        {
            Operations = [new Operation<object>("foo", "/bar", from: null, _faker.Lorem.Word())],
        };
        var sut = new TestRequestValidator(requireTestBeforeMutate: true);

        // act
        var result = sut.Validate(request);

        // assert
        result.Errors.ShouldHaveSingleItem().ErrorCode.ShouldBe(ErrorCodes.JsonPatchInvalidOperation);
    }

    [Fact]
    public void ShouldFailValidation_WhenMutationsRequireTest_AndOperationsStartsWithMutation()
    {
        // arrange
        var request = new TestRequest
        {
            Operations = [new Operation<object>("add", "/foo", from: null, _faker.Lorem.Word())],
        };
        var sut = new TestRequestValidator(requireTestBeforeMutate: true);

        // act
        var result = sut.Validate(request);

        // assert
        result.Errors.ShouldHaveSingleItem().ErrorCode.ShouldBe(ErrorCodes.JsonPatchInvalidSequence);
    }

    [Fact]
    public void ShouldFailValidation_WhenMutationsRequireTest_AndMoveDoesNotHaveTwoPreviousTests()
    {
        // arrange
        var request = new TestRequest
        {
            Operations =
            [
                new Operation<object>("test", "/foo", from: null, _faker.Lorem.Word()),
                new Operation<object>("move", "/foo", "/bar"),
            ],
        };
        var sut = new TestRequestValidator(requireTestBeforeMutate: true);

        // act
        var result = sut.Validate(request);

        // assert
        result.Errors.ShouldHaveSingleItem().ErrorCode.ShouldBe(ErrorCodes.JsonPatchInvalidSequence);
    }

    [Fact]
    public void ShouldFailValidation_WhenMutationsRequireTest_AndMutationDoesNotHaveTest()
    {
        // arrange
        var request = new TestRequest
        {
            Operations =
            [
                new Operation<object>("test", "/foo", from: null, _faker.Lorem.Word()),
                new Operation<object>("add", "/foo", from: null, _faker.Lorem.Word()),
                new Operation<object>("add", "/bar", from: null, _faker.Lorem.Word()),
            ],
        };
        var sut = new TestRequestValidator(requireTestBeforeMutate: true);

        // act
        var result = sut.Validate(request);

        // assert
        result.Errors.ShouldHaveSingleItem().ErrorCode.ShouldBe(ErrorCodes.JsonPatchInvalidSequence);
    }

    [Theory]
    [MemberData(nameof(ValidOperationsSequences))]
    public void ShouldNotFailValidation_WhenMutationsRequireTest_AndOperationsSequenceIsValid(
        Operation<object>[] operations
    )
    {
        // arrange
        var request = new TestRequest { Operations = operations };
        var sut = new TestRequestValidator(requireTestBeforeMutate: true);

        // act
        var result = sut.Validate(request);

        // assert
        result.Errors.ShouldBeEmpty();
    }

    private class TestRequest : PatchRequest<object>;

    private class TestRequestValidator : PatchRequestValidator<TestRequest, object>
    {
        /// <inheritdoc />
        public TestRequestValidator(bool requireTestBeforeMutate)
            : base(requireTestBeforeMutate) { }
    }
}
