using Grapevine;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests;

public class RequestCookieCollectionTests
{
    public class EmptyField
    {
        [Fact]
        public void HasCountOfZero()
        {
            RequestCookieCollection.Empty.Count.ShouldBe(0);
        }

        [Fact]
        public void IsSameInstanceOnEveryAccess()
        {
            RequestCookieCollection.Empty.ShouldBeSameAs(RequestCookieCollection.Empty);
        }
    }

    public class ParseMethod
    {
        [Fact]
        public void ReturnsEmpty_WhenHeaderIsNull()
        {
            RequestCookieCollection.Parse(null).ShouldBeSameAs(RequestCookieCollection.Empty);
        }

        [Fact]
        public void ReturnsEmpty_WhenHeaderIsEmpty()
        {
            RequestCookieCollection.Parse(string.Empty)
                .ShouldBeSameAs(RequestCookieCollection.Empty);
        }

        [Fact]
        public void ReturnsEmpty_WhenHeaderIsWhitespace()
        {
            RequestCookieCollection.Parse("   ")
                .ShouldBeSameAs(RequestCookieCollection.Empty);
        }

        [Fact]
        public void ReturnsEmpty_WhenHeaderHasNoValidPairs()
        {
            // No '=' character in any segment.
            RequestCookieCollection.Parse("noequalssign")
                .ShouldBeSameAs(RequestCookieCollection.Empty);
        }

        [Fact]
        public void ParsesSingleCookie()
        {
            var value = Guid.NewGuid().ToString();
            var c = RequestCookieCollection.Parse($"session={value}");
            c.Count.ShouldBe(1);
            c["session"].ShouldBe(value);
        }

        [Fact]
        public void ParsesMultipleCookies()
        {
            var v1 = Guid.NewGuid().ToString();
            var v2 = Guid.NewGuid().ToString();
            var c = RequestCookieCollection.Parse($"session={v1}; theme={v2}");
            c.Count.ShouldBe(2);
            c["session"].ShouldBe(v1);
            c["theme"].ShouldBe(v2);
        }

        [Fact]
        public void TrimsWhitespaceFromNames()
        {
            var value = Guid.NewGuid().ToString();
            var c = RequestCookieCollection.Parse($"  session  ={value}");
            c.ContainsKey("session").ShouldBeTrue();
        }

        [Fact]
        public void TrimsWhitespaceFromValues()
        {
            var value = Guid.NewGuid().ToString();
            var c = RequestCookieCollection.Parse($"session=  {value}  ");
            c["session"].ShouldBe(value);
        }

        [Fact]
        public void SkipsSegmentsWithNoEqualsSign()
        {
            var value = Guid.NewGuid().ToString();
            var c = RequestCookieCollection.Parse($"invalid; session={value}");
            c.Count.ShouldBe(1);
            c["session"].ShouldBe(value);
        }

        [Fact]
        public void SkipsSegmentsWithEmptyName()
        {
            var value = Guid.NewGuid().ToString();
            // A segment starting with '=' has an empty name after trimming.
            var c = RequestCookieCollection.Parse($"={value}; session={value}");
            c.Count.ShouldBe(1);
        }

        [Fact]
        public void SkipsEmptySegments()
        {
            var value = Guid.NewGuid().ToString();
            // Double semicolon produces an empty segment.
            var c = RequestCookieCollection.Parse($"session={value};;theme=dark");
            c.Count.ShouldBe(2);
        }

        [Fact]
        public void LastValueWins_WhenNameAppearsMultipleTimes()
        {
            var first = Guid.NewGuid().ToString();
            var last = Guid.NewGuid().ToString();
            var c = RequestCookieCollection.Parse($"session={first}; session={last}");
            c["session"].ShouldBe(last);
            c.Count.ShouldBe(1);
        }

        [Fact]
        public void NameLookupIsCaseInsensitive()
        {
            var value = Guid.NewGuid().ToString();
            var c = RequestCookieCollection.Parse($"Session={value}");
            c["session"].ShouldBe(value);
            c["SESSION"].ShouldBe(value);
        }

        [Fact]
        public void AllowsEqualsSignInValue()
        {
            // Base64-encoded values commonly contain '='. Only the first '='
            // should be treated as the name/value separator.
            var c = RequestCookieCollection.Parse("token=abc==def");
            c["token"].ShouldBe("abc==def");
        }

