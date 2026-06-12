using Grapevine.Abstractions.RouteConstraints;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class DecimalResolverTests
{
    [Theory]
    [InlineData(null, @"(?<amount>[-]?\d+(?:\.\d+)?)")]
    [InlineData("", @"(?<amount>[-]?\d+(?:\.\d+)?)")]
    [InlineData(" ", @"(?<amount>[-]?\d+(?:\.\d+)?)")]
    public void Resolve_ShouldReturnUnboundedDecimalPattern_WhenArgsIsNullOrWhitespace(string? args, string expected)
    {
        DecimalResolver.Resolve("amount", args).ShouldBe(expected);
    }

    [Theory]
    [InlineData("0",   @"(?<amount>[-]?\d+(?:\.\d{0})?)")]
    [InlineData("2",   @"(?<amount>[-]?\d+(?:\.\d{2})?)")]
    [InlineData("1,",  @"(?<amount>[-]?\d+(?:\.\d{1,})?)")]
    [InlineData(",3",  @"(?<amount>[-]?\d+(?:\.\d{0,3})?)")]
    [InlineData("1,3", @"(?<amount>[-]?\d+(?:\.\d{1,3})?)")]
    [InlineData("0,",  @"(?<amount>[-]?\d+(?:\.\d{0,})?)")]
    [InlineData("0,3", @"(?<amount>[-]?\d+(?:\.\d{0,3})?)")]
    public void Resolve_ShouldReturnBoundedDecimalPattern_WhenArgsAreValid(string args, string expected)
    {
        DecimalResolver.Resolve("amount", args).ShouldBe(expected);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData(",,")]
    [InlineData("1,2,3")]
    [InlineData("-1")]
    public void Resolve_ShouldThrowArgumentException_WhenArgsAreInvalid(string args)
    {
        var ex = Should.Throw<ArgumentException>(() => DecimalResolver.Resolve("amount", args));
        ex.Message.ShouldContain("Invalid argument");
    }
}