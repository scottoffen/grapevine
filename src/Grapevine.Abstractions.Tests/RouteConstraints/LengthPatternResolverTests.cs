using Grapevine.Abstractions.RouteConstraints;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class LengthPatternResolverTests
{
    public class Resolve
    {
        [Fact]
        public void ReturnsPlus_WhenArgsIsNull()
        {
            LengthPatternResolver.Resolve(null).ShouldBe("+");
        }

        [Fact]
        public void ReturnsPlus_WhenArgsIsEmpty()
        {
            LengthPatternResolver.Resolve(string.Empty).ShouldBe("+");
        }

        [Fact]
        public void ReturnsPlus_WhenArgsIsWhitespace()
        {
            LengthPatternResolver.Resolve("   ").ShouldBe("+");
        }

        [Fact]
        public void ReturnsExactQuantifier_WhenArgIsInteger()
        {
            LengthPatternResolver.Resolve("3").ShouldBe("{3}");
        }

        [Fact]
        public void ReturnsMinQuantifier_WhenArgIsMinOnly()
        {
            LengthPatternResolver.Resolve("1,").ShouldBe("{1,}");
        }

        [Fact]
        public void ReturnsMaxQuantifier_WhenArgIsMaxOnly()
        {
            LengthPatternResolver.Resolve(",5").ShouldBe("{1,5}");
        }

        [Fact]
        public void ReturnsRangeQuantifier_WhenArgIsRange()
        {
            LengthPatternResolver.Resolve("1,5").ShouldBe("{1,5}");
        }

        [Fact]
        public void Throws_WhenBothSidesOfCommaAreEmpty()
        {
            Should.Throw<ArgumentException>(() => LengthPatternResolver.Resolve(","));
        }

        [Fact]
        public void Throws_WhenExactValueIsZero()
        {
            Should.Throw<ArgumentException>(() => LengthPatternResolver.Resolve("0"));
        }

        [Fact]
        public void Throws_WhenExactValueIsNegative()
        {
            Should.Throw<ArgumentException>(() => LengthPatternResolver.Resolve("-1"));
        }

        [Fact]
        public void Throws_WhenMinIsZero()
        {
            Should.Throw<ArgumentException>(() => LengthPatternResolver.Resolve("0,5"));
        }

        [Fact]
        public void Throws_WhenMaxIsZero()
        {
            Should.Throw<ArgumentException>(() => LengthPatternResolver.Resolve("1,0"));
        }

        [Fact]
        public void Throws_WhenMaxIsLessThanMin()
        {
            Should.Throw<ArgumentException>(() => LengthPatternResolver.Resolve("5,1"));
        }

        [Fact]
        public void Throws_WhenArgIsNotAnInteger()
        {
            Should.Throw<ArgumentException>(() => LengthPatternResolver.Resolve("abc"));
        }

        [Fact]
        public void ThrowsWithExpectedMessage_WhenArgIsInvalid()
        {
            var args = Guid.NewGuid().ToString();
            var ex   = Should.Throw<ArgumentException>(() => LengthPatternResolver.Resolve(args));
            ex.Message.ShouldContain(args);
        }
    }
}