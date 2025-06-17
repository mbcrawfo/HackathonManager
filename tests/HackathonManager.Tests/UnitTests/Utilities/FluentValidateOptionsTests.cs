using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using HackathonManager.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.UnitTests.Utilities;

public class FluentValidateOptionsTests
{
    [Fact]
    public void ShouldSkipValidation_WhenNameDoesNotMatch()
    {
        // arrange
        var provider = new ServiceCollection().BuildServiceProvider();
        var sut = new FluentValidateOptions<object>(provider, "foo");

        // act
        var result = sut.Validate("bar", new object());

        // assert
        result.ShouldBe(ValidateOptionsResult.Skip);
    }

    [Fact]
    public void ShouldReturnValidateErrors_WhenValidationFails()
    {
        // arrange
        var provider = new ServiceCollection()
            .AddTransient<IValidator<TestOptions>, TestValidator>()
            .BuildServiceProvider();
        var sut = new FluentValidateOptions<TestOptions>(provider, null);

        // act
        var result = sut.Validate(null, new TestOptions { Name = null });

        // assert
        result.Failures.ShouldNotBeNull().ShouldContain(x => x.Contains("not be empty"));
    }

    [Fact]
    public void ShouldReturnSuccess_WhenValidationPasses()
    {
        // arrange
        var provider = new ServiceCollection()
            .AddTransient<IValidator<TestOptions>, TestValidator>()
            .BuildServiceProvider();
        var sut = new FluentValidateOptions<TestOptions>(provider, null);

        // act
        var result = sut.Validate(null, new TestOptions { Name = "Valid Name" });

        // assert
        result.ShouldBe(ValidateOptionsResult.Success);
    }

    private class TestOptions
    {
        public string? Name { get; init; }
    }

    private class TestValidator : AbstractValidator<TestOptions>
    {
        [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed")]
        public TestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
