using Grapevine.Abstractions.RouteConstraints;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class BoolResolverTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Resolve_ShouldReturnExpectedPattern_WhenArgsAreNullOrWhitespace(string? args)
    {
        BoolResolver.Resolve("flag", args).ShouldBe("(?<flag>true|false)");
    }

    [Theory]
    [InlineData("true")]
    [InlineData("1")]
    [InlineData("any")]
    public void Resolve_ShouldThrowArgumentException_WhenArgsAreProvided(string args)
    {
        var ex = Should.Throw<ArgumentException>(() => BoolResolver.Resolve("flag", args));
        ex.ParamName.ShouldBe("args");
        ex.Message.ShouldContain(BoolResolver.NoArgumentsMessage);
    }
}