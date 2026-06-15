using Grapevine;
using Grapevine.Abstractions;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests;

public class QueryParamsTests
{
    public class EmptyField
    {
        [Fact]
        public void HasCountOfZero()
        {
            QueryParams.Empty.Count.ShouldBe(0);
        }

        [Fact]
        public void IsSealed()
        {
            QueryParams.Empty.IsSealed.ShouldBeTrue();
        }

        [Fact]
        public void IsSameInstanceOnEveryAccess()
        {
            QueryParams.Empty.ShouldBeSameAs(QueryParams.Empty);
        }

        [Fact]
        public void ToStringReturnsEmptyString()
        {
            QueryParams.Empty.ToString().ShouldBe(string.Empty);
        }
    }

    public class IsSealedProperty
    {
        [Fact]
        public void AlwaysReturnsTrue()
        {
            QueryParams.Parse($"key={Guid.NewGuid()}").IsSealed.ShouldBeTrue();
        }
    }

    public class ParseMethod
    {
        [Fact]
        public void ReturnsEmpty_WhenInputIsNull()
        {
            QueryParams.Parse(null).ShouldBeSameAs(QueryParams.Empty);
        }

        [Fact]
        public void ReturnsEmpty_WhenInputIsEmpty()
        {
            QueryParams.Parse(string.Empty).ShouldBeSameAs(QueryParams.Empty);
        }

        [Fact]
        public void ReturnsEmpty_WhenInputIsQuestionMarkOnly()
        {
            QueryParams.Parse("?").ShouldBeSameAs(QueryParams.Empty);
        }

        [Fact]
        public void ReturnsEmpty_WhenNoValidPairsFound()
        {
            // A value-only segment with no name produces no entries.
            QueryParams.Parse("=value").ShouldBeSameAs(QueryParams.Empty);
        }

        [Fact]
        public void StripLeadingQuestionMark()
        {
            var value = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"?key={value}");
            c["key"].ShouldBe(value);
        }

