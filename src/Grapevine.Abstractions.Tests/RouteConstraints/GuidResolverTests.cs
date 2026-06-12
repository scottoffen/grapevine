using Grapevine.Abstractions.RouteConstraints;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class GuidResolverTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Resolve_ShouldReturnExpectedPattern_WhenArgsIsNullOrWhitespace(string? args)
    {
        var expected = @"(?<id>[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})";
        GuidResolver.Resolve("id", args).ShouldBe(expected);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("guid")]
    public void Resolve_ShouldThrowArgumentException_WhenArgsAreProvided(string args)
    {
        var ex = Should.Throw<ArgumentException>(() => GuidResolver.Resolve("id", args));
        ex.ParamName.ShouldBe("args");
        ex.Message.ShouldContain(GuidResolver.NoArgumentsMessage);
    }
}