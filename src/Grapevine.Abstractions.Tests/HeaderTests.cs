using Grapevine;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests;

public class HeaderTests
{
    public class Constructor
    {
        [Fact]
        public void SetsName()
        {
            var h = new Header("Accept");
            h.Name.ShouldBe("Accept");
        }

        [Fact]
        public void Throws_WhenNameIsNull()
        {
            Should.Throw<ArgumentNullException>(() => new Header(null!));
        }

        [Fact]
        public void HasNoValues_WhenValueIsNull()
        {
            var h = new Header("Accept");
            h.HasValue.ShouldBeFalse();
            h.Values.Count.ShouldBe(0);
        }

        [Fact]
        public void HasNoValues_WhenValueIsEmpty()
        {
            var h = new Header("Accept", string.Empty);
            h.HasValue.ShouldBeFalse();
        }

        [Fact]
        public void HasNoValues_WhenValueIsWhitespace()
        {
            var h = new Header("Accept", "   ");
            h.HasValue.ShouldBeFalse();
        }

        [Fact]
        public void ParsesSingleValue()
        {
            var h = new Header("Accept", "text/html");
            h.Values.Count.ShouldBe(1);
            h.Values[0].Value.ShouldBe("text/html");
        }

        [Fact]
        public void ParsesMultipleCommaSeparatedValues()
        {
            var h = new Header("Accept", "text/html, application/json, text/plain");
            h.Values.Count.ShouldBe(3);
        }

        [Fact]
        public void TrimsWhitespaceFromEachValue()
        {
            var h = new Header("Accept", "  text/html  ,  application/json  ");
            h.Values[0].Value.ShouldBe("text/html");
            h.Values[1].Value.ShouldBe("application/json");
        }

        [Fact]
        public void SkipsEmptySegments()
        {
            var h = new Header("Accept", "text/html,,application/json");
            h.Values.Count.ShouldBe(2);
        }

        [Fact]
        public void ParsesQualityFactor_WhenPresent()
        {
            var h = new Header("Accept", "text/html;q=0.9");
            h.Values[0].Quality.ShouldBe(0.9);
        }

        [Fact]
        public void ParsesQualityFactor_CaseInsensitive()
        {
            var h = new Header("Accept", "text/html;Q=0.9");
            h.Values[0].Quality.ShouldBe(0.9);
        }

        [Fact]
        public void ParsesMixedValuesWithAndWithoutQuality()
        {
            var h = new Header("Accept", "text/html, application/json;q=0.9");
            h.Values[0].Quality.ShouldBeNull();
            h.Values[1].Quality.ShouldBe(0.9);
        }

        [Fact]
        public void IgnoresMalformedQualityFactor_AddsValueWithoutQuality()
        {
            // A semicolon-delimited part that is not a valid q= expression
            // should result in the value being added without a quality factor.
            var h = new Header("Accept", "text/html;charset=UTF-8");
            h.Values.Count.ShouldBe(1);
            h.Values[0].Value.ShouldBe("text/html");
            h.Values[0].Quality.ShouldBeNull();
        }

        [Fact]
        public void IgnoresUnparsableQualityNumber_AddsValueWithoutQuality()
        {
            var h = new Header("Accept", "text/html;q=not-a-number");
            h.Values[0].Quality.ShouldBeNull();
        }
    }

    public class ValueProperty
    {
        [Fact]
        public void ReturnsNull_WhenNoValues()
        {
            var h = new Header("Accept");
            h.Value.ShouldBeNull();
        }

        [Fact]
        public void ReturnsFirstValue_WhenOneValue()
        {
            var h = new Header("Accept", "text/html");
            h.Value.ShouldBe("text/html");
        }

        [Fact]
        public void ReturnsFirstValue_WhenMultipleValues()
        {
            var h = new Header("Accept", "text/html, application/json");
            h.Value.ShouldBe("text/html");
        }
    }

    public class HasValueProperty
    {
        [Fact]
        public void ReturnsFalse_WhenNoValues()
        {
            var h = new Header("Accept");
            h.HasValue.ShouldBeFalse();
        }

        [Fact]
        public void ReturnsTrue_WhenOneValue()
        {
            var h = new Header("Accept", "text/html");
            h.HasValue.ShouldBeTrue();
        }
    }

    public class IsMultiValueProperty
    {
        [Fact]
        public void ReturnsFalse_WhenNoValues()
        {
            var h = new Header("Accept");
            h.IsMultiValue.ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenOneValue()
        {
            var h = new Header("Accept", "text/html");
            h.IsMultiValue.ShouldBeFalse();
        }

        [Fact]
        public void ReturnsTrue_WhenMultipleValues()
        {
            var h = new Header("Accept", "text/html, application/json");
            h.IsMultiValue.ShouldBeTrue();
        }
    }

