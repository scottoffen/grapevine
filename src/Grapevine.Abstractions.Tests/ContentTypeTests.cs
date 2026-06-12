using Grapevine;
using Shouldly;
using Xunit;

namespace Grapevine.Tests;

public class ContentTypeTests
{
    private static string UniqueType() => $"application/x-{Guid.NewGuid():N}";
    private static string UniqueExtension() => Guid.NewGuid().ToString("N");

    public class Constructor
    {
        [Fact]
        public void SetsType()
        {
            var ct = new ContentType("text", "html");
            ct.Type.ShouldBe("text");
        }

        [Fact]
        public void SetsSubType()
        {
            var ct = new ContentType("text", "html");
            ct.SubType.ShouldBe("html");
        }

        [Fact]
        public void SetsCharset()
        {
            var ct = new ContentType("text", "html", charset: "UTF-8");
            ct.Charset.ShouldBe("UTF-8");
        }

        [Fact]
        public void DefaultCharsetIsNull()
        {
            var ct = new ContentType("image", "png");
            ct.Charset.ShouldBeNull();
        }

        [Fact]
        public void WhitespaceCharsetIsNull()
        {
            var ct = new ContentType("text", "html", charset: "   ");
            ct.Charset.ShouldBeNull();
        }

        [Fact]
        public void AutoDetectsTextMode_ForTextType()
        {
            var ct = new ContentType("text", "plain");
            ct.Mode.ShouldBe(ContentMode.Text);
            ct.IsBinary.ShouldBeFalse();
        }

        [Fact]
        public void AutoDetectsTextMode_ForJsonSubType()
        {
            var ct = new ContentType("application", "json");
            ct.Mode.ShouldBe(ContentMode.Text);
        }

        [Fact]
        public void AutoDetectsTextMode_WhenCharsetPresent()
        {
            var ct = new ContentType("application", "octet-stream", charset: "UTF-8");
            ct.Mode.ShouldBe(ContentMode.Text);
        }

        [Fact]
        public void AutoDetectsBinaryMode_ForUnrecognizedType()
        {
            var ct = new ContentType("image", "png");
            ct.Mode.ShouldBe(ContentMode.Binary);
            ct.IsBinary.ShouldBeTrue();
        }

        [Fact]
        public void ExplicitModeOverridesAutoDetection()
        {
            var ct = new ContentType("text", "html", mode: ContentMode.Binary);
            ct.Mode.ShouldBe(ContentMode.Binary);
        }

        [Fact]
        public void SetsIsMultipart_WhenTypeIsMultipart()
        {
            var ct = new ContentType("multipart", "form-data");
            ct.IsMultipart.ShouldBeTrue();
        }

        [Fact]
        public void SetsIsMultipart_CaseInsensitive()
        {
            var ct = new ContentType("MULTIPART", "form-data");
            ct.IsMultipart.ShouldBeTrue();
        }

        [Fact]
        public void IsMultipartIsFalse_ForNonMultipartType()
        {
            var ct = new ContentType("text", "html");
            ct.IsMultipart.ShouldBeFalse();
        }

        [Fact]
        public void SetsBoundary_WhenProvidedAndMultipart()
        {
            var ct = new ContentType("multipart", "form-data", boundary: "my-boundary");
            ct.Boundary.ShouldBe("my-boundary");
        }

        [Fact]
        public void GeneratesBoundary_WhenNotProvidedAndMultipart()
        {
            var ct = new ContentType("multipart", "form-data");
            ct.Boundary.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ThrowsOnBoundaryAccess_WhenNotMultipart()
        {
            var ct = new ContentType("text", "html");
            Should.Throw<InvalidOperationException>(() => _ = ct.Boundary);
        }

        [Fact]
        public void Throws_WhenTypeIsNull()
        {
            Should.Throw<ArgumentNullException>(() => new ContentType(null!, "html"));
        }

        [Fact]
        public void DefaultsSubTypeToEmpty_WhenNull()
        {
            var ct = new ContentType("text", null!);
            ct.SubType.ShouldBe(string.Empty);
        }
    }

    public class ToStringMethod
    {
        [Fact]
        public void ReturnsTypeAndSubType()
        {
            var ct = new ContentType("image", "png");
            ct.ToString().ShouldBe("image/png");
        }

