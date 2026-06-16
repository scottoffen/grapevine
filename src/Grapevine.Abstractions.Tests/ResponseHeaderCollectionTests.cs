namespace Grapevine.Abstractions.Tests;

public class ResponseHeaderCollectionTests
{
    private static ResponseHeaderCollection Create() => new();

    private static ResponseHeaderCollection CreateSealed()
    {
        var c = new ResponseHeaderCollection();
        c.Seal();
        return c;
    }

    private static ResponseHeaderCollection CreateSealedWith(string name, string value)
    {
        var c = new ResponseHeaderCollection();
        c.Add(name, value);
        c.Seal();
        return c;
    }

    public class HeaderNameMapInvariant
    {
        [Fact]
        public void MapLengthMatchesEnumMemberCount()
        {
            var enumCount = Enum.GetValues(typeof(ResponseHeader)).Length;
            ResponseHeaderCollection.HeaderNameMap.Length.ShouldBe(enumCount);
        }

        [Fact]
        public void EachEnumMemberMapsToCorrectHeaderName()
        {
            // Spot-check first, last, and a representative middle set.
            ResponseHeaderCollection.HeaderNameMap[(int)ResponseHeader.AcceptRanges]
                .ShouldBe(HeaderNames.AcceptRanges);
            ResponseHeaderCollection.HeaderNameMap[(int)ResponseHeader.ContentType]
                .ShouldBe(HeaderNames.ContentType);
            ResponseHeaderCollection.HeaderNameMap[(int)ResponseHeader.Location]
                .ShouldBe(HeaderNames.Location);
            ResponseHeaderCollection.HeaderNameMap[(int)ResponseHeader.Server]
                .ShouldBe(HeaderNames.Server);
            ResponseHeaderCollection.HeaderNameMap[(int)ResponseHeader.WWWAuthenticate]
                .ShouldBe(HeaderNames.WWWAuthenticate);
        }
    }

    public class EnumKeyedIndexer
    {
        [Fact]
        public void ReturnsValue_WhenHeaderIsPresent()
        {
            var c = Create();
            c.Add(ResponseHeader.Server, Guid.NewGuid().ToString());
            c[ResponseHeader.Server].ShouldNotBeNull();
        }

        [Fact]
        public void ReturnsNull_WhenHeaderIsAbsent()
        {
            Create()[ResponseHeader.Server].ShouldBeNull();
        }

        [Fact]
        public void ReturnsFirstValue_WhenMultipleValuesPresent()
        {
            var first = Guid.NewGuid().ToString();
            var c = Create();
            c.Add(ResponseHeader.Via, first);
            c.Add(ResponseHeader.Via, Guid.NewGuid().ToString());
            c[ResponseHeader.Via].ShouldBe(first);
        }
    }

    public class EnumKeyedAddMethod
    {
        [Fact]
        public void AddsValueUnderCorrectHeaderName()
        {
            var value = Guid.NewGuid().ToString();
            var c = Create();
            c.Add(ResponseHeader.Server, value);
            c[HeaderNames.Server].ShouldBe(value);
        }

        [Fact]
        public void AccumulatesMultipleValues()
        {
            var c = Create();
            c.Add(ResponseHeader.Via, Guid.NewGuid().ToString());
            c.Add(ResponseHeader.Via, Guid.NewGuid().ToString());
            c.TryGetValues(ResponseHeader.Via, out var values).ShouldBeTrue();
            values!.Count.ShouldBe(2);
        }

        [Fact]
        public void Throws_WhenSealed()
        {
            Should.Throw<InvalidOperationException>(() =>
                CreateSealed().Add(ResponseHeader.Server, Guid.NewGuid().ToString()));
        }
    }

    public class EnumKeyedSetMethod
    {
        [Fact]
        public void SetsValueUnderCorrectHeaderName()
        {
            var value = Guid.NewGuid().ToString();
            var c = Create();
            c.Set(ResponseHeader.Server, value);
            c[HeaderNames.Server].ShouldBe(value);
        }

        [Fact]
        public void ReplacesExistingValues()
        {
            var replacement = Guid.NewGuid().ToString();
            var c = Create();
            c.Add(ResponseHeader.Via, Guid.NewGuid().ToString());
            c.Add(ResponseHeader.Via, Guid.NewGuid().ToString());
            c.Set(ResponseHeader.Via, replacement);
            c.TryGetValues(ResponseHeader.Via, out var values).ShouldBeTrue();
            values!.Count.ShouldBe(1);
            values[0].ShouldBe(replacement);
        }