    public class ToStringMethod
    {
        [Fact]
        public void ReturnsNameOnly_WhenNoValues()
        {
            var h = new Header("Accept");
            h.ToString().ShouldBe("Accept");
        }

        [Fact]
        public void ReturnsNameAndValue_WhenOneValue()
        {
            var h = new Header("Accept", "text/html");
            h.ToString().ShouldBe("Accept: text/html");
        }

        [Fact]
        public void ReturnsNameAndCommaSeparatedValues_WhenMultipleValues()
        {
            var h = new Header("Accept", "text/html, application/json");
            h.ToString().ShouldBe("Accept: text/html, application/json");
        }

        [Fact]
        public void IncludesQualityFactor_WhenPresent()
        {
            var h = new Header("Accept", "text/html;q=0.9");
            h.ToString().ShouldBe("Accept: text/html;q=0.9");
        }
    }

    public class ImplicitStringConversion
    {
        [Fact]
        public void ReturnsToStringResult()
        {
            var h = new Header("Accept", "text/html");
            string result = h;
            result.ShouldBe(h.ToString());
        }

        [Fact]
        public void ReturnsNameOnly_WhenNoValues()
        {
            var h = new Header("Accept");
            string result = h;
            result.ShouldBe("Accept");
        }
    }

    public class EqualsMethod
    {
        [Fact]
        public void ReturnsTrue_ForSameInstance()
        {
            var h = new Header("Accept", "text/html");
            h.Equals(h).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenNameAndValuesMatch()
        {
            var a = new Header("Accept", "text/html");
            var b = new Header("Accept", "text/html");
            a.Equals(b).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenNameMatchesCaseInsensitively()
        {
            var a = new Header("accept", "text/html");
            var b = new Header("ACCEPT", "text/html");
            a.Equals(b).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenNamesDiffer()
        {
            var a = new Header("Accept", "text/html");
            var b = new Header("Content-Type", "text/html");
            a.Equals(b).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenValuesDiffer()
        {
            var a = new Header("Accept", "text/html");
            var b = new Header("Accept", "application/json");
            a.Equals(b).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenValueCountsDiffer()
        {
            var a = new Header("Accept", "text/html");
            var b = new Header("Accept", "text/html, application/json");
            a.Equals(b).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenOtherIsNull()
        {
            var h = new Header("Accept", "text/html");
            h.Equals((Header?)null).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenObjectIsUnrelatedType()
        {
            var h = new Header("Accept", "text/html");
            h.Equals(42).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsTrue_WhenBothHaveNoValues()
        {
            var a = new Header("Accept");
            var b = new Header("Accept");
            a.Equals(b).ShouldBeTrue();
        }
    }

    public class EqualityOperators
    {
        [Fact]
        public void EqualsOperator_ReturnsTrue_WhenBothNull()
        {
            Header? a = null;
            Header? b = null;
            (a == b).ShouldBeTrue();
        }

        [Fact]
        public void EqualsOperator_ReturnsFalse_WhenOneIsNull()
        {
            Header? a = null;
            var b = new Header("Accept", "text/html");
            (a == b).ShouldBeFalse();
        }

        [Fact]
        public void EqualsOperator_ReturnsTrue_WhenNameAndValuesMatch()
        {
            var a = new Header("Accept", "text/html");
            var b = new Header("Accept", "text/html");
            (a == b).ShouldBeTrue();
        }

        [Fact]
        public void InequalityOperator_ReturnsTrue_WhenValuesDiffer()
        {
            var a = new Header("Accept", "text/html");
            var b = new Header("Accept", "application/json");
            (a != b).ShouldBeTrue();
        }
    }

    public class GetHashCodeMethod
    {
        [Fact]
        public void IsConsistentWithEquality_ForEqualInstances()
        {
            var a = new Header("Accept", "text/html");
            var b = new Header("Accept", "text/html");
            a.GetHashCode().ShouldBe(b.GetHashCode());
        }

        [Fact]
        public void IsConsistentWithEquality_ForCaseInsensitiveName()
        {
            var a = new Header("accept", "text/html");
            var b = new Header("ACCEPT", "text/html");
            a.GetHashCode().ShouldBe(b.GetHashCode());
        }

        [Fact]
        public void DiffersForDifferentNames()
        {
            var a = new Header("Accept", "text/html");
            var b = new Header("Content-Type", "text/html");
            a.GetHashCode().ShouldNotBe(b.GetHashCode());
        }
    }
}