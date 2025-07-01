using Bogus;
using FastEndpoints;
using HackathonManager.Utilities.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.UnitTests.Utilities.JsonPatch;

public class PatchRequestTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void ApplyOperationsTo_ShouldApplyOperationsToModel()
    {
        // arrange
        Factory.RegisterTestServices(_ => { });

        var expectedName = _faker.Lorem.Word();
        var model = new TestModel();
        var sut = new TestRequest { Operations = [new Operation<TestModel>("add", "/name", null, expectedName)] };

        // act
        sut.ApplyOperationsTo(model);

        // assert
        model.Name.ShouldBe(expectedName);
    }

    [Fact]
    public void ApplyOperationsTo_ShouldThrowValidationFailureException_WhenTestOperationFails()
    {
        // arrange
        Factory.RegisterTestServices(_ => { });

        var model = new TestModel();
        var sut = new TestRequest
        {
            Operations = [new Operation<TestModel>("test", "/name", null, _faker.Lorem.Word())],
        };

        // act
        var act = () => sut.ApplyOperationsTo(model);

        // assert
        act.ShouldThrow<ValidationFailureException>()
            .Failures.ShouldHaveSingleItem()
            .ErrorCode.ShouldBe(ErrorCodes.JsonPatchTestFailed);
    }

    [Fact]
    public void ApplyOperationsTo_ShouldThrowValidationFailureException_WhenOtherOperationsFail()
    {
        // arrange
        Factory.RegisterTestServices(_ => { });

        var model = new TestModel();
        var sut = new TestRequest
        {
            Operations = [new Operation<TestModel>("add", "/badProperty", null, _faker.Lorem.Word())],
        };

        // act
        var act = () => sut.ApplyOperationsTo(model);

        // assert
        act.ShouldThrow<ValidationFailureException>()
            .Failures.ShouldHaveSingleItem()
            .ErrorCode.ShouldBe(ErrorCodes.JsonPatchFailed);
    }

    private class TestModel
    {
        public string? Name { get; set; }
    }

    private class TestRequest : PatchRequest<TestModel>;
}