        [Fact]
        public void Throws_WhenSealed()
        {
            Should.Throw<InvalidOperationException>(() =>
                CreateSealed().Set(ResponseHeader.Server, Guid.NewGuid().ToString()));
        }
    }

    public class EnumKeyedRemoveMethod
    {
        [Fact]
        public void ReturnsTrue_WhenHeaderExists()
        {
            var c = Create();
            c.Add(ResponseHeader.Server, Guid.NewGuid().ToString());
            c.Remove(ResponseHeader.Server).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenHeaderDoesNotExist()
        {
            Create().Remove(ResponseHeader.Server).ShouldBeFalse();
        }

        [Fact]
        public void RemovesHeaderFromCollection()
        {
            var c = Create();
            c.Add(ResponseHeader.Server, Guid.NewGuid().ToString());
            c.Remove(ResponseHeader.Server);
            c.Contains(ResponseHeader.Server).ShouldBeFalse();
        }

        [Fact]
        public void Throws_WhenSealed()
        {
            Should.Throw<InvalidOperationException>(() =>
                CreateSealed().Remove(ResponseHeader.Server));
        }
    }

    public class EnumKeyedContainsMethod
    {
        [Fact]
        public void ReturnsTrue_WhenHeaderExists()
        {
            var c = Create();
            c.Add(ResponseHeader.Server, Guid.NewGuid().ToString());
            c.Contains(ResponseHeader.Server).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenHeaderDoesNotExist()
        {
            Create().Contains(ResponseHeader.Server).ShouldBeFalse();
        }
    }

    public class EnumKeyedTryGetValueMethod
    {
        [Fact]
        public void ReturnsTrue_WhenHeaderExists()
        {
            var c = Create();
            c.Add(ResponseHeader.Server, Guid.NewGuid().ToString());
            c.TryGetValue(ResponseHeader.Server, out _).ShouldBeTrue();
        }

        [Fact]
        public void SetsValue_WhenHeaderExists()
        {
            var expected = Guid.NewGuid().ToString();
            var c = Create();
            c.Add(ResponseHeader.Server, expected);
            c.TryGetValue(ResponseHeader.Server, out var value);
            value.ShouldBe(expected);
        }

        [Fact]
        public void ReturnsFalse_WhenHeaderDoesNotExist()
        {
            Create().TryGetValue(ResponseHeader.Server, out _).ShouldBeFalse();
        }

        [Fact]
        public void SetsValueToNull_WhenHeaderDoesNotExist()
        {
            Create().TryGetValue(ResponseHeader.Server, out var value);
            value.ShouldBeNull();
        }
    }

