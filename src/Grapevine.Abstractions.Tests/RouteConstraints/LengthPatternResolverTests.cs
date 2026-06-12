using Grapevine.Abstractions.RouteConstraints;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class LengthPatternResolverTests
{
    [Theory]
    [InlineData(null, "+")]
    [InlineData("", "+")]
    [InlineData(" ", "+")]
    public void Resolve_WithNullOrEmptyOrWhitespace_ShouldReturnPlus(string? input, string expected)
    {
        LengthPatternResolver.Resolve(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("1", "{1}")]
    [InlineData("3", "{3}")]
    [InlineData("5", "{5}")]
    public void Resolve_WithExactCount_ShouldReturnExactQuantifier(string input, string expected)
    {
        LengthPatternResolver.Resolve(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("1,",  "{1,}")]
    [InlineData("4,",  "{4,}")]
    public void Resolve_WithLowerBoundOnly_ShouldReturnMinOnlyQuantifier(string input, string expected)
    {
        LengthPatternResolver.Resolve(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData(",3", "{1,3}")]
    [InlineData(",5", "{1,5}")]
    public void Resolve_WithUpperBoundOnly_ShouldApplyMinOfOne(string input, string expected)
    {
        LengthPatternResolver.Resolve(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("1,3", "{1,3}")]
    [InlineData("2,5", "{2,5}")]
    public void Resolve_WithMinAndMax_ShouldReturnBoundedQuantifier(string input, string expected)
    {
        LengthPatternResolver.Resolve(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData(" 3 ",   "{3}")]
    [InlineData(" 1 , 4 ", "{1,4}")]
    [InlineData(" , 5 ", "{1,5}")]
    public void Resolve_WithWhitespaceInInput_ShouldReturnTrimmedQuantifier(string input, string expected)
    {
        LengthPatternResolver.Resolve(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1,,5")]
    [InlineData(",,")]
    [InlineData("-1")]
    [InlineData("2,-5")]
    [InlineData("-1,5")]
    [InlineData("5,2")]
    [InlineData("0")]
    [InlineData("0,2")]
    [InlineData("0,")]
    public void Resolve_WithInvalidInput_ShouldThrowArgumentException(string input)
    {
        Should.Throw<ArgumentException>(() => LengthPatternResolver.Resolve(input));
    }
}