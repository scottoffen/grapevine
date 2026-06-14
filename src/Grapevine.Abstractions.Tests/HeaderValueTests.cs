using Grapevine;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests;

public class HeaderValueTests
{
    public class Constructor
    {
        [Fact]
        public void SetsValue()
        {
            var hv = new HeaderValue("gzip");
            hv.Value.ShouldBe("gzip");
        }

        [Fact]
        public void TrimsValue()
        {
            var hv = new HeaderValue("  gzip  ");
            hv.Value.ShouldBe("gzip");
        }

        [Fact]
        public void SetsQuality_WhenProvided()
        {
            var hv = new HeaderValue("gzip", 0.9);
            hv.Quality.ShouldBe(0.9);
        }

        [Fact]
        public void QualityIsNull_WhenNotProvided()
        {
            var hv = new HeaderValue("gzip");
            hv.Quality.ShouldBeNull();
        }

        [Fact]
        public void AcceptsQualityOfZero()
        {
            var hv = new HeaderValue("gzip", 0.0);
            hv.Quality.ShouldBe(0.0);
        }

        [Fact]
        public void AcceptsQualityOfOne()
        {
            var hv = new HeaderValue("gzip", 1.0);
            hv.Quality.ShouldBe(1.0);
        }

        [Fact]
        public void Throws_WhenValueIsNull()
        {
            Should.Throw<ArgumentNullException>(() => new HeaderValue(null!));
        }

