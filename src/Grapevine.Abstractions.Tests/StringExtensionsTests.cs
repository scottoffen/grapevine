namespace Grapevine.Abstractions.Tests;

public class StringExtensionsTests
{
    public class TrimPath
    {
        [Fact]
        public void ReturnsEmptyString_WhenPathIsNull()
        {
            string? path = null;
            path!.TrimPath().ShouldBe(string.Empty);
        }

        [Fact]
        public void ReturnsEmptyString_WhenPathIsEmpty()
        {
            string.Empty.TrimPath().ShouldBe(string.Empty);
        }

        [Fact]
        public void ReturnsEmptyString_WhenPathIsWhitespace()
        {
            "   ".TrimPath().ShouldBe(string.Empty);
        }

        [Fact]
        public void ReturnsEmptyString_WhenPathIsOnlySlashes()
        {
            "///".TrimPath().ShouldBe(string.Empty);
        }

        [Fact]
        public void ReturnsSingleLeadingSlash_WhenPathHasNoSlashes()
        {
            "api".TrimPath().ShouldBe("/api");
        }

        [Fact]
        public void ReturnsSingleLeadingSlash_WhenPathAlreadyHasLeadingSlash()
        {
            "/api".TrimPath().ShouldBe("/api");
        }

        [Fact]
        public void RemovesTrailingSlash()
        {
            "api/".TrimPath().ShouldBe("/api");
        }

        [Fact]
        public void RemovesMultipleLeadingAndTrailingSlashes()
        {
            "///api///".TrimPath().ShouldBe("/api");
        }

        [Fact]
        public void PreservesInternalSlashes()
        {
            "api/v1/endpoint".TrimPath().ShouldBe("/api/v1/endpoint");
        }

        [Fact]
        public void NormalizesConsecutiveInternalSlashes()
        {
            "api//v1///endpoint".TrimPath().ShouldBe("/api/v1/endpoint");
        }

        [Fact]
        public void TrimsWhitespaceAroundPath()
        {
            "  /api/v1/  ".TrimPath().ShouldBe("/api/v1");
        }
    }

