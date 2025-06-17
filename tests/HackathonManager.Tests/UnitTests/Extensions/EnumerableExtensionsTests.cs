using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HackathonManager.Extensions;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.UnitTests.Extensions;

public class EnumerableExtensionsTests
{
    [Fact]
    public void ToReadOnlyCollection_ShouldReturnSameCollection_WhenEnumerableIsReadOnlyCollection()
    {
        // arrange
        var expected = new ReadOnlyCollection<int>([1, 2, 3]);

        // act
        var actual = expected.ToReadOnlyCollection();

        // assert
        actual.ShouldBeSameAs(expected);
    }

    [Fact]
    public void ToReadOnlyCollection_ShouldReturnEmptyArray_WhenEnumerableIsEmptyCollection()
    {
        // arrange
        var sut = Array.Empty<int>();

        // act
        var actual = sut.ToReadOnlyCollection();

        // assert
        actual.ShouldBeOfType<int[]>().ShouldBeEmpty();
    }

    [Fact]
    public void ToReadOnlyCollection_ShouldWrapListInReadOnlyCollection_WhenEnumerableIsList()
    {
        // arrange
        var sut = new List<int> { 1, 2, 3 };

        // act
        var result = sut.ToReadOnlyCollection();

        // assert
        result.ShouldBeOfType<ReadOnlyCollection<int>>();
        result.ShouldBe(sut);
    }

    [Fact]
    public void ToReadOnlyCollection_ShouldEnumerateItemsAndReturnArray_WhenEnumerableIsOtherType()
    {
        // arrange
        var sut = new[] { 1, 2 }.Append(element: 3);

        // act
        var result = sut.ToReadOnlyCollection();

        // assert
        result.ShouldBeOfType<int[]>().ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void WhereNotNull_ShouldRemoveNullItems_WhenTypeIsClass()
    {
        // arrange
        var sut = new[] { "a", null, "b", null, "c" };

        // act
        var result = sut.WhereNotNull();

        // assert
        result.ShouldBe(["a", "b", "c"]);
    }

    [Fact]
    public void WhereNotNull_ShouldRemoveNullItems_WhenTypeIsStruct()
    {
        // arrange
        var sut = new int?[] { 1, null, 3, null, 5 };

        // act
        var result = sut.WhereNotNull();

        // assert
        result.ShouldBe([1, 3, 5]);
    }
}
