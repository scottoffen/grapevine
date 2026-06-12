using Grapevine.Abstractions.RouteConstraints;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class IntResolverTests
{
    [Theory]
    [InlineData(null, @"(?<num>-?\d+)")]
    [InlineData("", @"(?<num>-?\d+)")]
    [InlineData(" ", @"(?<num>-?\d+)")]
    public void Resolve_ShouldReturnUnboundedPattern_WhenArgsIsNullOrWhitespace(string? args, string expected)
    {
        IntResolver.Resolve("num", args).ShouldBe(expected);
    }

    [Theory]
    [InlineData("3",   @"(?<num>-?\d{3})")]
    [InlineData("1,",  @"(?<num>-?\d{1,})")]
    [InlineData(",5",  @"(?<num>-?\d{1,5})")]
    [InlineData("1,5", @"(?<num>-?\d{1,5})")]
    public void Resolve_ShouldReturnBoundedPattern_WhenArgsAreValid(string args, string expected)
    {
        IntResolver.Resolve("num", args).ShouldBe(expected);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("1.2")]
    [InlineData(",")]
    [InlineData("1,2,3")]
    [InlineData("0")]
    public void Resolve_ShouldThrowArgumentException_WhenArgsAreInvalid(string args)
    {
        var ex = Should.Throw<ArgumentException>(() => IntResolver.Resolve("num", args));
        ex.Message.ShouldContain("Invalid argument");
    }
}