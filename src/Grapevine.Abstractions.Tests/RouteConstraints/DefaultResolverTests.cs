using Grapevine.Abstractions.RouteConstraints;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class DefaultResolverTests
{
    [Theory]
    [InlineData(null, @"(?<segment>[^/]+)")]
    [InlineData("", @"(?<segment>[^/]+)")]
    [InlineData(" ", @"(?<segment>[^/]+)")]
    public void Resolve_ShouldReturnUnboundedPattern_WhenArgsIsNullOrWhitespace(string? args, string expected)
    {
        DefaultResolver.Resolve("segment", args).ShouldBe(expected);
    }

    [Theory]
    [InlineData("3",   @"(?<segment>[^/]{3})")]
    [InlineData("1,",  @"(?<segment>[^/]{1,})")]
    [InlineData(",5",  @"(?<segment>[^/]{1,5})")]
    [InlineData("1,5", @"(?<segment>[^/]{1,5})")]
    public void Resolve_ShouldReturnBoundedPattern_WhenArgsAreValid(string args, string expected)
    {
        DefaultResolver.Resolve("segment", args).ShouldBe(expected);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("1.2")]
    [InlineData(",")]
    [InlineData("1,2,3")]
    [InlineData("0")]
    public void Resolve_ShouldThrowArgumentException_WhenArgsAreInvalid(string args)
    {
        var ex = Should.Throw<ArgumentException>(() => DefaultResolver.Resolve("segment", args));
        ex.Message.ShouldContain("Invalid argument");
    }
}