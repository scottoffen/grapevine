using Grapevine.Abstractions.RouteConstraints;
using Shouldly;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class DefaultResolverTests
{
    public class Resolve
    {
        [Fact]
        public void ReturnsUnboundPattern_WhenArgsIsNull()
        {
            var (pattern, _, _) = DefaultResolver.Resolve("val", null);
            pattern.ShouldBe("(?<val>[^/]+)");
        }

        [Fact]
        public void ReturnsUnboundPattern_WhenArgsIsEmpty()
        {
            var (pattern, _, _) = DefaultResolver.Resolve("val", string.Empty);
            pattern.ShouldBe("(?<val>[^/]+)");
        }

        [Fact]
        public void ReturnsExactLengthPattern_WhenArgIsExact()
        {
            var (pattern, _, _) = DefaultResolver.Resolve("val", "3");
            pattern.ShouldBe("(?<val>[^/]{3})");
        }

        [Fact]
        public void ReturnsMinLengthPattern_WhenArgIsMinOnly()
        {
            var (pattern, _, _) = DefaultResolver.Resolve("val", "1,");
            pattern.ShouldBe("(?<val>[^/]{1,})");
        }

        [Fact]
        public void ReturnsRangePattern_WhenArgIsRange()
        {
            var (pattern, _, _) = DefaultResolver.Resolve("val", "1,5");
            pattern.ShouldBe("(?<val>[^/]{1,5})");
        }

        [Fact]
        public void ReturnsCorrectStrictness()
        {
            var (_, strictness, _) = DefaultResolver.Resolve("val", null);
            strictness.ShouldBe(DefaultResolver.Strictness);
        }

        [Fact]
        public void ReturnsCorrectGroup()
        {
            var (_, _, group) = DefaultResolver.Resolve("val", null);
            group.ShouldBe(DefaultResolver.Group);
        }

        [Fact]
        public void Throws_WhenArgsIsInvalid()
        {
            Should.Throw<ArgumentException>(() => DefaultResolver.Resolve("val", "0"));
        }
    }
}