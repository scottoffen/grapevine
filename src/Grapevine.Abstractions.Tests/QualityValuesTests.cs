using Grapevine;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests;

public class QualityValuesTests
{
    public class ParseMethod
    {
        [Fact]
        public void ReturnsSingleValue_WhenNoQualityFactor()
        {
            var result = QualityValues.Parse("text/html");
            result.ShouldBe(new[] { "text/html" });
        }

        [Fact]
        public void ReturnsMultipleValues_WhenNoQualityFactors()
        {
            var result = QualityValues.Parse("text/html, application/json, text/plain");
            result.ShouldBe(new[] { "text/html", "application/json", "text/plain" });
        }

        [Fact]
        public void ReturnsHighestQualityFactorFirst()
        {
            var result = QualityValues.Parse("text/plain;q=0.5, text/html;q=0.9, application/json;q=0.8");
            result[0].ShouldBe("text/html");
            result[1].ShouldBe("application/json");
            result[2].ShouldBe("text/plain");
        }

        [Fact]
        public void ImplicitQualityFactor_RanksAboveExplicit()
        {
            var result = QualityValues.Parse("text/html, application/json;q=0.9");
            result[0].ShouldBe("text/html");
            result[1].ShouldBe("application/json");
        }

        [Fact]
        public void SortsValuesBySpecificity_WithinSameQualityFactor()
        {
            var result = QualityValues.Parse("*/*, text/html, text/*");
            result[0].ShouldBe("text/html");
            result[1].ShouldBe("text/*");
            result[2].ShouldBe("*/*");
        }

        [Fact]
        public void HandlesMixedQualityFactorsAndSpecificities()
        {
            var result = QualityValues.Parse("text/html, application/json;q=0.9, text/*;q=0.9, */*;q=0.8");
            result[0].ShouldBe("text/html");
            result[1].ShouldBe("application/json");
            result[2].ShouldBe("text/*");
            result[3].ShouldBe("*/*");
        }
    }

    public class GroupByQualityFactorMethod
    {
        [Fact]
        public void DefaultsToQualityFactorOne_WhenNoQFactorPresent()
        {
            var result = QualityValues.GroupByQualityFactor("text/html");
            result.ShouldContainKey(1m);
            result[1m].ShouldContain("text/html");
        }

        [Fact]
        public void ParsesExplicitQualityFactor()
        {
            var result = QualityValues.GroupByQualityFactor("text/html;q=0.9");
            result.ShouldContainKey(0.9m);
            result[0.9m].ShouldContain("text/html");
        }

        [Fact]
        public void GroupsMultipleValuesAtSameQualityFactor()
        {
            var result = QualityValues.GroupByQualityFactor("text/html;q=0.9, application/json;q=0.9");
            result[0.9m].Count.ShouldBe(2);
            result[0.9m].ShouldContain("text/html");
            result[0.9m].ShouldContain("application/json");
        }

        [Fact]
        public void GroupsValuesAtDifferentQualityFactors()
        {
            var result = QualityValues.GroupByQualityFactor("text/html;q=0.9, application/json;q=0.8");
            result.ShouldContainKey(0.9m);
            result.ShouldContainKey(0.8m);
            result[0.9m].ShouldContain("text/html");
            result[0.8m].ShouldContain("application/json");
        }

        [Fact]
        public void ReturnsFallback_WhenQualityFactorIsMalformed()
        {
            var result = QualityValues.GroupByQualityFactor("text/html;q=abc");
            result.ShouldContainKey(1m);
            result[1m].ShouldContain("text/html");
        }

        [Fact]
        public void ReturnsHighestQualityFactorFirst()
        {
            var result = QualityValues.GroupByQualityFactor("text/html;q=0.5, application/json;q=0.9");
            result.Keys.First().ShouldBe(0.9m);
        }
    }

    public class SortBySpecificityMethod
    {
        [Fact]
        public void ReturnsSingleValue_AsIs()
        {
            var result = QualityValues.SortBySpecificity(new[] { "text/html" });
            result.ShouldBe(new[] { "text/html" });
        }

        [Fact]
        public void FullySpecificValuesRankFirst()
        {
            var result = QualityValues.SortBySpecificity(new[] { "*/*", "text/html" });
            result[0].ShouldBe("text/html");
        }

        [Fact]
        public void PartialWildcardsRankAboveFullWildcards()
        {
            var result = QualityValues.SortBySpecificity(new[] { "*/*", "text/*" });
            result[0].ShouldBe("text/*");
            result[1].ShouldBe("*/*");
        }

        [Fact]
        public void FullySpecific_ThenPartial_ThenWildcard()
        {
            var result = QualityValues.SortBySpecificity(new[] { "*/*", "text/*", "text/html" });
            result[0].ShouldBe("text/html");
            result[1].ShouldBe("text/*");
            result[2].ShouldBe("*/*");
        }

        [Fact]
        public void MultipleFullySpecificValuesPreserveRelativeOrder()
        {
            var result = QualityValues.SortBySpecificity(
                new[] { "text/html", "application/json", "text/plain" });
            result[0].ShouldBe("text/html");
            result[1].ShouldBe("application/json");
            result[2].ShouldBe("text/plain");
        }

        [Fact]
        public void MultiplePartialWildcardsPreserveRelativeOrder()
        {
            var result = QualityValues.SortBySpecificity(
                new[] { "*/*", "text/*", "image/*" });
            result[0].ShouldBe("text/*");
            result[1].ShouldBe("image/*");
            result[2].ShouldBe("*/*");
        }
    }
}