        [Fact]
        public void ReturnsTypeOnly_WhenSubTypeIsEmpty()
        {
            var ct = new ContentType("text", string.Empty);
            ct.ToString().ShouldBe("text");
        }

        [Fact]
        public void IncludesCharset_WhenPresent()
        {
            var ct = new ContentType("text", "html", charset: "UTF-8");
            ct.ToString().ShouldBe("text/html; charset=UTF-8");
        }

        [Fact]
        public void IncludesBoundary_WhenMultipart()
        {
            var ct = new ContentType("multipart", "form-data", boundary: "test-boundary");
            ct.ToString().ShouldBe("multipart/form-data; boundary=test-boundary");
        }

        [Fact]
        public void GeneratesAndIncludesBoundary_WhenMultipartAndNoBoundaryProvided()
        {
            var ct = new ContentType("multipart", "form-data");
            ct.ToString().ShouldStartWith("multipart/form-data; boundary=");
        }

        [Fact]
        public void IncludesAdditionalParameters()
        {
            var ct = new ContentType("text", "html");
            ct.Parameters["foo"] = "bar";
            ct.ToString().ShouldContain("; foo=bar");
        }

        [Fact]
        public void QuotesParameterValues_WhenContainingSpecialChars()
        {
            var ct = new ContentType("text", "html");
            ct.Parameters["foo"] = "bar baz";
            ct.ToString().ShouldContain("; foo=\"bar baz\"");
        }
    }

    public class ParseMethod
    {
        [Fact]
        public void ParsesTypeAndSubType()
        {
            var ct = ContentType.Parse("text/html");
            ct.Type.ShouldBe("text");
            ct.SubType.ShouldBe("html");
        }

        [Fact]
        public void ParsesCharset()
        {
            var ct = ContentType.Parse("text/html; charset=UTF-8");
            ct.Charset.ShouldBe("UTF-8");
        }

        [Fact]
        public void ParsesBoundary()
        {
            var ct = ContentType.Parse("multipart/form-data; boundary=abc123");
            ct.Boundary.ShouldBe("abc123");
        }

        [Fact]
        public void ParsesAdditionalParameters()
        {
            var ct = ContentType.Parse("text/html; charset=UTF-8; foo=bar");
            ct.Parameters["foo"].ShouldBe("bar");
        }

        [Fact]
        public void ParsesParametersRegardlessOfOrder()
        {
            var ct = ContentType.Parse("multipart/mixed; boundary=abc123; charset=UTF-8");
            ct.Boundary.ShouldBe("abc123");
            ct.Charset.ShouldBe("UTF-8");
        }

        [Fact]
        public void StripsQuotesFromParameterValues()
        {
            var ct = ContentType.Parse("text/html; charset=\"UTF-8\"");
            ct.Charset.ShouldBe("UTF-8");
        }

        [Fact]
        public void Throws_WhenValueIsNull()
        {
            Should.Throw<ArgumentException>(() => ContentType.Parse(null!));
        }

        [Fact]
        public void Throws_WhenValueIsWhitespace()
        {
            Should.Throw<ArgumentException>(() => ContentType.Parse("   "));
        }

        [Fact]
        public void AlwaysCreatesNewInstance()
        {
            var a = ContentType.Parse("text/html");
            var b = ContentType.Parse("text/html");
            a.ShouldNotBeSameAs(b);
        }
    }

    public class ImplicitStringConversion
    {
        [Fact]
        public void ReturnsFormattedString()
        {
            string result = ContentType.Png;
            result.ShouldBe("image/png");
        }

        [Fact]
        public void IncludesCharset_WhenPresent()
        {
            string result = ContentType.Html;
            result.ShouldBe("text/html; charset=UTF-8");
        }

        [Fact]
        public void IncludesBoundary_WhenMultipart()
        {
            var ct = new ContentType("multipart", "form-data", boundary: "test-boundary");
            string result = ct;
            result.ShouldBe("multipart/form-data; boundary=test-boundary");
        }

        [Fact]
        public void MatchesToString()
        {
            string implicitResult = ContentType.Json;
            implicitResult.ShouldBe(ContentType.Json.ToString());
        }

