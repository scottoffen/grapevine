using Grapevine;

namespace Grapevine.Abstractions.Tests;

public class ContentTypeTests
{
    private static string UniqueMimeType() => $"application/x-{Guid.NewGuid():N}";
    private static string UniqueExtension() => Guid.NewGuid().ToString("N");

    public class Constructor
    {
        [Fact]
        public void SetsValue()
        {
            var ct = new ContentType("text/html", ContentMode.Text, "UTF-8");
            ct.Value.ShouldBe("text/html");
        }

        [Fact]
        public void SetsMode()
        {
            var ct = new ContentType("text/html", ContentMode.Text, "UTF-8");
            ct.Mode.ShouldBe(ContentMode.Text);
        }

        [Fact]
        public void SetsCharSet()
        {
            var ct = new ContentType("text/html", ContentMode.Text, "UTF-8");
            ct.CharSet.ShouldBe("UTF-8");
        }

        [Fact]
        public void DefaultModeIsBinary()
        {
            var ct = new ContentType("application/octet-stream");
            ct.Mode.ShouldBe(ContentMode.Binary);
        }

        [Fact]
        public void DefaultCharSetIsEmpty()
        {
            var ct = new ContentType("application/octet-stream");
            ct.CharSet.ShouldBe(string.Empty);
        }

        [Fact]
        public void IsBinaryIsTrue_WhenModeIsBinary()
        {
            var ct = new ContentType("application/octet-stream", ContentMode.Binary);
            ct.IsBinary.ShouldBeTrue();
        }

        [Fact]
        public void IsBinaryIsFalse_WhenModeIsText()
        {
            var ct = new ContentType("text/plain", ContentMode.Text);
            ct.IsBinary.ShouldBeFalse();
        }

        [Fact]
        public void SetsBoundary_WhenProvidedAndMultipart()
        {
            var ct = new ContentType("multipart/form-data", ContentMode.Binary, "", "my-boundary");
            ct.Boundary.ShouldBe("my-boundary");
        }

        [Fact]
        public void GeneratesBoundary_WhenNotProvidedAndMultipart()
        {
            var ct = new ContentType("multipart/form-data", ContentMode.Binary);
            ct.Boundary.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ThrowsOnBoundaryAccess_WhenNotMultipart()
        {
            var ct = new ContentType("text/html", ContentMode.Text);
            Should.Throw<InvalidOperationException>(() => _ = ct.Boundary);
        }
    }

    public class ToStringMethod
    {
        [Fact]
        public void ReturnsValueOnly_WhenCharSetIsEmpty()
        {
            var ct = new ContentType("image/png", ContentMode.Binary);
            ct.ToString().ShouldBe("image/png");
        }

        [Fact]
        public void ReturnsValueWithCharSet_WhenCharSetIsPresent()
        {
            var ct = new ContentType("text/html", ContentMode.Text, "UTF-8");
            ct.ToString().ShouldBe("text/html; charset=UTF-8");
        }

        [Fact]
        public void ReturnsValueWithBoundary_WhenMultipart()
        {
            var ct = new ContentType("multipart/form-data", ContentMode.Binary, "", "test-boundary");
            ct.ToString().ShouldBe("multipart/form-data; boundary=test-boundary");
        }

        [Fact]
        public void GeneratesAndIncludesBoundary_WhenMultipartAndNoBoundaryProvided()
        {
            var ct = new ContentType("multipart/form-data", ContentMode.Binary);
            ct.ToString().ShouldStartWith("multipart/form-data; boundary=");
        }
    }

    public class ImplicitStringConversion
    {
        [Fact]
        public void ReturnsValueOnly_WhenCharSetIsEmpty()
        {
            string result = ContentType.Png;
            result.ShouldBe("image/png");
        }

        [Fact]
        public void ReturnsValueWithCharSet_WhenCharSetIsPresent()
        {
            string result = ContentType.Html;
            result.ShouldBe("text/html; charset=UTF-8");
        }

        [Fact]
        public void ReturnsValueWithBoundary_WhenMultipart()
        {
            var ct = new ContentType("multipart/form-data", ContentMode.Binary, "", "test-boundary");
            string result = ct;
            result.ShouldBe("multipart/form-data; boundary=test-boundary");
        }

        [Fact]
        public void MatchesToString()
        {
            string implicitResult = ContentType.Json;
            implicitResult.ShouldBe(ContentType.Json.ToString());
        }
    }

