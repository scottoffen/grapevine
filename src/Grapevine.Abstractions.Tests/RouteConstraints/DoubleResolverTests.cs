using Grapevine.Abstractions.RouteConstraints;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class DoubleResolverTests
{
    public class Resolve
    {
        [Fact]
        public void ReturnsUnboundPattern_WhenArgsIsNull()
        {
            var (pattern, _, _) = DoubleResolver.Resolve("n", null);
            pattern.ShouldBe(@"(?<n>[-]?\d+(?:\.\d+)?(?:[eE][-+]?\d+)?)");
        }

        [Fact]
        public void ReturnsUnboundPattern_WhenArgsIsEmpty()
        {
            var (pattern, _, _) = DoubleResolver.Resolve("n", string.Empty);
            pattern.ShouldBe(@"(?<n>[-]?\d+(?:\.\d+)?(?:[eE][-+]?\d+)?)");
        }

        [Fact]
        public void ReturnsWholeNumberPattern_WhenArgIsZero()
        {
            var (pattern, _, _) = DoubleResolver.Resolve("n", "0");
            pattern.ShouldBe(@"(?<n>[-]?\d+(?:\.\d{0})?(?:[eE][-+]?\d+)?)");
        }

        [Fact]
        public void ReturnsExactPrecisionPattern_WhenArgIsExact()
        {
            var (pattern, _, _) = DoubleResolver.Resolve("n", "2");
            pattern.ShouldBe(@"(?<n>[-]?\d+(?:\.\d{2})?(?:[eE][-+]?\d+)?)");
        }

        [Fact]
        public void ReturnsRangePattern_WhenArgIsRange()
        {
            var (pattern, _, _) = DoubleResolver.Resolve("n", "1,3");
            pattern.ShouldBe(@"(?<n>[-]?\d+(?:\.\d{1,3})?(?:[eE][-+]?\d+)?)");
        }

        [Fact]
        public void ReturnsCorrectStrictness()
        {
            var (_, strictness, _) = DoubleResolver.Resolve("n", null);
            strictness.ShouldBe(DoubleResolver.Strictness);
        }

        [Fact]
        public void ReturnsCorrectGroup()
        {
            var (_, _, group) = DoubleResolver.Resolve("n", null);
            group.ShouldBe(DoubleResolver.Group);
        }

        [Fact]
        public void Throws_WhenArgsIsInvalid()
        {
            Should.Throw<ArgumentException>(() => DoubleResolver.Resolve("n", "-1"));
        }
    }
}