        [Fact]
        public void ParsesSinglePair()
        {
            var value = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"key={value}");
            c.Count.ShouldBe(1);
            c["key"].ShouldBe(value);
        }

        [Fact]
        public void ParsesMultiplePairs()
        {
            var v1 = Guid.NewGuid().ToString();
            var v2 = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"a={v1}&b={v2}");
            c.Count.ShouldBe(2);
            c["a"].ShouldBe(v1);
            c["b"].ShouldBe(v2);
        }

        [Fact]
        public void KeyLookupIsCaseInsensitive()
        {
            var value = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"Key={value}");
            c["key"].ShouldBe(value);
            c["KEY"].ShouldBe(value);
        }

        [Fact]
        public void CollectsMultipleValuesForSameKey()
        {
            var v1 = Guid.NewGuid().ToString();
            var v2 = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"tag={v1}&tag={v2}");
            c.Count.ShouldBe(1);
            var values = c.GetValues("tag");
            values.Count.ShouldBe(2);
            values[0].ShouldBe(v1);
            values[1].ShouldBe(v2);
        }

        [Fact]
        public void CollectsMultipleValuesForSameKey_CaseInsensitive()
        {
            var v1 = Guid.NewGuid().ToString();
            var v2 = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"tag={v1}&TAG={v2}");
            c.Count.ShouldBe(1);
            c.GetValues("tag").Count.ShouldBe(2);
        }

        [Fact]
        public void PercentDecodesKeys()
        {
            var value = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"my%20key={value}");
            c["my key"].ShouldBe(value);
        }

        [Fact]
        public void PercentDecodesValues()
        {
            var c = QueryParams.Parse("key=hello%20world");
            c["key"].ShouldBe("hello world");
        }

        [Fact]
        public void AllowsEqualsSignInValue()
        {
            // Only the first '=' is the name/value separator.
            var c = QueryParams.Parse("token=abc==def");
            c["token"].ShouldBe("abc==def");
        }

        [Fact]
        public void AllowsEmptyValue()
        {
            var c = QueryParams.Parse("key=");
            c.ContainsKey("key").ShouldBeTrue();
            c.GetValues("key").Count.ShouldBe(1);
            c.GetValues("key")[0].ShouldBe(string.Empty);
        }

        [Fact]
        public void SkipsParameterWithNoName()
        {
            var value = Guid.NewGuid().ToString();
            // Leading '&' produces an empty-name segment that should be skipped.
            var c = QueryParams.Parse($"&key={value}");
            c.Count.ShouldBe(1);
            c["key"].ShouldBe(value);
        }

        [Fact]
        public void SkipsParameterWithWhitespaceName()
        {
            var value = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"%20={value}&key={value}");
            c.Count.ShouldBe(1);
        }

        [Fact]
        public void HandlesParameterWithNoEqualsSign()
        {
            // A flag-style parameter with no '=' creates a key entry with no values.
            var value = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"flag&key={value}");
            c.ContainsKey("flag").ShouldBeTrue();
            c.GetValues("flag").Count.ShouldBe(0);
        }

        [Fact]
        public void PreservesRawString_WithoutLeadingQuestionMark()
        {
            var raw = $"a=1&b=2";
            var c = QueryParams.Parse(raw);
            c.ToString().ShouldBe(raw);
        }

        [Fact]
        public void PreservesRawString_StrippingLeadingQuestionMark()
        {
            var c = QueryParams.Parse("?a=1&b=2");
            c.ToString().ShouldBe("a=1&b=2");
        }

        [Fact]
        public void ReturnsNewInstance_NotEmpty_WhenValidPairsPresent()
        {
            QueryParams.Parse($"key={Guid.NewGuid()}")
                .ShouldNotBeSameAs(QueryParams.Empty);
        }
    }

    public class Indexer
    {
        [Fact]
        public void ReturnsFirstValue_WhenKeyExists()
        {
            var value = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"key={value}");
            c["key"].ShouldBe(value);
        }

        [Fact]
        public void ReturnsFirstValue_WhenMultipleValuesExist()
        {
            var first = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"tag={first}&tag={Guid.NewGuid()}");
            c["tag"].ShouldBe(first);
        }

        [Fact]
        public void ReturnsNull_WhenKeyDoesNotExist()
        {
            QueryParams.Empty["missing"].ShouldBeNull();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var value = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"Key={value}");
            c["key"].ShouldBe(value);
            c["KEY"].ShouldBe(value);
        }
    }

    public class ContainsKeyMethod
    {
        [Fact]
        public void ReturnsTrue_WhenKeyExists()
        {
            var c = QueryParams.Parse($"key={Guid.NewGuid()}");
            c.ContainsKey("key").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenKeyDoesNotExist()
        {
            QueryParams.Empty.ContainsKey("key").ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var c = QueryParams.Parse($"Key={Guid.NewGuid()}");
            c.ContainsKey("key").ShouldBeTrue();
            c.ContainsKey("KEY").ShouldBeTrue();
        }
    }

    public class TryGetValueMethod
    {
        [Fact]
        public void ReturnsTrue_WhenKeyExists()
        {
            var c = QueryParams.Parse($"key={Guid.NewGuid()}");
            c.TryGetValue("key", out _).ShouldBeTrue();
        }

        [Fact]
        public void SetsValue_WhenKeyExists()
        {
            var expected = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"key={expected}");
            c.TryGetValue("key", out var value);
            value.ShouldBe(expected);
        }

        [Fact]
        public void ReturnsFirstValue_WhenMultipleValuesExist()
        {
            var first = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"tag={first}&tag={Guid.NewGuid()}");
            c.TryGetValue("tag", out var value);
            value.ShouldBe(first);
        }

        [Fact]
        public void ReturnsFalse_WhenKeyDoesNotExist()
        {
            QueryParams.Empty.TryGetValue("key", out _).ShouldBeFalse();
        }

        [Fact]
        public void SetsValueToNull_WhenKeyDoesNotExist()
        {
            QueryParams.Empty.TryGetValue("key", out var value);
            value.ShouldBeNull();
        }

        [Fact]
        public void ReturnsFalse_WhenKeyExistsWithNoValues()
        {
            // Flag-style parameter: key present but no '=' means no values.
            var c = QueryParams.Parse("flag");
            c.TryGetValue("flag", out _).ShouldBeFalse();
        }
    }

    public class GetValuesMethod
    {
        [Fact]
        public void ReturnsAllValues_WhenMultipleExist()
        {
            var v1 = Guid.NewGuid().ToString();
            var v2 = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"tag={v1}&tag={v2}");
            var values = c.GetValues("tag");
            values.Count.ShouldBe(2);
            values[0].ShouldBe(v1);
            values[1].ShouldBe(v2);
        }

        [Fact]
        public void ReturnsEmptyList_WhenKeyDoesNotExist()
        {
            QueryParams.Empty.GetValues("missing").ShouldBeEmpty();
        }

        [Fact]
        public void ReturnsEmptyList_WhenKeyExistsWithNoValues()
        {
            var c = QueryParams.Parse("flag");
            c.GetValues("flag").ShouldBeEmpty();
        }
    }

    public class TryGetValuesMethod
    {
        [Fact]
        public void ReturnsTrue_WhenKeyExists()
        {
            var c = QueryParams.Parse($"tag={Guid.NewGuid()}");
            c.TryGetValues("tag", out _).ShouldBeTrue();
        }

        [Fact]
        public void SetsValues_WhenKeyExists()
        {
            var v1 = Guid.NewGuid().ToString();
            var v2 = Guid.NewGuid().ToString();
            var c = QueryParams.Parse($"tag={v1}&tag={v2}");
            c.TryGetValues("tag", out var values).ShouldBeTrue();
            values!.Count.ShouldBe(2);
        }

        [Fact]
        public void ReturnsFalse_WhenKeyDoesNotExist()
        {
            QueryParams.Empty.TryGetValues("missing", out _).ShouldBeFalse();
        }

        [Fact]
        public void SetsValuesToNull_WhenKeyDoesNotExist()
        {
            QueryParams.Empty.TryGetValues("missing", out var values);
            values.ShouldBeNull();
        }
    }

    public class ToStringMethod
    {
        [Fact]
        public void ReturnsOriginalRawString()
        {
            var raw = $"a={Guid.NewGuid()}&b={Guid.NewGuid()}";
            QueryParams.Parse(raw).ToString().ShouldBe(raw);
        }

        [Fact]
        public void OmitsLeadingQuestionMark()
        {
            QueryParams.Parse("?a=1").ToString().ShouldBe("a=1");
        }

        [Fact]
        public void ReturnsEmptyString_ForEmptyCollection()
        {
            QueryParams.Empty.ToString().ShouldBe(string.Empty);
        }

        [Fact]
        public void PreservesPercentEncoding()
        {
            // The raw string is stored as-is; encoding is not normalised.
            var raw = "key=hello%20world";
            QueryParams.Parse(raw).ToString().ShouldBe(raw);
        }
    }

    public class GetEnumerator
    {
        [Fact]
        public void EnumeratesAllPairs()
        {
            var c = QueryParams.Parse($"a={Guid.NewGuid()}&b={Guid.NewGuid()}");
            var count = 0;
            foreach (var _ in c) count++;
            count.ShouldBe(2);
        }

        [Fact]
        public void YieldsEmptySequence_WhenCollectionIsEmpty()
        {
            var count = 0;
            foreach (var _ in QueryParams.Empty) count++;
            count.ShouldBe(0);
        }

        [Fact]
        public void EachPairValueIsReadOnlyList()
        {
            var c = QueryParams.Parse($"tag={Guid.NewGuid()}&tag={Guid.NewGuid()}");
            foreach (var pair in c)
                pair.Value.ShouldBeAssignableTo<IReadOnlyList<string>>();
        }
    }
}