    public class IsMultipartProperty
    {
        [Fact]
        public void ReturnsTrue_ForMultipartType()
        {
            var ct = new ContentType("multipart/form-data", ContentMode.Binary);
            ct.IsMultipart.ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_ForNonMultipartType()
        {
            var ct = new ContentType("text/html", ContentMode.Text);
            ct.IsMultipart.ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var ct = new ContentType("MULTIPART/FORM-DATA", ContentMode.Binary);
            ct.IsMultipart.ShouldBeTrue();
        }
    }

    public class EqualityOperators
    {
        public class ContentTypeVsContentType
        {
            [Fact]
            public void ReturnsTrue_WhenValuesAreEqual()
            {
                var a = new ContentType("text/html", ContentMode.Text, "UTF-8");
                var b = new ContentType("text/html", ContentMode.Binary);
                (a == b).ShouldBeTrue();
            }

            [Fact]
            public void ReturnsTrue_WhenValuesAreEqualWithDifferentCase()
            {
                var a = new ContentType("text/html", ContentMode.Text);
                var b = new ContentType("TEXT/HTML", ContentMode.Text);
                (a == b).ShouldBeTrue();
            }

            [Fact]
            public void ReturnsFalse_WhenValuesAreDifferent()
            {
                var a = new ContentType("text/html", ContentMode.Text);
                var b = new ContentType("text/plain", ContentMode.Text);
                (a == b).ShouldBeFalse();
            }

            [Fact]
            public void InequalityReturnsTrue_WhenValuesAreDifferent()
            {
                var a = new ContentType("text/html", ContentMode.Text);
                var b = new ContentType("text/plain", ContentMode.Text);
                (a != b).ShouldBeTrue();
            }
        }

        public class ContentTypeVsString
        {
            [Fact]
            public void ReturnsTrue_WhenStringMatchesBareValue()
            {
                (ContentType.Html == "text/html").ShouldBeTrue();
            }

            [Fact]
            public void ReturnsTrue_WhenStringIncludesCharset()
            {
                (ContentType.Html == "text/html; charset=utf-8").ShouldBeTrue();
            }

            [Fact]
            public void ReturnsFalse_WhenStringDoesNotMatch()
            {
                (ContentType.Html == "text/plain").ShouldBeFalse();
            }

            [Fact]
            public void InequalityReturnsTrue_WhenStringDoesNotMatch()
            {
                (ContentType.Html != "text/plain").ShouldBeTrue();
            }

            [Fact]
            public void IsCaseInsensitive()
            {
                (ContentType.Html == "TEXT/HTML").ShouldBeTrue();
            }

            [Fact]
            public void ReturnsTrue_WhenStringIncludesBoundary()
            {
                (ContentType.MultipartFormData == "multipart/form-data; boundary=abc123").ShouldBeTrue();
            }
        }

        public class StringVsContentType
        {
            [Fact]
            public void ReturnsTrue_WhenStringMatchesBareValue()
            {
                ("text/html" == ContentType.Html).ShouldBeTrue();
            }

            [Fact]
            public void ReturnsTrue_WhenStringIncludesCharset()
            {
                ("text/html; charset=utf-8" == ContentType.Html).ShouldBeTrue();
            }

            [Fact]
            public void ReturnsFalse_WhenStringDoesNotMatch()
            {
                ("text/plain" == ContentType.Html).ShouldBeFalse();
            }

            [Fact]
            public void InequalityReturnsTrue_WhenStringDoesNotMatch()
            {
                ("text/plain" != ContentType.Html).ShouldBeTrue();
            }
        }

        public class EqualsMethod
        {
            [Fact]
            public void ReturnsFalse_WhenObjectIsNull()
            {
                ContentType.Html.Equals(null).ShouldBeFalse();
            }

            [Fact]
            public void ReturnsFalse_WhenObjectIsUnrelatedType()
            {
                ContentType.Html.Equals(42).ShouldBeFalse();
            }
        }
    }

    public class GetHashCodeMethod
    {
        [Fact]
        public void IsConsistentWithEquality_ForTwoEqualInstances()
        {
            var a = new ContentType("text/html", ContentMode.Text, "UTF-8");
            var b = new ContentType("text/html", ContentMode.Binary);
            a.GetHashCode().ShouldBe(b.GetHashCode());
        }

        [Fact]
        public void IsConsistentWithEquality_ForDifferentCase()
        {
            var a = new ContentType("text/html", ContentMode.Text);
            var b = new ContentType("TEXT/HTML", ContentMode.Text);
            a.GetHashCode().ShouldBe(b.GetHashCode());
        }
    }

