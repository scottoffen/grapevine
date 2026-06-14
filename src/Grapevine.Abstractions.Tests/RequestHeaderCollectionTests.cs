using Grapevine;
using Grapevine.Abstractions;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests;

public class RequestHeaderCollectionTests
{
    private static RequestHeaderCollection Create() => new();

    private static RequestHeaderCollection CreateSealed()
    {
        var c = new RequestHeaderCollection();
        c.Seal();
        return c;
    }

    private static RequestHeaderCollection CreateSealedWith(string name, string value)
    {
        var c = new RequestHeaderCollection();
        c.Add(name, value);
        c.Seal();
        return c;
    }

    public class HeaderNameMapInvariant
    {
        [Fact]
        public void MapLengthMatchesEnumMemberCount()
        {
            var enumCount = Enum.GetValues(typeof(RequestHeader)).Length;
            RequestHeaderCollection.HeaderNameMap.Length.ShouldBe(enumCount);
        }

        [Fact]
        public void EachEnumMemberMapsToCorrectHeaderName()
        {
            // Spot-check a representative set of enum members to confirm the
            // mapping array is in the correct order.
            RequestHeaderCollection.HeaderNameMap[(int)RequestHeader.Accept]
                .ShouldBe(HeaderNames.Accept);
            RequestHeaderCollection.HeaderNameMap[(int)RequestHeader.ContentType]
                .ShouldBe(HeaderNames.ContentType);
            RequestHeaderCollection.HeaderNameMap[(int)RequestHeader.UserAgent]
                .ShouldBe(HeaderNames.UserAgent);
            RequestHeaderCollection.HeaderNameMap[(int)RequestHeader.Host]
                .ShouldBe(HeaderNames.Host);
            RequestHeaderCollection.HeaderNameMap[(int)RequestHeader.Warning]
                .ShouldBe(HeaderNames.Warning);
        }
    }

    public class EnumKeyedIndexer
    {
        [Fact]
        public void ReturnsValue_WhenHeaderIsPresent()
        {
            var c = Create();
            c.Add(RequestHeader.Host, Guid.NewGuid().ToString());
            c[RequestHeader.Host].ShouldNotBeNull();
        }

        [Fact]
        public void ReturnsNull_WhenHeaderIsAbsent()
        {
            Create()[RequestHeader.Host].ShouldBeNull();
        }

        [Fact]
        public void ReturnsFirstValue_WhenMultipleValuesPresent()
        {
            var first = Guid.NewGuid().ToString();
            var c = Create();
            c.Add(RequestHeader.Via, first);
            c.Add(RequestHeader.Via, Guid.NewGuid().ToString());
            c[RequestHeader.Via].ShouldBe(first);
        }
    }

    public class EnumKeyedAddMethod
    {
        [Fact]
        public void AddsValueUnderCorrectHeaderName()
        {
            var value = Guid.NewGuid().ToString();
            var c = Create();
            c.Add(RequestHeader.Host, value);
            c[HeaderNames.Host].ShouldBe(value);
        }

        [Fact]
        public void AccumulatesMultipleValues()
        {
            var c = Create();
            c.Add(RequestHeader.Via, Guid.NewGuid().ToString());
            c.Add(RequestHeader.Via, Guid.NewGuid().ToString());
            c.TryGetValues(RequestHeader.Via, out var values).ShouldBeTrue();
            values!.Count.ShouldBe(2);
        }

        [Fact]
        public void Throws_WhenSealed()
        {
            Should.Throw<InvalidOperationException>(() =>
                CreateSealed().Add(RequestHeader.Host, Guid.NewGuid().ToString()));
        }
    }

    public class EnumKeyedSetMethod
    {
        [Fact]
        public void SetsValueUnderCorrectHeaderName()
        {
            var value = Guid.NewGuid().ToString();
            var c = Create();
            c.Set(RequestHeader.Host, value);
            c[HeaderNames.Host].ShouldBe(value);
        }

        [Fact]
        public void ReplacesExistingValues()
        {
            var replacement = Guid.NewGuid().ToString();
            var c = Create();
            c.Add(RequestHeader.Via, Guid.NewGuid().ToString());
            c.Add(RequestHeader.Via, Guid.NewGuid().ToString());
            c.Set(RequestHeader.Via, replacement);
            c.TryGetValues(RequestHeader.Via, out var values).ShouldBeTrue();
            values!.Count.ShouldBe(1);
            values[0].ShouldBe(replacement);
        }

        [Fact]
        public void Throws_WhenSealed()
        {
            Should.Throw<InvalidOperationException>(() =>
                CreateSealed().Set(RequestHeader.Host, Guid.NewGuid().ToString()));
        }
    }

