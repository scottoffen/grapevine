using Grapevine.Abstractions.RouteConstraints;
using Shouldly;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class AlphaResolverTests
{
    public class Resolve
    {
        [Fact]
        public void ReturnsUnboundPattern_WhenArgsIsNull()
        {
            var (pattern, _, _) = AlphaResolver.Resolve("name", null);
            pattern.ShouldBe("(?<name>[a-zA-Z]+)");
        }

        [Fact]
        public void ReturnsUnboundPattern_WhenArgsIsEmpty()
        {
            var (pattern, _, _) = AlphaResolver.Resolve("name", string.Empty);
            pattern.ShouldBe("(?<name>[a-zA-Z]+)");
        }

        [Fact]
        public void ReturnsExactLengthPattern_WhenArgIsExact()
        {
            var (pattern, _, _) = AlphaResolver.Resolve("name", "3");
            pattern.ShouldBe("(?<name>[a-zA-Z]{3})");
        }

        [Fact]
        public void ReturnsMinLengthPattern_WhenArgIsMinOnly()
        {
            var (pattern, _, _) = AlphaResolver.Resolve("name", "1,");
            pattern.ShouldBe("(?<name>[a-zA-Z]{1,})");
        }

        [Fact]
        public void ReturnsMaxLengthPattern_WhenArgIsMaxOnly()
        {
            var (pattern, _, _) = AlphaResolver.Resolve("name", ",5");
            pattern.ShouldBe("(?<name>[a-zA-Z]{1,5})");
        }

        [Fact]
        public void ReturnsRangePattern_WhenArgIsRange()
        {
            var (pattern, _, _) = AlphaResolver.Resolve("name", "1,5");
            pattern.ShouldBe("(?<name>[a-zA-Z]{1,5})");
        }

        [Fact]
        public void ReturnsCorrectStrictness()
        {
            var (_, strictness, _) = AlphaResolver.Resolve("name", null);
            strictness.ShouldBe(AlphaResolver.Strictness);
        }

        [Fact]
        public void ReturnsCorrectGroup()
        {
            var (_, _, group) = AlphaResolver.Resolve("name", null);
            group.ShouldBe(AlphaResolver.Group);
        }

        [Fact]
        public void Throws_WhenArgsIsInvalid()
        {
            Should.Throw<ArgumentException>(() => AlphaResolver.Resolve("name", "0"));
        }
    }
}