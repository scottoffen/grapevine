using Grapevine.Abstractions.RouteConstraints;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class RegexResolverTests
{
    [Theory]
    [InlineData("abc",               @"(?<x>abc)")]
    [InlineData(@"\d+",              @"(?<x>\d+)")]
    [InlineData(@"[a-zA-Z0-9_-]+",  @"(?<x>[a-zA-Z0-9_-]+)")]
    [InlineData(@"[a-z]{3}\d{2}",   @"(?<x>[a-z]{3}\d{2})")]
    public void Resolve_ShouldReturnExpectedPattern_WhenArgsAreValid(string args, string expected)
    {
        RegexResolver.Resolve("x", args).ShouldBe(expected);
    }

    [Fact]
    public void Resolve_ShouldReturnSameInstance_OnSubsequentCallsWithSamePattern()
    {
        var first  = RegexResolver.Resolve("x", @"\d+");
        var second = RegexResolver.Resolve("y", @"\d+");
        first.ShouldBe(@"(?<x>\d+)");
        second.ShouldBe(@"(?<y>\d+)");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Resolve_ShouldThrowArgumentException_WhenArgsIsEmpty(string? args)
    {
        var ex = Should.Throw<ArgumentException>(() => RegexResolver.Resolve("x", args));
        ex.ParamName.ShouldBe("args");
        ex.Message.ShouldContain(RegexResolver.EmptyPatternMessage);
    }

    [Theory]
    [InlineData(@"^\d+$")]
    [InlineData("abc$")]
    [InlineData("^hello")]
    public void Resolve_ShouldThrowArgumentException_WhenPatternIsAnchored(string args)
    {
        var ex = Should.Throw<ArgumentException>(() => RegexResolver.Resolve("x", args));
        ex.ParamName.ShouldBe("args");
        ex.Message.ShouldContain(RegexResolver.AnchoredPatternMessage);
    }

    [Theory]
    [InlineData("abc(def)")]
    [InlineData("pre(?<group>abc)post")]
    [InlineData(@"([a-z]{2})(\d{2})")]
    public void Resolve_ShouldThrowArgumentException_WhenPatternContainsCaptureGroups(string args)
    {
        var ex = Should.Throw<ArgumentException>(() => RegexResolver.Resolve("x", args));
        ex.ParamName.ShouldBe("args");
        ex.Message.ShouldContain(RegexResolver.CaptureGroupsMessage);
    }

    [Theory]
    [InlineData("[a-")]
    [InlineData("*")]
    [InlineData("abc(")]
    public void Resolve_ShouldThrowArgumentException_WhenPatternIsInvalidRegex(string args)
    {
        var ex = Should.Throw<ArgumentException>(() => RegexResolver.Resolve("x", args));
        ex.ParamName.ShouldBe("args");
        ex.Message.ShouldContain(RegexResolver.InvalidPatternMessage);
    }
}