    public class EnumKeyedRemoveMethod
    {
        [Fact]
        public void ReturnsTrue_WhenHeaderExists()
        {
            var c = Create();
            c.Add(RequestHeader.Host, Guid.NewGuid().ToString());
            c.Remove(RequestHeader.Host).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenHeaderDoesNotExist()
        {
            Create().Remove(RequestHeader.Host).ShouldBeFalse();
        }

        [Fact]
        public void RemovesHeaderFromCollection()
        {
            var c = Create();
            c.Add(RequestHeader.Host, Guid.NewGuid().ToString());
            c.Remove(RequestHeader.Host);
            c.Contains(RequestHeader.Host).ShouldBeFalse();
        }

        [Fact]
        public void Throws_WhenSealed()
        {
            Should.Throw<InvalidOperationException>(() =>
                CreateSealed().Remove(RequestHeader.Host));
        }
    }

    public class EnumKeyedContainsMethod
    {
        [Fact]
        public void ReturnsTrue_WhenHeaderExists()
        {
            var c = Create();
            c.Add(RequestHeader.Host, Guid.NewGuid().ToString());
            c.Contains(RequestHeader.Host).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenHeaderDoesNotExist()
        {
            Create().Contains(RequestHeader.Host).ShouldBeFalse();
        }
    }

    public class EnumKeyedTryGetValueMethod
    {
        [Fact]
        public void ReturnsTrue_WhenHeaderExists()
        {
            var c = Create();
            c.Add(RequestHeader.Host, Guid.NewGuid().ToString());
            c.TryGetValue(RequestHeader.Host, out _).ShouldBeTrue();
        }

        [Fact]
        public void SetsValue_WhenHeaderExists()
        {
            var expected = Guid.NewGuid().ToString();
            var c = Create();
            c.Add(RequestHeader.Host, expected);
            c.TryGetValue(RequestHeader.Host, out var value);
            value.ShouldBe(expected);
        }

        [Fact]
        public void ReturnsFalse_WhenHeaderDoesNotExist()
        {
            Create().TryGetValue(RequestHeader.Host, out _).ShouldBeFalse();
        }

        [Fact]
        public void SetsValueToNull_WhenHeaderDoesNotExist()
        {
            Create().TryGetValue(RequestHeader.Host, out var value);
            value.ShouldBeNull();
        }
    }

    public class EnumKeyedTryGetValuesMethod
    {
        [Fact]
        public void ReturnsTrue_WhenHeaderExists()
        {
            var c = Create();
            c.Add(RequestHeader.Via, Guid.NewGuid().ToString());
            c.TryGetValues(RequestHeader.Via, out _).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsAllValues()
        {
            var c = Create();
            c.Add(RequestHeader.Via, Guid.NewGuid().ToString());
            c.Add(RequestHeader.Via, Guid.NewGuid().ToString());
            c.TryGetValues(RequestHeader.Via, out var values).ShouldBeTrue();
            values!.Count.ShouldBe(2);
        }

        [Fact]
        public void ReturnsFalse_WhenHeaderDoesNotExist()
        {
            Create().TryGetValues(RequestHeader.Via, out _).ShouldBeFalse();
        }
    }

    public class AcceptProperty
    {
        [Fact]
        public void ReturnsNull_WhenHeaderAbsent()
        {
            Create().Accept.ShouldBeNull();
        }

        [Fact]
        public void ReturnsHeader_WhenPresent()
        {
            var c = Create();
            c.Add(HeaderNames.Accept, "text/html");
            c.Accept.ShouldNotBeNull();
            c.Accept!.Name.ShouldBe(HeaderNames.Accept);
        }

        [Fact]
        public void ParsesQualityValues()
        {
            var c = Create();
            c.Add(HeaderNames.Accept, "text/html;q=0.9, application/json");
            c.Accept!.Values.Count.ShouldBe(2);
            c.Accept.Values[0].Quality.ShouldBe(0.9);
            c.Accept.Values[1].Quality.ShouldBeNull();
        }

        [Fact]
        public void ReturnsNewInstance_BeforeSealing()
        {
            var c = Create();
            c.Add(HeaderNames.Accept, "text/html");
            var first = c.Accept;
            var second = c.Accept;
            first.ShouldNotBeSameAs(second);
        }

        [Fact]
        public void ReturnsCachedInstance_AfterSealing()
        {
            var c = CreateSealedWith(HeaderNames.Accept, "text/html");
            var first = c.Accept;
            var second = c.Accept;
            first.ShouldBeSameAs(second);
        }

        [Fact]
        public void CacheReturnsNull_AfterSealingWithNoHeader()
        {
            CreateSealed().Accept.ShouldBeNull();
        }
    }