        [Fact]
        public void ParsesStringToContentType()
        {
            ContentType ct = "text/html; charset=UTF-8";
            ct.Type.ShouldBe("text");
            ct.SubType.ShouldBe("html");
            ct.Charset.ShouldBe("UTF-8");
        }
    }

    public class EqualityOperators
    {
        public class ContentTypeVsContentType
        {
            [Fact]
            public void ReturnsTrue_WhenTypeAndSubTypeAreEqual()
            {
                var a = new ContentType("text", "html", charset: "UTF-8");
                var b = new ContentType("text", "html");
                (a == b).ShouldBeTrue();
            }

            [Fact]
            public void ReturnsTrue_WhenEqualWithDifferentCase()
            {
                var a = new ContentType("text", "html");
                var b = new ContentType("TEXT", "HTML");
                (a == b).ShouldBeTrue();
            }

            [Fact]
            public void ReturnsFalse_WhenTypesDiffer()
            {
                var a = new ContentType("text", "html");
                var b = new ContentType("text", "plain");
                (a == b).ShouldBeFalse();
            }

            [Fact]
            public void InequalityReturnsTrue_WhenTypesDiffer()
            {
                var a = new ContentType("text", "html");
                var b = new ContentType("text", "plain");
                (a != b).ShouldBeTrue();
            }

            [Fact]
            public void ReturnsTrue_WhenBothAreNull()
            {
                ContentType? a = null;
                ContentType? b = null;
                (a == b).ShouldBeTrue();
            }