    public class RegisterContentType
    {
        [Fact]
        public void RegistersUnderFullStringKey_WhenCharSetIsPresent()
        {
            var mimeType = UniqueMimeType();
            var ct = new ContentType(mimeType, ContentMode.Text, "UTF-8");
            ContentType.Register(ct);
            ContentType.FromMimeType($"{mimeType}; charset=UTF-8").ShouldBeSameAs(ct);
        }

        [Fact]
        public void RegistersUnderBareValueKey_WhenCharSetIsPresent()
        {
            var mimeType = UniqueMimeType();
            var ct = new ContentType(mimeType, ContentMode.Text, "UTF-8");
            ContentType.Register(ct);
            ContentType.FromMimeType(mimeType).ShouldBeSameAs(ct);
        }

        [Fact]
        public void FirstRegistrationWins_ForBareValueKey()
        {
            var mimeType = UniqueMimeType();
            var first = new ContentType(mimeType, ContentMode.Text, "UTF-8");
            var second = new ContentType(mimeType, ContentMode.Text, "UTF-16");
            ContentType.Register(first);
            ContentType.Register(second);
            ContentType.FromMimeType(mimeType).ShouldBeSameAs(first);
        }

        [Fact]
        public void RegistersExtensions()
        {
            var mimeType = UniqueMimeType();
            var ext = UniqueExtension();
            var ct = new ContentType(mimeType, ContentMode.Binary);
            ContentType.Register(ct, ext);
            ContentType.FromExtension(ext).ShouldBeSameAs(ct);
        }

        [Fact]
        public void HasNoEffect_WhenAlreadyRegistered()
        {
            var mimeType = UniqueMimeType();
            var first = new ContentType(mimeType, ContentMode.Binary);
            var second = new ContentType(mimeType, ContentMode.Text);
            ContentType.Register(first);
            ContentType.Register(second);
            ContentType.FromMimeType(mimeType).ShouldBeSameAs(first);
        }

        [Fact]
        public void Throws_WhenContentTypeIsMultipart()
        {
            var ct = new ContentType("multipart/mixed", ContentMode.Binary);
            Should.Throw<InvalidOperationException>(() => ContentType.Register(ct));
        }
    }

    public class RegisterString_Mode
    {
        [Fact]
        public void ParsesCombinedValueString()
        {
            var mimeType = UniqueMimeType();
            ContentType.Register($"{mimeType}; charset=UTF-8", ContentMode.Text);
            var result = ContentType.FromMimeType(mimeType);
            result.Value.ShouldBe(mimeType);
            result.CharSet.ShouldBe("UTF-8");
            result.Mode.ShouldBe(ContentMode.Text);
        }

        [Fact]
        public void RegistersBareValueString()
        {
            var mimeType = UniqueMimeType();
            ContentType.Register(mimeType, ContentMode.Binary);
            ContentType.FromMimeType(mimeType).Value.ShouldBe(mimeType);
        }

        [Fact]
        public void Throws_WhenValueIsMultipart()
        {
            Should.Throw<InvalidOperationException>(() =>
                ContentType.Register("multipart/mixed", ContentMode.Binary));
        }
    }

    public class RegisterString_CharSet_Mode
    {
        [Fact]
        public void RegistersWithSeparateCharSet()
        {
            var mimeType = UniqueMimeType();
            ContentType.Register(mimeType, "UTF-8", ContentMode.Text);
            var result = ContentType.FromMimeType(mimeType);
            result.Value.ShouldBe(mimeType);
            result.CharSet.ShouldBe("UTF-8");
            result.Mode.ShouldBe(ContentMode.Text);
        }

        [Fact]
        public void TrimsWhitespaceFromValue()
        {
            var mimeType = UniqueMimeType();
            ContentType.Register($"  {mimeType}  ", "UTF-8", ContentMode.Text);
            ContentType.FromMimeType(mimeType).Value.ShouldBe(mimeType);
        }

        [Fact]
        public void Throws_WhenValueIsMultipart()
        {
            Should.Throw<InvalidOperationException>(() =>
                ContentType.Register("multipart/mixed", "UTF-8", ContentMode.Binary));
        }
    }

    public class FromMimeTypeMethod
    {
        [Fact]
        public void ReturnsExactMatch_WhenFullStringIsRegistered()
        {
            ContentType.FromMimeType("text/html; charset=UTF-8").ShouldBeSameAs(ContentType.Html);
        }

        [Fact]
        public void ReturnsPartialMatch_WhenOnlyBareValueIsRegistered()
        {
            ContentType.FromMimeType("text/html; charset=utf-16").ShouldBeSameAs(ContentType.Html);
        }