    public class ContainsAny_Params
    {
        [Fact]
        public void ReturnsTrue_WhenSourceContainsOneOfTheValues()
        {
            "foobar".ContainsAny("oba", "baz").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenSourceContainsNoneOfTheValues()
        {
            "foobar".ContainsAny("qux", "baz").ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            "FooBar".ContainsAny("foo", "baz").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenSourceIsNull()
        {
            string? source = null;
            source.ContainsAny("foo", "bar").ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenSourceIsEmpty()
        {
            string.Empty.ContainsAny("foo", "bar").ShouldBeFalse();
        }

        [Fact]
        public void ReturnsTrue_WhenValueIsExactMatch()
        {
            "foo".ContainsAny("foo", "bar").ShouldBeTrue();
        }
    }

    public class ContainsAny_Comparison
    {
        [Fact]
        public void ReturnsTrue_WhenSourceContainsOneOfTheValues()
        {
            "foobar".ContainsAny(StringComparison.Ordinal, "oba", "baz").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenSourceContainsNoneOfTheValues()
        {
            "foobar".ContainsAny(StringComparison.Ordinal, "qux", "baz").ShouldBeFalse();
        }

        [Fact]
        public void IsCaseSensitive_WhenOrdinalComparisonSpecified()
        {
            "FooBar".ContainsAny(StringComparison.Ordinal, "foo", "baz").ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive_WhenOrdinalIgnoreCaseComparisonSpecified()
        {
            "FooBar".ContainsAny(StringComparison.OrdinalIgnoreCase, "foo", "baz").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenSourceIsNull()
        {
            string? source = null;
            source.ContainsAny(StringComparison.Ordinal, "foo", "bar").ShouldBeFalse();
        }
    }

    public class ContainsAny_Enumerable
    {
        [Fact]
        public void ReturnsTrue_WhenSourceContainsOneOfTheValues()
        {
            "foobar".ContainsAny(new List<string> { "oba", "baz" }).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenSourceContainsNoneOfTheValues()
        {
            "foobar".ContainsAny(new List<string> { "qux", "baz" }).ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive_ByDefault()
        {
            "FooBar".ContainsAny(new List<string> { "foo", "baz" }).ShouldBeTrue();
        }

        [Fact]
        public void IsCaseSensitive_WhenOrdinalComparisonSpecified()
        {
            "FooBar".ContainsAny(new List<string> { "foo", "baz" }, StringComparison.Ordinal).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenSourceIsNull()
        {
            string? source = null;
            source.ContainsAny(new List<string> { "foo", "bar" }).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenCollectionIsNull()
        {
            "foobar".ContainsAny((IEnumerable<string>?)null).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenCollectionIsEmpty()
        {
            "foobar".ContainsAny(new List<string>()).ShouldBeFalse();
        }
    }

    public class ContainsIgnoreCase
    {
        [Fact]
        public void ReturnsTrue_WhenSourceContainsValue()
        {
            "foobar".ContainsIgnoreCase("oba").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenSourceDoesNotContainValue()
        {
            "foobar".ContainsIgnoreCase("qux").ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive_ByDefault()
        {
            "FooBar".ContainsIgnoreCase("foo").ShouldBeTrue();
        }

        [Fact]
        public void IsCaseSensitive_WhenOrdinalComparisonSpecified()
        {
            "FooBar".ContainsIgnoreCase("foo", StringComparison.Ordinal).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenSourceIsNull()
        {
            string? source = null;
            source.ContainsIgnoreCase("foo").ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenSourceIsEmpty()
        {
            string.Empty.ContainsIgnoreCase("foo").ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenValueIsNull()
        {
            "foobar".ContainsIgnoreCase(null).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenValueIsEmpty()
        {
            "foobar".ContainsIgnoreCase(string.Empty).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsTrue_WhenValueIsExactMatch()
        {
            "foo".ContainsIgnoreCase("foo").ShouldBeTrue();
        }
    }

    public class EndsWithAny
    {
        [Fact]
        public void ReturnsTrue_WhenValueEndsWithOneOfTheSuffixes()
        {
            "foobar".EndsWithAny("bar", "baz").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenValueDoesNotEndWithAnySuffix()
        {
            "foobar".EndsWithAny("foo", "baz").ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            "FooBar".EndsWithAny("bar", "baz").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenCollectionIsEmpty()
        {
            "foobar".EndsWithAny().ShouldBeFalse();
        }

        [Fact]
        public void ReturnsTrue_WhenSuffixIsAnExactMatch()
        {
            "foo".EndsWithAny("foo", "bar").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenSuffixIsEmptyString()
        {
            "foobar".EndsWithAny(string.Empty, "baz").ShouldBeTrue();
        }
    }

    public class StartsWithAny_Enumerable
    {
        [Fact]
        public void ReturnsTrue_WhenValueStartsWithOneOfThePrefixes()
        {
            "foobar".StartsWithAny(new List<string> { "foo", "baz" }).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenValueDoesNotStartWithAnyPrefix()
        {
            "foobar".StartsWithAny(new List<string> { "baz", "qux" }).ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            "FooBar".StartsWithAny(new List<string> { "foo", "baz" }).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenCollectionIsEmpty()
        {
            "foobar".StartsWithAny(new List<string>()).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsTrue_WhenPrefixIsAnExactMatch()
        {
            "foo".StartsWithAny(new List<string> { "foo", "bar" }).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenPrefixIsEmptyString()
        {
            "foobar".StartsWithAny(new List<string> { string.Empty, "baz" }).ShouldBeTrue();
        }
    }

    public class StartsWithAny_Params
    {
        [Fact]
        public void ReturnsTrue_WhenValueStartsWithOneOfThePrefixes()
        {
            "foobar".StartsWithAny("foo", "baz").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenValueDoesNotStartWithAnyPrefix()
        {
            "foobar".StartsWithAny("baz", "qux").ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            "FooBar".StartsWithAny("foo", "baz").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenPrefixIsAnExactMatch()
        {
            "foo".StartsWithAny("foo", "baz").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenPrefixIsEmptyString()
        {
            "foobar".StartsWithAny(string.Empty, "baz").ShouldBeTrue();
        }
    }
}