            [Fact]
            public void ReturnsFalse_WhenOneIsNull()
            {
                ContentType? a = null;
                (a == ContentType.Html).ShouldBeFalse();
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
            public void ReturnsFalse_WhenOtherIsNull()
            {
                ContentType.Html.Equals((ContentType?)null).ShouldBeFalse();
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
        public void IsConsistentWithEquality_ForEqualInstances()
        {
            var a = new ContentType("text", "html", charset: "UTF-8");
            var b = new ContentType("text", "html");
            a.GetHashCode().ShouldBe(b.GetHashCode());
        }

        [Fact]
        public void IsConsistentWithEquality_ForDifferentCase()
        {
            var a = new ContentType("text", "html");
            var b = new ContentType("TEXT", "HTML");
            a.GetHashCode().ShouldBe(b.GetHashCode());
        }
    }

    public class RegisterMethod
    {
        [Fact]
        public void RegistersContentType_ByMimeType()
        {
            var type = UniqueType();
            var parts = type.Split('/');
            ContentType.Register(type);
            ContentType.FromMimeType(type).Type.ShouldBe(parts[0]);
        }

        [Fact]
        public void RegistersExtensions()
        {
            var type = UniqueType();
            var ext = UniqueExtension();
            ContentType.Register(type, ext);
            ContentType.FromExtension(ext).ShouldBe(ContentType.Parse(type));
        }

        [Fact]
        public void HasNoEffect_WhenAlreadyRegistered()
        {
            var type = UniqueType();
            ContentType.Register(type);
            var first = ContentType.FromMimeType(type);
            ContentType.Register(type);
            var second = ContentType.FromMimeType(type);
            first.ShouldBeSameAs(second);
        }

        [Fact]
        public void Throws_WhenContentTypeIsMultipart()
        {
            Should.Throw<InvalidOperationException>(() =>
                ContentType.Register("multipart/mixed"));
        }
    }

    public class FromMimeTypeMethod
    {
        [Fact]
        public void ReturnsWellKnownInstance_ForKnownType()
        {
            ContentType.FromMimeType("text/html").ShouldBeSameAs(ContentType.Html);
        }

        [Fact]
        public void ReturnsWellKnownInstance_WhenCharsetIncluded()
        {
            ContentType.FromMimeType("text/html; charset=UTF-8").ShouldBeSameAs(ContentType.Html);
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            ContentType.FromMimeType("TEXT/HTML").ShouldBeSameAs(ContentType.Html);
        }

        [Fact]
        public void RegistersAndCachesNewInstance_ForUnknownType()
        {
            var type = UniqueType();
            var first = ContentType.FromMimeType(type);
            var second = ContentType.FromMimeType(type);
            first.ShouldBeSameAs(second);
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
            ct.Charset.ShouldBe("UTF-8");
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
        public void ReturnsCorrectType()
        {
            ContentType.MultipartFormData.Type.ShouldBe("multipart");
        }

        [Fact]
        public void ReturnsCorrectSubType()
        {
            ContentType.MultipartFormData.SubType.ShouldBe("form-data");
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
            var ct = ContentType.ForMultipart(Multipart.Mixed);
            ct.Type.ShouldBe("multipart");
            ct.SubType.ShouldBe("mixed");
        }

        [Fact]
        public void ReturnsAlternative()
        {
            ContentType.ForMultipart(Multipart.Alternative).SubType.ShouldBe("alternative");
        }

        [Fact]
        public void ReturnsDigest()
        {
            ContentType.ForMultipart(Multipart.Digest).SubType.ShouldBe("digest");
        }

        [Fact]
        public void ReturnsEncrypted()
        {
            ContentType.ForMultipart(Multipart.Encrypted).SubType.ShouldBe("encrypted");
        }

        [Fact]
        public void ReturnsFormData_WithHyphen()
        {
            ContentType.ForMultipart(Multipart.FormData).SubType.ShouldBe("form-data");
        }

        [Fact]
        public void ReturnsRelated()
        {
            ContentType.ForMultipart(Multipart.Related).SubType.ShouldBe("related");
        }

        [Fact]
        public void ReturnsSigned()
        {
            ContentType.ForMultipart(Multipart.Signed).SubType.ShouldBe("signed");
        }

        [Fact]
        public void ReturnsParallel()
        {
            ContentType.ForMultipart(Multipart.Parallel).SubType.ShouldBe("parallel");
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
            ContentType.Html.Type.ShouldBe("text");
            ContentType.Html.SubType.ShouldBe("html");
            ContentType.Html.Mode.ShouldBe(ContentMode.Text);
            ContentType.Html.Charset.ShouldBe("UTF-8");
        }

        [Fact]
        public void Json_HasCorrectProperties()
        {
            ContentType.Json.Type.ShouldBe("application");
            ContentType.Json.SubType.ShouldBe("json");
            ContentType.Json.Mode.ShouldBe(ContentMode.Text);
            ContentType.Json.Charset.ShouldBe("UTF-8");
        }

        [Fact]
        public void Png_HasCorrectProperties()
        {
            ContentType.Png.Type.ShouldBe("image");
            ContentType.Png.SubType.ShouldBe("png");
            ContentType.Png.Mode.ShouldBe(ContentMode.Binary);
            ContentType.Png.Charset.ShouldBeNull();
        }

        [Fact]
        public void Icon_HasCorrectIanaValue()
        {
            ContentType.Icon.Type.ShouldBe("image");
            ContentType.Icon.SubType.ShouldBe("vnd.microsoft.icon");
        }

        [Fact]
        public void LegacyIconMimeType_ResolvesToIconInstance()
        {
            ContentType.FromMimeType("image/x-icon").ShouldBeSameAs(ContentType.Icon);
        }

        [Fact]
        public void Binary_HasCorrectProperties()
        {
            ContentType.Binary.Type.ShouldBe("application");
            ContentType.Binary.SubType.ShouldBe("octet-stream");
            ContentType.Binary.Mode.ShouldBe(ContentMode.Binary);
        }

        [Fact]
        public void Svg_HasCorrectProperties()
        {
            ContentType.Svg.Type.ShouldBe("image");
            ContentType.Svg.SubType.ShouldBe("svg+xml");
            ContentType.Svg.Mode.ShouldBe(ContentMode.Text);
            ContentType.Svg.Charset.ShouldBe("UTF-8");
        }

        [Fact]
        public void ProblemDetailsJson_HasCorrectProperties()
        {
            ContentType.ProblemDetailsJson.Type.ShouldBe("application");
            ContentType.ProblemDetailsJson.SubType.ShouldBe("problem+json");
            ContentType.ProblemDetailsJson.Mode.ShouldBe(ContentMode.Text);
            ContentType.ProblemDetailsJson.Charset.ShouldBe("UTF-8");
        }
    }
}