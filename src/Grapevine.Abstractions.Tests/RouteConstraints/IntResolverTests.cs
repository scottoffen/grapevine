using Grapevine.Abstractions.RouteConstraints;
using Shouldly;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class IntResolverTests
{
    public class Resolve
    {
        [Fact]
        public void ReturnsUnboundPattern_WhenArgsIsNull()
        {
            var (pattern, _, _) = IntResolver.Resolve("n", null);
            pattern.ShouldBe(@"(?<n>-?\d+)");
        }

        [Fact]
        public void ReturnsUnboundPattern_WhenArgsIsEmpty()
        {
            var (pattern, _, _) = IntResolver.Resolve("n", string.Empty);
            pattern.ShouldBe(@"(?<n>-?\d+)");
        }

        [Fact]
        public void ReturnsExactLengthPattern_WhenArgIsExact()
        {
            var (pattern, _, _) = IntResolver.Resolve("n", "3");
            pattern.ShouldBe(@"(?<n>-?\d{3})");
        }

        [Fact]
        public void ReturnsMinLengthPattern_WhenArgIsMinOnly()
        {
            var (pattern, _, _) = IntResolver.Resolve("n", "1,");
            pattern.ShouldBe(@"(?<n>-?\d{1,})");
        }

        [Fact]
        public void ReturnsRangePattern_WhenArgIsRange()
        {
            var (pattern, _, _) = IntResolver.Resolve("n", "1,5");
            pattern.ShouldBe(@"(?<n>-?\d{1,5})");
        }

        [Fact]
        public void ReturnsCorrectStrictness()
        {
            var (_, strictness, _) = IntResolver.Resolve("n", null);
            strictness.ShouldBe(IntResolver.Strictness);
        }

        [Fact]
        public void ReturnsCorrectGroup()
        {
            var (_, _, group) = IntResolver.Resolve("n", null);
            group.ShouldBe(IntResolver.Group);
        }

        [Fact]
        public void Throws_WhenArgsIsInvalid()
        {
            Should.Throw<ArgumentException>(() => IntResolver.Resolve("n", "0"));
        }
    }
}