using Grapevine.Abstractions.RouteConstraints;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class BoolResolverTests
{
    public class Resolve
    {
        [Fact]
        public void ReturnsPattern_WhenArgsIsNull()
        {
            var (pattern, _, _) = BoolResolver.Resolve("flag", null);
            pattern.ShouldBe("(?<flag>true|false)");
        }

        [Fact]
        public void ReturnsPattern_WhenArgsIsEmpty()
        {
            var (pattern, _, _) = BoolResolver.Resolve("flag", string.Empty);
            pattern.ShouldBe("(?<flag>true|false)");
        }

        [Fact]
        public void ReturnsCorrectStrictness()
        {
            var (_, strictness, _) = BoolResolver.Resolve("flag", null);
            strictness.ShouldBe(BoolResolver.Strictness);
        }

        [Fact]
        public void ReturnsCorrectGroup()
        {
            var (_, _, group) = BoolResolver.Resolve("flag", null);
            group.ShouldBe(BoolResolver.Group);
        }

        [Fact]
        public void Throws_WhenArgsIsNonEmpty()
        {
            var ex = Should.Throw<ArgumentException>(() => BoolResolver.Resolve("flag", "true"));
            ex.Message.ShouldContain(BoolResolver.NoArgumentsMessage);
        }
    }
}