        [Fact]
        public void CreatesAndRegistersNewInstance_ForUnknownType()
        {
            var mimeType = UniqueMimeType();
            var first = ContentType.FromMimeType(mimeType);
            var second = ContentType.FromMimeType(mimeType);
            first.ShouldBeSameAs(second);
        }

        [Fact]
        public void InfersTextMode_ForTextPrefix()
        {
            var mimeType = $"text/{Guid.NewGuid():N}";
            ContentType.FromMimeType(mimeType).Mode.ShouldBe(ContentMode.Text);
        }

        [Fact]
        public void InfersTextMode_ForXmlSuffix()
        {
            var mimeType = $"{UniqueMimeType()}+xml";
            ContentType.FromMimeType(mimeType).Mode.ShouldBe(ContentMode.Text);
        }

        [Fact]
        public void InfersTextMode_ForJsonSuffix()
        {
            var mimeType = $"{UniqueMimeType()}+json";
            ContentType.FromMimeType(mimeType).Mode.ShouldBe(ContentMode.Text);
        }

        [Fact]
        public void DefaultsToBinaryMode_ForUnrecognizedPattern()
        {
            ContentType.FromMimeType(UniqueMimeType()).Mode.ShouldBe(ContentMode.Binary);
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            ContentType.FromMimeType("TEXT/HTML").ShouldBeSameAs(ContentType.Html);
        }

        [Fact]
        public void ReturnsNewInstance_ForMultipartType()
        {
            var ct1 = ContentType.FromMimeType("multipart/form-data; boundary=abc123");
            var ct2 = ContentType.FromMimeType("multipart/form-data; boundary=abc123");
            ct1.ShouldNotBeSameAs(ct2);
        }

        [Fact]
        public void ParsesBoundary_ForMultipartType()
        {
            var ct = ContentType.FromMimeType("multipart/form-data; boundary=----WebKitFormBoundary");
            ct.Boundary.ShouldBe("----WebKitFormBoundary");
        }

        [Fact]
        public void ParsesBoundaryAndCharset_WhenBothPresent()
        {
            var ct = ContentType.FromMimeType("multipart/mixed; charset=UTF-8; boundary=abc123");
            ct.Boundary.ShouldBe("abc123");
            ct.CharSet.ShouldBe("UTF-8");
        }

        [Fact]
        public void ParsesBoundaryRegardlessOfParameterOrder()
        {
            var ct = ContentType.FromMimeType("multipart/mixed; boundary=abc123; charset=UTF-8");
            ct.Boundary.ShouldBe("abc123");
            ct.CharSet.ShouldBe("UTF-8");
        }

        [Fact]
        public void ReturnsMultipartInstance_WithIsMultipartTrue()
        {
            var ct = ContentType.FromMimeType("multipart/form-data; boundary=abc123");
            ct.IsMultipart.ShouldBeTrue();
        }
    }

    public class FromExtensionMethod
    {
        [Fact]
        public void ReturnsCorrectInstance_ForKnownExtensionWithoutDot()
        {
            ContentType.FromExtension("html").ShouldBeSameAs(ContentType.Html);
        }

        [Fact]
        public void ReturnsCorrectInstance_ForKnownExtensionWithLeadingDot()
        {
            ContentType.FromExtension(".html").ShouldBeSameAs(ContentType.Html);
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            ContentType.FromExtension(".HTML").ShouldBeSameAs(ContentType.Html);
        }

        [Fact]
        public void ReturnsBinary_ForUnknownExtension()
        {
            ContentType.FromExtension(UniqueExtension()).ShouldBeSameAs(ContentType.Binary);
        }
    }

