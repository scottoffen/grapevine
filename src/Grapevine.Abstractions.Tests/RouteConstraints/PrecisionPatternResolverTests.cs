using Grapevine.Abstractions.RouteConstraints;
using Shouldly;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class PrecisionPatternResolverTests
{
    public class Resolve
    {
        [Fact]
        public void ReturnsPlus_WhenArgsIsNull()
        {
            PrecisionPatternResolver.Resolve(null).ShouldBe("+");
        }

        [Fact]
        public void ReturnsPlus_WhenArgsIsEmpty()
        {
            PrecisionPatternResolver.Resolve(string.Empty).ShouldBe("+");
        }

        [Fact]
        public void ReturnsPlus_WhenArgsIsWhitespace()
        {
            PrecisionPatternResolver.Resolve("   ").ShouldBe("+");
        }

        [Fact]
        public void ReturnsZeroQuantifier_WhenArgIsZero()
        {
            PrecisionPatternResolver.Resolve("0").ShouldBe("{0}");
        }

        [Fact]
        public void ReturnsExactQuantifier_WhenArgIsInteger()
        {
            PrecisionPatternResolver.Resolve("2").ShouldBe("{2}");
        }

        [Fact]
        public void ReturnsMinQuantifier_WhenArgIsMinOnly()
        {
            PrecisionPatternResolver.Resolve("0,").ShouldBe("{0,}");
        }

        [Fact]
        public void ReturnsMaxQuantifier_WhenArgIsMaxOnly()
        {
            PrecisionPatternResolver.Resolve(",3").ShouldBe("{0,3}");
        }

        [Fact]
        public void ReturnsRangeQuantifier_WhenArgIsRange()
        {
            PrecisionPatternResolver.Resolve("1,3").ShouldBe("{1,3}");
        }

        [Fact]
        public void Throws_WhenBothSidesOfCommaAreEmpty()
        {
            Should.Throw<ArgumentException>(() => PrecisionPatternResolver.Resolve(","));
        }

        [Fact]
        public void Throws_WhenExactValueIsNegative()
        {
            Should.Throw<ArgumentException>(() => PrecisionPatternResolver.Resolve("-1"));
        }

        [Fact]
        public void Throws_WhenMaxIsNegative()
        {
            Should.Throw<ArgumentException>(() => PrecisionPatternResolver.Resolve("0,-1"));
        }

        [Fact]
        public void Throws_WhenMaxIsLessThanMin()
        {
            Should.Throw<ArgumentException>(() => PrecisionPatternResolver.Resolve("3,1"));
        }

        [Fact]
        public void Throws_WhenArgIsNotAnInteger()
        {
            Should.Throw<ArgumentException>(() => PrecisionPatternResolver.Resolve("abc"));
        }

        [Fact]
        public void ThrowsWithExpectedMessage_WhenArgIsInvalid()
        {
            var args = Guid.NewGuid().ToString();
            var ex   = Should.Throw<ArgumentException>(() => PrecisionPatternResolver.Resolve(args));
            ex.Message.ShouldContain(args);
        }
    }
}