    public class AcceptEncodingProperty
    {
        [Fact]
        public void ReturnsNull_WhenHeaderAbsent()
        {
            Create().AcceptEncoding.ShouldBeNull();
        }

        [Fact]
        public void ReturnsHeader_WhenPresent()
        {
            var c = Create();
            c.Add(HeaderNames.AcceptEncoding, "gzip, deflate");
            c.AcceptEncoding.ShouldNotBeNull();
        }

        [Fact]
        public void ReturnsCachedInstance_AfterSealing()
        {
            var c = CreateSealedWith(HeaderNames.AcceptEncoding, "gzip");
            c.AcceptEncoding.ShouldBeSameAs(c.AcceptEncoding);
        }
    }

    public class AcceptLanguageProperty
    {
        [Fact]
        public void ReturnsNull_WhenHeaderAbsent()
        {
            Create().AcceptLanguage.ShouldBeNull();
        }

        [Fact]
        public void ReturnsHeader_WhenPresent()
        {
            var c = Create();
            c.Add(HeaderNames.AcceptLanguage, "en-US, en;q=0.9");
            c.AcceptLanguage.ShouldNotBeNull();
        }

        [Fact]
        public void ReturnsCachedInstance_AfterSealing()
        {
            var c = CreateSealedWith(HeaderNames.AcceptLanguage, "en-US");
            c.AcceptLanguage.ShouldBeSameAs(c.AcceptLanguage);
        }
    }

    public class ContentTypeProperty
    {
        [Fact]
        public void ReturnsNull_WhenHeaderAbsent()
        {
            Create().ContentType.ShouldBeNull();
        }

        [Fact]
        public void ReturnsContentType_WhenPresent()
        {
            var c = Create();
            c.Add(HeaderNames.ContentType, "application/json");
            c.ContentType.ShouldNotBeNull();
            c.ContentType!.ShouldBe(ContentType.Json);
        }

        [Fact]
        public void ReturnsNull_WhenValueIsUnparseable()
        {
            var c = Create();
            c.Add(HeaderNames.ContentType, "   ");
            c.ContentType.ShouldBeNull();
        }

        [Fact]
        public void ReturnsNewInstance_BeforeSealing()
        {
            var c = Create();
            c.Add(HeaderNames.ContentType, "text/html");
            c.ContentType.ShouldNotBeSameAs(c.ContentType);
        }

        [Fact]
        public void ReturnsCachedInstance_AfterSealing()
        {
            var c = CreateSealedWith(HeaderNames.ContentType, "text/html");
            c.ContentType.ShouldBeSameAs(c.ContentType);
        }
    }

    public class ContentLengthProperty
    {
        [Fact]
        public void ReturnsNull_WhenHeaderAbsent()
        {
            Create().ContentLength.ShouldBeNull();
        }

        [Fact]
        public void ReturnsValue_WhenPresent()
        {
            var c = Create();
            c.Add(HeaderNames.ContentLength, "1024");
            c.ContentLength.ShouldBe(1024L);
        }

        [Fact]
        public void ReturnsNull_WhenValueIsNotANumber()
        {
            var c = Create();
            c.Add(HeaderNames.ContentLength, "not-a-number");
            c.ContentLength.ShouldBeNull();
        }

        [Fact]
        public void ReturnsSameValue_AfterSealing()
        {
            var c = CreateSealedWith(HeaderNames.ContentLength, "512");
            var first = c.ContentLength;
            var second = c.ContentLength;
            first.ShouldBe(second);
            first.ShouldBe(512L);
        }

        [Fact]
        public void CachesNullValue_AfterSealingWithNoHeader()
        {
            var c = CreateSealed();
            c.ContentLength.ShouldBeNull();
            // Access again to exercise the cached null path.
            c.ContentLength.ShouldBeNull();
        }
    }

    public class CookiesProperty
    {
        [Fact]
        public void ReturnsEmpty_WhenCookieHeaderAbsent()
        {
            Create().Cookies.ShouldBeSameAs(RequestCookieCollection.Empty);
        }

        [Fact]
        public void ParsesCookies_WhenHeaderPresent()
        {
            var value = Guid.NewGuid().ToString("N");
            var c = Create();
            c.Add(HeaderNames.Cookie, $"session={value}");
            c.Cookies["session"].ShouldBe(value);
        }