        [Fact]
        public void Throws_WhenQualityIsNegative()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => new HeaderValue("gzip", -0.1));
        }

        [Fact]
        public void Throws_WhenQualityExceedsOne()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => new HeaderValue("gzip", 1.1));
        }

        [Fact]
        public void DefaultWeightIsZero()
        {
            var hv = new HeaderValue("gzip");
            hv.Weight.ShouldBe(0);
        }

        [Fact]
        public void SetsWeight_WhenProvided()
        {
            var hv = new HeaderValue("gzip", weight: HeaderValue.WeightSpecific);
            hv.Weight.ShouldBe(HeaderValue.WeightSpecific);
        }
    }

    public class EffectiveQualityProperty
    {
        [Fact]
        public void ReturnsQuality_WhenSet()
        {
            var hv = new HeaderValue("gzip", 0.5);
            hv.EffectiveQuality.ShouldBe(0.5);
        }

        [Fact]
        public void ReturnsOne_WhenQualityIsNull()
        {
            var hv = new HeaderValue("gzip");
            hv.EffectiveQuality.ShouldBe(1.0);
        }

        [Fact]
        public void ReturnsZero_WhenQualityIsZero()
        {
            var hv = new HeaderValue("gzip", 0.0);
            hv.EffectiveQuality.ShouldBe(0.0);
        }
    }

    public class ToStringMethod
    {
        [Fact]
        public void ReturnsValueOnly_WhenNoQuality()
        {
            var hv = new HeaderValue("gzip");
            hv.ToString().ShouldBe("gzip");
        }

        [Fact]
        public void ReturnsValueAndQuality_WhenQualityIsSet()
        {
            var hv = new HeaderValue("gzip", 0.9);
            hv.ToString().ShouldBe("gzip;q=0.9");
        }

        [Fact]
        public void FormatsQualityWithUpToThreeDecimalPlaces()
        {
            var hv = new HeaderValue("gzip", 0.123);
            hv.ToString().ShouldBe("gzip;q=0.123");
        }

        [Fact]
        public void OmitsTrailingZeros_InQuality()
        {
            var hv = new HeaderValue("gzip", 0.9);
            hv.ToString().ShouldBe("gzip;q=0.9");
        }

        [Fact]
        public void FormatsQualityOfZero()
        {
            var hv = new HeaderValue("gzip", 0.0);
            hv.ToString().ShouldBe("gzip;q=0");
        }

        [Fact]
        public void FormatsQualityOfOne()
        {
            var hv = new HeaderValue("gzip", 1.0);
            hv.ToString().ShouldBe("gzip;q=1");
        }

        [Fact]
        public void UsesInvariantCulture_ForQualityFormatting()
        {
            // Ensures decimal separator is always '.' regardless of system locale.
            var hv = new HeaderValue("gzip", 0.5);
            hv.ToString().ShouldContain("0.5");
            hv.ToString().ShouldNotContain("0,5");
        }
    }

    public class EqualsMethod
    {
        [Fact]
        public void ReturnsTrue_ForSameInstance()
        {
            var hv = new HeaderValue("gzip");
            hv.Equals(hv).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenValueAndQualityMatch()
        {
            var a = new HeaderValue("gzip", 0.9);
            var b = new HeaderValue("gzip", 0.9);
            a.Equals(b).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenValueMatchesCaseInsensitively()
        {
            var a = new HeaderValue("gzip");
            var b = new HeaderValue("GZIP");
            a.Equals(b).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenValuesDiffer()
        {
            var a = new HeaderValue("gzip");
            var b = new HeaderValue("deflate");
            a.Equals(b).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenQualitiesDiffer()
        {
            var a = new HeaderValue("gzip", 0.9);
            var b = new HeaderValue("gzip", 0.8);
            a.Equals(b).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenOneQualityIsNull()
        {
            var a = new HeaderValue("gzip");
            var b = new HeaderValue("gzip", 0.9);
            a.Equals(b).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenOtherIsNull()
        {
            var hv = new HeaderValue("gzip");
            hv.Equals((HeaderValue?)null).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenObjectIsUnrelatedType()
        {
            var hv = new HeaderValue("gzip");
            hv.Equals(42).ShouldBeFalse();
        }

        [Fact]
        public void EqualityIgnoresWeight()
        {
            // Weight is an internal sort hint, not part of the value identity.
            var a = new HeaderValue("text/html", weight: HeaderValue.WeightSpecific);
            var b = new HeaderValue("text/html", weight: HeaderValue.WeightWildcard);
            a.Equals(b).ShouldBeTrue();
        }
    }

    public class EqualityOperators
    {
        [Fact]
        public void EqualsOperator_ReturnsTrue_WhenBothNull()
        {
            HeaderValue? a = null;
            HeaderValue? b = null;
            (a == b).ShouldBeTrue();
        }

        [Fact]
        public void EqualsOperator_ReturnsFalse_WhenOneIsNull()
        {
            HeaderValue? a = null;
            var b = new HeaderValue("gzip");
            (a == b).ShouldBeFalse();
        }

        [Fact]
        public void EqualsOperator_ReturnsTrue_WhenValuesAndQualityMatch()
        {
            var a = new HeaderValue("gzip", 0.9);
            var b = new HeaderValue("gzip", 0.9);
            (a == b).ShouldBeTrue();
        }

        [Fact]
        public void InequalityOperator_ReturnsTrue_WhenValuesDiffer()
        {
            var a = new HeaderValue("gzip");
            var b = new HeaderValue("deflate");
            (a != b).ShouldBeTrue();
        }
    }

    public class GetHashCodeMethod
    {
        [Fact]
        public void IsConsistentWithEquality_ForEqualInstances()
        {
            var a = new HeaderValue("gzip", 0.9);
            var b = new HeaderValue("gzip", 0.9);
            a.GetHashCode().ShouldBe(b.GetHashCode());
        }

        [Fact]
        public void IsConsistentWithEquality_ForCaseInsensitiveValues()
        {
            var a = new HeaderValue("gzip");
            var b = new HeaderValue("GZIP");
            a.GetHashCode().ShouldBe(b.GetHashCode());
        }

        [Fact]
        public void DiffersForDifferentValues()
        {
            var a = new HeaderValue("gzip");
            var b = new HeaderValue("deflate");
            a.GetHashCode().ShouldNotBe(b.GetHashCode());
        }
    }

    public class CompareToMethod
    {
        [Fact]
        public void ReturnsNegative_WhenOtherIsNull()
        {
            var hv = new HeaderValue("gzip");
            hv.CompareTo(null).ShouldBeLessThan(0);
        }

        [Fact]
        public void ReturnsZero_ForEqualInstances()
        {
            var a = new HeaderValue("gzip", 0.9);
            var b = new HeaderValue("gzip", 0.9);
            a.CompareTo(b).ShouldBe(0);
        }

        [Fact]
        public void SortsByDescendingQuality()
        {
            var high = new HeaderValue("gzip", 0.9);
            var low = new HeaderValue("deflate", 0.5);

            // high should sort before low, so CompareTo returns negative
            high.CompareTo(low).ShouldBeLessThan(0);
            low.CompareTo(high).ShouldBeGreaterThan(0);
        }

        [Fact]
        public void TreatsNullQualityAsOne_WhenComparingQuality()
        {
            var implicit1 = new HeaderValue("gzip");          // effective quality 1.0
            var explicit09 = new HeaderValue("deflate", 0.9);  // effective quality 0.9
            implicit1.CompareTo(explicit09).ShouldBeLessThan(0);
        }

        [Fact]
        public void SortsByDescendingWeight_WhenQualityIsEqual()
        {
            var specific = HeaderValue.FromMimeType("text/html");
            var wildcard = HeaderValue.FromMimeType("text/*");

            // specific weight > wildcard weight, so specific sorts first
            specific.CompareTo(wildcard).ShouldBeLessThan(0);
            wildcard.CompareTo(specific).ShouldBeGreaterThan(0);
        }

        [Fact]
        public void SortsByAscendingValue_WhenQualityAndWeightAreEqual()
        {
            var a = new HeaderValue("aaa");
            var b = new HeaderValue("zzz");

            a.CompareTo(b).ShouldBeLessThan(0);
            b.CompareTo(a).ShouldBeGreaterThan(0);
        }

        [Fact]
        public void ValueSortIsCaseInsensitive()
        {
            var lower = new HeaderValue("aaa");
            var upper = new HeaderValue("AAA");
            lower.CompareTo(upper).ShouldBe(0);
        }

        [Fact]
        public void SortIsStable_WhenAllFactorsAreEqual()
        {
            var a = new HeaderValue("gzip");
            var b = new HeaderValue("gzip");
            a.CompareTo(b).ShouldBe(0);
        }

        [Fact]
        public void ListSort_ProducesCorrectPreferenceOrder()
        {
            // Simulates a real Accept-Encoding header:
            // gzip;q=0.9, deflate;q=0.8, br, identity;q=0.5
            // Expected order after sort: br (q=1), gzip (q=0.9), deflate (q=0.8), identity (q=0.5)
            var values = new List<HeaderValue>
            {
                new HeaderValue("gzip", 0.9),
                new HeaderValue("deflate", 0.8),
                new HeaderValue("br"),
                new HeaderValue("identity", 0.5),
            };

            values.Sort();

            values[0].Value.ShouldBe("br");
            values[1].Value.ShouldBe("gzip");
            values[2].Value.ShouldBe("deflate");
            values[3].Value.ShouldBe("identity");
        }
    }

    public class FromMimeTypeMethod
    {
        [Fact]
        public void SetsValue()
        {
            var hv = HeaderValue.FromMimeType("text/html");
            hv.Value.ShouldBe("text/html");
        }

        [Fact]
        public void SetsQuality_WhenProvided()
        {
            var hv = HeaderValue.FromMimeType("text/html", 0.9);
            hv.Quality.ShouldBe(0.9);
        }

        [Fact]
        public void QualityIsNull_WhenNotProvided()
        {
            var hv = HeaderValue.FromMimeType("text/html");
            hv.Quality.ShouldBeNull();
        }

        [Fact]
        public void AssignsWeightSpecific_ForFullySpecifiedMimeType()
        {
            var hv = HeaderValue.FromMimeType("text/html");
            hv.Weight.ShouldBe(HeaderValue.WeightSpecific);
        }

        [Fact]
        public void AssignsWeightPartialWildcard_ForWildcardSubtype()
        {
            var hv = HeaderValue.FromMimeType("text/*");
            hv.Weight.ShouldBe(HeaderValue.WeightPartialWildcard);
        }

        [Fact]
        public void AssignsWeightWildcard_ForFullWildcard()
        {
            var hv = HeaderValue.FromMimeType("*/*");
            hv.Weight.ShouldBe(HeaderValue.WeightWildcard);
        }

        [Fact]
        public void SpecificOutranksPartialWildcard_WhenQualityIsEqual()
        {
            var specific = HeaderValue.FromMimeType("text/html");
            var partial = HeaderValue.FromMimeType("text/*");
            specific.CompareTo(partial).ShouldBeLessThan(0);
        }

        [Fact]
        public void PartialWildcardOutranksFullWildcard_WhenQualityIsEqual()
        {
            var partial = HeaderValue.FromMimeType("text/*");
            var full = HeaderValue.FromMimeType("*/*");
            partial.CompareTo(full).ShouldBeLessThan(0);
        }

        [Fact]
        public void LowerQualityOutweighsHigherSpecificity()
        {
            // A lower quality specific type should still sort after a higher
            // quality wildcard, because quality is the primary sort key.
            var specificLowQ = HeaderValue.FromMimeType("text/html", 0.5);
            var wildcardHighQ = HeaderValue.FromMimeType("*/*", 0.9);
            wildcardHighQ.CompareTo(specificLowQ).ShouldBeLessThan(0);
        }

        [Fact]
        public void ListSort_ProducesCorrectRfc7231Order()
        {
            // Simulates: Accept: text/html, application/json;q=0.9, text/*;q=0.8, */*;q=0.7
            // Expected: text/html (q=1, specific), application/json (q=0.9, specific),
            //           text/* (q=0.8, partial), */* (q=0.7, wildcard)
            var values = new List<HeaderValue>
            {
                HeaderValue.FromMimeType("*/*", 0.7),
                HeaderValue.FromMimeType("text/*", 0.8),
                HeaderValue.FromMimeType("application/json", 0.9),
                HeaderValue.FromMimeType("text/html"),
            };

            values.Sort();

            values[0].Value.ShouldBe("text/html");
            values[1].Value.ShouldBe("application/json");
            values[2].Value.ShouldBe("text/*");
            values[3].Value.ShouldBe("*/*");
        }
    }
}