    public class EnumKeyedTryGetValuesMethod
    {
        [Fact]
        public void ReturnsTrue_WhenHeaderExists()
        {
            var c = Create();
            c.Add(ResponseHeader.Via, Guid.NewGuid().ToString());
            c.TryGetValues(ResponseHeader.Via, out _).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsAllValues()
        {
            var c = Create();
            c.Add(ResponseHeader.Via, Guid.NewGuid().ToString());
            c.Add(ResponseHeader.Via, Guid.NewGuid().ToString());
            c.TryGetValues(ResponseHeader.Via, out var values).ShouldBeTrue();
            values!.Count.ShouldBe(2);
        }

        [Fact]
        public void ReturnsFalse_WhenHeaderDoesNotExist()
        {
            Create().TryGetValues(ResponseHeader.Via, out _).ShouldBeFalse();
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
            c.Add(HeaderNames.ContentType, "application/json; charset=UTF-8");
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
        public void SetterStoresFormattedString()
        {
            var c = Create();
            c.ContentType = ContentType.Json;
            c[HeaderNames.ContentType].ShouldBe(ContentType.Json.ToString());
        }

        [Fact]
        public void SetterRemovesHeader_WhenValueIsNull()
        {
            var c = Create();
            c.Add(HeaderNames.ContentType, "text/html");
            c.ContentType = null;
            c.Contains(HeaderNames.ContentType).ShouldBeFalse();
        }

        [Fact]
        public void SetterThrows_WhenSealed()
        {
            Should.Throw<InvalidOperationException>(() =>
                CreateSealed().ContentType = ContentType.Json);
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

        [Fact]
        public void CacheReturnsNull_AfterSealingWithNoHeader()
        {
            CreateSealed().ContentType.ShouldBeNull();
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
            c.Add(HeaderNames.ContentLength, "2048");
            c.ContentLength.ShouldBe(2048L);
        }

        [Fact]
        public void ReturnsNull_WhenValueIsNotANumber()
        {
            var c = Create();
            c.Add(HeaderNames.ContentLength, "not-a-number");
            c.ContentLength.ShouldBeNull();
        }

        [Fact]
        public void SetterStoresStringRepresentation()
        {
            var c = Create();
            c.ContentLength = 4096L;
            c[HeaderNames.ContentLength].ShouldBe("4096");
        }

        [Fact]
        public void SetterRemovesHeader_WhenValueIsNull()
        {
            var c = Create();
            c.Add(HeaderNames.ContentLength, "1024");
            c.ContentLength = null;
            c.Contains(HeaderNames.ContentLength).ShouldBeFalse();
        }

        [Fact]
        public void SetterThrows_WhenSealed()
        {
            Should.Throw<InvalidOperationException>(() =>
                CreateSealed().ContentLength = 512L);
        }

        [Fact]
        public void ReturnsSameValue_AfterSealing()
        {
            var c = CreateSealedWith(HeaderNames.ContentLength, "1024");
            c.ContentLength.ShouldBe(c.ContentLength);
            c.ContentLength.ShouldBe(1024L);
        }

        [Fact]
        public void CachesNullValue_AfterSealingWithNoHeader()
        {
            var c = CreateSealed();
            c.ContentLength.ShouldBeNull();
            c.ContentLength.ShouldBeNull();
        }
    }

    public class CookiesProperty
    {
        [Fact]
        public void IsNotNull_WhenNew()
        {
            Create().Cookies.ShouldNotBeNull();
        }

        [Fact]
        public void ReturnsSameInstance_OnEveryAccess()
        {
            var c = Create();
            c.Cookies.ShouldBeSameAs(c.Cookies);
        }

        [Fact]
        public void AcceptsCookies_BeforeSealing()
        {
            var c = Create();
            c.Cookies.Add(new Cookie("session", Guid.NewGuid().ToString("N")));
            c.Cookies.Count.ShouldBe(1);
        }

        [Fact]
        public void CookiesAreSealed_WhenHeadersAreSealed()
        {
            var c = Create();
            c.Seal();
            c.Cookies.IsSealed.ShouldBeTrue();
        }

        [Fact]
        public void CookiesThrowOnMutation_AfterHeadersAreSealed()
        {
            var c = CreateSealed();
            Should.Throw<InvalidOperationException>(() =>
                c.Cookies.Add(new Cookie("session", Guid.NewGuid().ToString("N"))));
        }

        [Fact]
        public void CookieSealIsIdempotent_WhenHeadersSealedMoreThanOnce()
        {
            var c = Create();
            c.Seal();
            c.Seal();
            c.Cookies.IsSealed.ShouldBeTrue();
        }
    }

    public class StringConvenienceProperties
    {
        // Each response string property has both a getter and a setter.
        // The theory covers: getter returns null when absent, getter returns
        // value when present, setter stores the value, setter with null removes
        // the header, and setter throws when sealed.

        public static IEnumerable<object[]> PropertyTestCases()
        {
            yield return new object[] { "AcceptRanges",           HeaderNames.AcceptRanges };
            yield return new object[] { "Age",                    HeaderNames.Age };
            yield return new object[] { "Allow",                  HeaderNames.Allow };
            yield return new object[] { "AltSvc",                 HeaderNames.AltSvc };
            yield return new object[] { "CacheControl",           HeaderNames.CacheControl };
            yield return new object[] { "Connection",             HeaderNames.Connection };
            yield return new object[] { "ContentDisposition",     HeaderNames.ContentDisposition };
            yield return new object[] { "ContentEncoding",        HeaderNames.ContentEncoding };
            yield return new object[] { "ContentLanguage",        HeaderNames.ContentLanguage };
            yield return new object[] { "ContentLocation",        HeaderNames.ContentLocation };
            yield return new object[] { "ContentRange",           HeaderNames.ContentRange };
            yield return new object[] { "Date",                   HeaderNames.Date };
            yield return new object[] { "ETag",                   HeaderNames.ETag };
            yield return new object[] { "Expires",                HeaderNames.Expires };
            yield return new object[] { "HTTP2Settings",          HeaderNames.HTTP2Settings };
            yield return new object[] { "LastModified",           HeaderNames.LastModified };
            yield return new object[] { "Link",                   HeaderNames.Link };
            yield return new object[] { "Location",               HeaderNames.Location };
            yield return new object[] { "Pragma",                 HeaderNames.Pragma };
            yield return new object[] { "ProxyAuthenticate",      HeaderNames.ProxyAuthenticate };
            yield return new object[] { "RetryAfter",             HeaderNames.RetryAfter };
            yield return new object[] { "Server",                 HeaderNames.Server };
            yield return new object[] { "SetCookie",              HeaderNames.SetCookie };
            yield return new object[] { "StrictTransportSecurity",HeaderNames.StrictTransportSecurity };
            yield return new object[] { "TransferEncoding",       HeaderNames.TransferEncoding };
            yield return new object[] { "Upgrade",                HeaderNames.Upgrade };
            yield return new object[] { "Vary",                   HeaderNames.Vary };
            yield return new object[] { "Via",                    HeaderNames.Via };
            yield return new object[] { "WWWAuthenticate",        HeaderNames.WWWAuthenticate };
        }

        [Theory]
        [MemberData(nameof(PropertyTestCases))]
        public void ReturnsNull_WhenHeaderIsAbsent(string propertyName, string _)
        {
            var prop = typeof(ResponseHeaderCollection).GetProperty(propertyName);
            prop.ShouldNotBeNull($"Property '{propertyName}' not found on ResponseHeaderCollection");
            prop!.GetValue(Create()).ShouldBeNull();
        }

        [Theory]
        [MemberData(nameof(PropertyTestCases))]
        public void ReturnsValue_WhenHeaderIsPresent(string propertyName, string headerName)
        {
            var expected = Guid.NewGuid().ToString();
            var c = Create();
            c.Add(headerName, expected);

            var prop = typeof(ResponseHeaderCollection).GetProperty(propertyName);
            prop.ShouldNotBeNull($"Property '{propertyName}' not found on ResponseHeaderCollection");
            prop!.GetValue(c).ShouldBe(expected);
        }

        [Theory]
        [MemberData(nameof(PropertyTestCases))]
        public void SetterStoresValue(string propertyName, string headerName)
        {
            var expected = Guid.NewGuid().ToString();
            var c = Create();

            var prop = typeof(ResponseHeaderCollection).GetProperty(propertyName);
            prop.ShouldNotBeNull($"Property '{propertyName}' not found on ResponseHeaderCollection");
            prop!.SetValue(c, expected);

            c[headerName].ShouldBe(expected);
        }

        [Theory]
        [MemberData(nameof(PropertyTestCases))]
        public void SetterRemovesHeader_WhenValueIsNull(string propertyName, string headerName)
        {
            var c = Create();
            c.Add(headerName, Guid.NewGuid().ToString());

            var prop = typeof(ResponseHeaderCollection).GetProperty(propertyName);
            prop.ShouldNotBeNull($"Property '{propertyName}' not found on ResponseHeaderCollection");
            prop!.SetValue(c, null);

            c.Contains(headerName).ShouldBeFalse();
        }

        [Theory]
        [MemberData(nameof(PropertyTestCases))]
        public void SetterThrows_WhenSealed(string propertyName, string _)
        {
            var c = CreateSealed();
            var prop = typeof(ResponseHeaderCollection).GetProperty(propertyName);
            prop.ShouldNotBeNull($"Property '{propertyName}' not found on ResponseHeaderCollection");

            // Reflection wraps the InvalidOperationException in a TargetInvocationException.
            var ex = Should.Throw<System.Reflection.TargetInvocationException>(() =>
                prop!.SetValue(c, Guid.NewGuid().ToString()));
            ex.InnerException.ShouldBeOfType<InvalidOperationException>();
            ex.InnerException!.Message.ShouldBe(HeaderCollection.SealedCollectionMessage);
        }
    }
}