        [Fact]
        public void JoinsMultipleCookieHeaders_BeforeParsing()
        {
            // RFC 6265: multiple Cookie headers are joined with "; "
            var v1 = Guid.NewGuid().ToString("N");
            var v2 = Guid.NewGuid().ToString("N");
            var c = Create();
            c.Add(HeaderNames.Cookie, $"session={v1}");
            c.Add(HeaderNames.Cookie, $"theme={v2}");
            c.Cookies.Count.ShouldBe(2);
            c.Cookies["session"].ShouldBe(v1);
            c.Cookies["theme"].ShouldBe(v2);
        }

        [Fact]
        public void ReturnsNewInstance_BeforeSealing()
        {
            var c = Create();
            c.Add(HeaderNames.Cookie, $"session={Guid.NewGuid():N}");
            c.Cookies.ShouldNotBeSameAs(c.Cookies);
        }

        [Fact]
        public void ReturnsCachedInstance_AfterSealing()
        {
            var value = Guid.NewGuid().ToString("N");
            var c = new RequestHeaderCollection();
            c.Add(HeaderNames.Cookie, $"session={value}");
            c.Seal();
            c.Cookies.ShouldBeSameAs(c.Cookies);
        }

        [Fact]
        public void CachesEmpty_AfterSealingWithNoCookieHeader()
        {
            var c = CreateSealed();
            c.Cookies.ShouldBeSameAs(RequestCookieCollection.Empty);
            // Access again to exercise the cached path.
            c.Cookies.ShouldBeSameAs(RequestCookieCollection.Empty);
        }
    }

    public class StringConvenienceProperties
    {
        // Rather than twenty-six near-identical facts, a single theory covers the
        // pattern: each property reads from the correct header name and returns
        // null when the header is absent.

        public static IEnumerable<object[]> PropertyTestCases()
        {
            yield return new object[] { "AcceptCharset",     HeaderNames.AcceptCharset };
            yield return new object[] { "Authorization",     HeaderNames.Authorization };
            yield return new object[] { "CacheControl",      HeaderNames.CacheControl };
            yield return new object[] { "Connection",        HeaderNames.Connection };
            yield return new object[] { "ContentEncoding",   HeaderNames.ContentEncoding };
            yield return new object[] { "Cookie",            HeaderNames.Cookie };
            yield return new object[] { "Date",              HeaderNames.Date };
            yield return new object[] { "Expect",            HeaderNames.Expect };
            yield return new object[] { "Forwarded",         HeaderNames.Forwarded };
            yield return new object[] { "From",              HeaderNames.From };
            yield return new object[] { "Host",              HeaderNames.Host };
            yield return new object[] { "IfMatch",           HeaderNames.IfMatch };
            yield return new object[] { "IfModifiedSince",   HeaderNames.IfModifiedSince };
            yield return new object[] { "IfNoneMatch",       HeaderNames.IfNoneMatch };
            yield return new object[] { "IfRange",           HeaderNames.IfRange };
            yield return new object[] { "IfUnmodifiedSince", HeaderNames.IfUnmodifiedSince };
            yield return new object[] { "Origin",            HeaderNames.Origin };
            yield return new object[] { "Pragma",            HeaderNames.Pragma };
            yield return new object[] { "ProxyAuthorization",HeaderNames.ProxyAuthorization };
            yield return new object[] { "Range",             HeaderNames.Range };
            yield return new object[] { "Referer",           HeaderNames.Referer };
            yield return new object[] { "TE",                HeaderNames.TE };
            yield return new object[] { "Trailer",           HeaderNames.Trailer };
            yield return new object[] { "TransferEncoding",  HeaderNames.TransferEncoding };
            yield return new object[] { "Upgrade",           HeaderNames.Upgrade };
            yield return new object[] { "UserAgent",         HeaderNames.UserAgent };
            yield return new object[] { "Via",               HeaderNames.Via };
            yield return new object[] { "Warning",           HeaderNames.Warning };
        }

        [Theory]
        [MemberData(nameof(PropertyTestCases))]
        public void ReturnsNull_WhenHeaderIsAbsent(string propertyName, string _)
        {
            var c = Create();
            var prop = typeof(RequestHeaderCollection).GetProperty(propertyName);
            prop.ShouldNotBeNull($"Property '{propertyName}' not found on RequestHeaderCollection");
            prop!.GetValue(c).ShouldBeNull();
        }

        [Theory]
        [MemberData(nameof(PropertyTestCases))]
        public void ReturnsValue_WhenHeaderIsPresent(string propertyName, string headerName)
        {
            var expected = Guid.NewGuid().ToString();
            var c = Create();
            c.Add(headerName, expected);

            var prop = typeof(RequestHeaderCollection).GetProperty(propertyName);
            prop.ShouldNotBeNull($"Property '{propertyName}' not found on RequestHeaderCollection");
            prop!.GetValue(c).ShouldBe(expected);
        }
    }
}