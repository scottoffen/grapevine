using Grapevine.Abstractions.RouteConstraints;
using Shouldly;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class NumericResolverTests
{
    public class Resolve
    {
        [Fact]
        public void ReturnsUnboundPattern_WhenArgsIsNull()
        {
            var (pattern, _, _) = NumericResolver.Resolve("n", null);
            pattern.ShouldBe(@"(?<n>\d+)");
        }

        [Fact]
        public void ReturnsUnboundPattern_WhenArgsIsEmpty()
        {
            var (pattern, _, _) = NumericResolver.Resolve("n", string.Empty);
            pattern.ShouldBe(@"(?<n>\d+)");
        }

        [Fact]
        public void ReturnsExactLengthPattern_WhenArgIsExact()
        {
            var (pattern, _, _) = NumericResolver.Resolve("n", "3");
            pattern.ShouldBe(@"(?<n>\d{3})");
        }

        [Fact]
        public void ReturnsMinLengthPattern_WhenArgIsMinOnly()
        {
            var (pattern, _, _) = NumericResolver.Resolve("n", "1,");
            pattern.ShouldBe(@"(?<n>\d{1,})");
        }

        [Fact]
        public void ReturnsRangePattern_WhenArgIsRange()
        {
            var (pattern, _, _) = NumericResolver.Resolve("n", "1,5");
            pattern.ShouldBe(@"(?<n>\d{1,5})");
        }

        [Fact]
        public void ReturnsCorrectStrictness()
        {
            var (_, strictness, _) = NumericResolver.Resolve("n", null);
            strictness.ShouldBe(NumericResolver.Strictness);
        }

        [Fact]
        public void ReturnsCorrectGroup()
        {
            var (_, _, group) = NumericResolver.Resolve("n", null);
            group.ShouldBe(NumericResolver.Group);
        }

        [Fact]
        public void Throws_WhenArgsIsInvalid()
        {
            Should.Throw<ArgumentException>(() => NumericResolver.Resolve("n", "0"));
        }
    }
}