        [Fact]
        public void AllowsEmptyValue()
        {
            var c = RequestCookieCollection.Parse("session=");
            c.ContainsKey("session").ShouldBeTrue();
            c["session"].ShouldBe(string.Empty);
        }

        [Fact]
        public void ReturnsNewInstance_NotEmpty_WhenValidPairsPresent()
        {
            var c = RequestCookieCollection.Parse($"session={Guid.NewGuid()}");
            c.ShouldNotBeSameAs(RequestCookieCollection.Empty);
        }
    }

    public class Indexer
    {
        [Fact]
        public void ReturnsValue_WhenKeyExists()
        {
            var value = Guid.NewGuid().ToString();
            var c = RequestCookieCollection.Parse($"session={value}");
            c["session"].ShouldBe(value);
        }

        [Fact]
        public void Throws_WhenKeyDoesNotExist()
        {
            var c = RequestCookieCollection.Parse($"session={Guid.NewGuid()}");
            Should.Throw<KeyNotFoundException>(() => _ = c["missing"]);
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var value = Guid.NewGuid().ToString();
            var c = RequestCookieCollection.Parse($"Session={value}");
            c["session"].ShouldBe(value);
            c["SESSION"].ShouldBe(value);
        }
    }

    public class ContainsKeyMethod
    {
        [Fact]
        public void ReturnsTrue_WhenKeyExists()
        {
            var c = RequestCookieCollection.Parse($"session={Guid.NewGuid()}");
            c.ContainsKey("session").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenKeyDoesNotExist()
        {
            RequestCookieCollection.Empty.ContainsKey("session").ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var c = RequestCookieCollection.Parse($"Session={Guid.NewGuid()}");
            c.ContainsKey("session").ShouldBeTrue();
            c.ContainsKey("SESSION").ShouldBeTrue();
        }
    }

    public class TryGetValueMethod
    {
        [Fact]
        public void ReturnsTrue_WhenKeyExists()
        {
            var c = RequestCookieCollection.Parse($"session={Guid.NewGuid()}");
            c.TryGetValue("session", out _).ShouldBeTrue();
        }

        [Fact]
        public void SetsValue_WhenKeyExists()
        {
            var expected = Guid.NewGuid().ToString();
            var c = RequestCookieCollection.Parse($"session={expected}");
            c.TryGetValue("session", out var value);
            value.ShouldBe(expected);
        }

        [Fact]
        public void ReturnsFalse_WhenKeyDoesNotExist()
        {
            RequestCookieCollection.Empty.TryGetValue("session", out _).ShouldBeFalse();
        }

        [Fact]
        public void SetsValueToEmpty_WhenKeyDoesNotExist()
        {
            RequestCookieCollection.Empty.TryGetValue("session", out var value);
            value.ShouldBe(string.Empty);
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var expected = Guid.NewGuid().ToString();
            var c = RequestCookieCollection.Parse($"Session={expected}");
            c.TryGetValue("SESSION", out var value);
            value.ShouldBe(expected);
        }
    }

    public class KeysAndValuesProperties
    {
        [Fact]
        public void Keys_ContainsAllParsedNames()
        {
            var c = RequestCookieCollection.Parse(
                $"session={Guid.NewGuid()}; theme={Guid.NewGuid()}");
            c.Keys.ShouldContain("session");
            c.Keys.ShouldContain("theme");
        }

        [Fact]
        public void Values_ContainsAllParsedValues()
        {
            var v1 = Guid.NewGuid().ToString();
            var v2 = Guid.NewGuid().ToString();
            var c = RequestCookieCollection.Parse($"session={v1}; theme={v2}");
            c.Values.ShouldContain(v1);
            c.Values.ShouldContain(v2);
        }
    }

    public class GetEnumerator
    {
        [Fact]
        public void EnumeratesAllPairs()
        {
            var v1 = Guid.NewGuid().ToString();
            var v2 = Guid.NewGuid().ToString();
            var c = RequestCookieCollection.Parse($"session={v1}; theme={v2}");

            var pairs = new List<KeyValuePair<string, string>>();
            foreach (var pair in c)
                pairs.Add(pair);

            pairs.Count.ShouldBe(2);
        }

        [Fact]
        public void YieldsEmptySequence_WhenCollectionIsEmpty()
        {
            var count = 0;
            foreach (var _ in RequestCookieCollection.Empty)
                count++;
            count.ShouldBe(0);
        }
    }
}