    public class IsMultipartContentMethod
    {
        [Fact]
        public void ReturnsTrue_ForMultipartString()
        {
            ContentType.IsMultipartContent("multipart/form-data; boundary=abc123").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_ForMultipartStringWithoutBoundary()
        {
            ContentType.IsMultipartContent("multipart/mixed").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_ForNonMultipartString()
        {
            ContentType.IsMultipartContent("text/html; charset=utf-8").ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            ContentType.IsMultipartContent("MULTIPART/FORM-DATA; boundary=abc").ShouldBeTrue();
        }
    }

    public class MultipartFormDataProperty
    {
        [Fact]
        public void ReturnsMultipartFormDataValue()
        {
            ContentType.MultipartFormData.Value.ShouldBe("multipart/form-data");
        }

        [Fact]
        public void ReturnsFreshInstance_OnEachAccess()
        {
            var a = ContentType.MultipartFormData;
            var b = ContentType.MultipartFormData;
            a.ShouldNotBeSameAs(b);
        }

        [Fact]
        public void GeneratesUniqueBoundary_OnEachAccess()
        {
            var a = ContentType.MultipartFormData;
            var b = ContentType.MultipartFormData;
            a.Boundary.ShouldNotBe(b.Boundary);
        }

        [Fact]
        public void IsMultipart()
        {
            ContentType.MultipartFormData.IsMultipart.ShouldBeTrue();
        }

        [Fact]
        public void EqualsMultipartFormDataString()
        {
            (ContentType.MultipartFormData == "multipart/form-data").ShouldBeTrue();
        }
    }

    public class ForMultipartMethod
    {
        [Fact]
        public void ReturnsMixed()
        {
            ContentType.ForMultipart(Multipart.Mixed).Value.ShouldBe("multipart/mixed");
        }

        [Fact]
        public void ReturnsAlternative()
        {
            ContentType.ForMultipart(Multipart.Alternative).Value.ShouldBe("multipart/alternative");
        }

        [Fact]
        public void ReturnsDigest()
        {
            ContentType.ForMultipart(Multipart.Digest).Value.ShouldBe("multipart/digest");
        }

        [Fact]
        public void ReturnsEncrypted()
        {
            ContentType.ForMultipart(Multipart.Encrypted).Value.ShouldBe("multipart/encrypted");
        }

        [Fact]
        public void ReturnsFormData_WithHyphen()
        {
            ContentType.ForMultipart(Multipart.FormData).Value.ShouldBe("multipart/form-data");
        }

        [Fact]
        public void ReturnsRelated()
        {
            ContentType.ForMultipart(Multipart.Related).Value.ShouldBe("multipart/related");
        }

        [Fact]
        public void ReturnsSigned()
        {
            ContentType.ForMultipart(Multipart.Signed).Value.ShouldBe("multipart/signed");
        }

        [Fact]
        public void ReturnsParallel()
        {
            ContentType.ForMultipart(Multipart.Parallel).Value.ShouldBe("multipart/parallel");
        }

        [Fact]
        public void ReturnsFreshInstance_OnEachCall()
        {
            var a = ContentType.ForMultipart(Multipart.Mixed);
            var b = ContentType.ForMultipart(Multipart.Mixed);
            a.ShouldNotBeSameAs(b);
        }

        [Fact]
        public void GeneratesUniqueBoundary_OnEachCall()
        {
            var a = ContentType.ForMultipart(Multipart.Mixed);
            var b = ContentType.ForMultipart(Multipart.Mixed);
            a.Boundary.ShouldNotBe(b.Boundary);
        }

        [Fact]
        public void Throws_ForUnrecognizedEnumValue()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
                ContentType.ForMultipart((Multipart)999));
        }
    }

    public class WellKnownStaticFields
    {
        [Fact]
        public void Html_HasCorrectProperties()
        {
            ContentType.Html.Value.ShouldBe("text/html");
            ContentType.Html.Mode.ShouldBe(ContentMode.Text);
            ContentType.Html.CharSet.ShouldBe("UTF-8");
        }

        [Fact]
        public void Json_HasCorrectProperties()
        {
            ContentType.Json.Value.ShouldBe("application/json");
            ContentType.Json.Mode.ShouldBe(ContentMode.Text);
            ContentType.Json.CharSet.ShouldBe("UTF-8");
        }

        [Fact]
        public void Png_HasCorrectProperties()
        {
            ContentType.Png.Value.ShouldBe("image/png");
            ContentType.Png.Mode.ShouldBe(ContentMode.Binary);
            ContentType.Png.CharSet.ShouldBe(string.Empty);
        }

        [Fact]
        public void Icon_HasCorrectIanaValue()
        {
            ContentType.Icon.Value.ShouldBe("image/vnd.microsoft.icon");
        }

        [Fact]
        public void LegacyIconMimeType_ResolvesToIconInstance()
        {
            ContentType.FromMimeType("image/x-icon").ShouldBeSameAs(ContentType.Icon);
        }

        [Fact]
        public void Binary_HasCorrectProperties()
        {
            ContentType.Binary.Value.ShouldBe("application/octet-stream");
            ContentType.Binary.Mode.ShouldBe(ContentMode.Binary);
        }

        [Fact]
        public void Svg_HasCorrectProperties()
        {
            ContentType.Svg.Value.ShouldBe("image/svg+xml");
            ContentType.Svg.Mode.ShouldBe(ContentMode.Text);
            ContentType.Svg.CharSet.ShouldBe("UTF-8");
        }
    }
}