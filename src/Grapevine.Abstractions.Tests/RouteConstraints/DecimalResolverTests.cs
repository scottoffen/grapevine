using Grapevine.Abstractions.RouteConstraints;
using Shouldly;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class DecimalResolverTests
{
    public class Resolve
    {
        [Fact]
        public void ReturnsUnboundPattern_WhenArgsIsNull()
        {
            var (pattern, _, _) = DecimalResolver.Resolve("n", null);
            pattern.ShouldBe(@"(?<n>[-]?\d+(?:\.\d+)?)");
        }

        [Fact]
        public void ReturnsUnboundPattern_WhenArgsIsEmpty()
        {
            var (pattern, _, _) = DecimalResolver.Resolve("n", string.Empty);
            pattern.ShouldBe(@"(?<n>[-]?\d+(?:\.\d+)?)");
        }

        [Fact]
        public void ReturnsWholeNumberPattern_WhenArgIsZero()
        {
            var (pattern, _, _) = DecimalResolver.Resolve("n", "0");
            pattern.ShouldBe(@"(?<n>[-]?\d+(?:\.\d{0})?)");
        }

        [Fact]
        public void ReturnsExactPrecisionPattern_WhenArgIsExact()
        {
            var (pattern, _, _) = DecimalResolver.Resolve("n", "2");
            pattern.ShouldBe(@"(?<n>[-]?\d+(?:\.\d{2})?)");
        }

        [Fact]
        public void ReturnsRangePattern_WhenArgIsRange()
        {
            var (pattern, _, _) = DecimalResolver.Resolve("n", "1,3");
            pattern.ShouldBe(@"(?<n>[-]?\d+(?:\.\d{1,3})?)");
        }

        [Fact]
        public void ReturnsCorrectStrictness()
        {
            var (_, strictness, _) = DecimalResolver.Resolve("n", null);
            strictness.ShouldBe(DecimalResolver.Strictness);
        }

        [Fact]
        public void ReturnsCorrectGroup()
        {
            var (_, _, group) = DecimalResolver.Resolve("n", null);
            group.ShouldBe(DecimalResolver.Group);
        }

        [Fact]
        public void Throws_WhenArgsIsInvalid()
        {
            Should.Throw<ArgumentException>(() => DecimalResolver.Resolve("n", "-1"));
        }
    }
}