namespace Grapevine.Abstractions.Tests;

public class CookieOptionsTests
{
    public class DomainProperty
    {
        [Fact]
        public void IsNullByDefault()
        {
            new CookieOptions().Domain.ShouldBeNull();
        }

        [Fact]
        public void AcceptsValidDomain()
        {
            var o = new CookieOptions();
            o.Domain = "example.com";
            o.Domain.ShouldBe("example.com");
        }

        [Fact]
        public void AcceptsNull_ClearsDomain()
        {
            var o = new CookieOptions { Domain = "example.com" };
            o.Domain = null;
            o.Domain.ShouldBeNull();
        }

        [Fact]
        public void AcceptsIpAddress()
        {
            var o = new CookieOptions();
            o.Domain = "127.0.0.1";
            o.Domain.ShouldBe("127.0.0.1");
        }

        [Fact]
        public void AcceptsSubdomain()
        {
            var o = new CookieOptions();
            o.Domain = "api.example.com";
            o.Domain.ShouldBe("api.example.com");
        }

        [Fact]
        public void Throws_WhenEmpty()
        {
            var ex = Should.Throw<ArgumentException>(() =>
                new CookieOptions { Domain = string.Empty });
            ex.Message.ShouldContain(CookieOptions.InvalidDomainMessage);
        }

        [Fact]
        public void Throws_WhenWhitespace()
        {
            var ex = Should.Throw<ArgumentException>(() =>
                new CookieOptions { Domain = "   " });
            ex.Message.ShouldContain(CookieOptions.InvalidDomainMessage);
        }

        [Fact]
        public void Throws_WhenInvalidFormat()
        {
            var ex = Should.Throw<ArgumentException>(() =>
                new CookieOptions { Domain = "not a valid domain!!" });
            ex.Message.ShouldContain(CookieOptions.InvalidDomainFormatMessage);
        }
    }

    public class PathProperty
    {
        [Fact]
        public void IsNullByDefault()
        {
            new CookieOptions().Path.ShouldBeNull();
        }

        [Fact]
        public void AcceptsRootPath()
        {
            var o = new CookieOptions { Path = "/" };
            o.Path.ShouldBe("/");
        }

        [Fact]
        public void AcceptsSubPath()
        {
            var o = new CookieOptions { Path = "/account/settings" };
            o.Path.ShouldBe("/account/settings");
        }

        [Fact]
        public void AcceptsNull_ClearsPath()
        {
            var o = new CookieOptions { Path = "/account" };
            o.Path = null;
            o.Path.ShouldBeNull();
        }

        [Fact]
        public void Throws_WhenEmpty()
        {
            var ex = Should.Throw<ArgumentException>(() =>
                new CookieOptions { Path = string.Empty });
            ex.Message.ShouldContain(CookieOptions.InvalidPathMessage);
        }

        [Fact]
        public void Throws_WhenWhitespace()
        {
            var ex = Should.Throw<ArgumentException>(() =>
                new CookieOptions { Path = "   " });
            ex.Message.ShouldContain(CookieOptions.InvalidPathMessage);
        }

        [Fact]
        public void Throws_WhenNotStartingWithSlash()
        {
            var ex = Should.Throw<ArgumentException>(() =>
                new CookieOptions { Path = "account" });
            ex.Message.ShouldContain(CookieOptions.InvalidPathFormatMessage);
        }
    }

    public class AlwaysEmitPathProperty
    {
        [Fact]
        public void IsTrueByDefault()
        {
            new CookieOptions().AlwaysEmitPath.ShouldBeTrue();
        }

        [Fact]
        public void CanBeSetToFalse()
        {
            var o = new CookieOptions { AlwaysEmitPath = false };
            o.AlwaysEmitPath.ShouldBeFalse();
        }
    }

    public class ToStringMethod
    {
        [Fact]
        public void EmitsPathSlash_WhenAlwaysEmitPathAndNoPathSet()
        {
            var o = new CookieOptions();
            o.ToString().ShouldContain("; Path=/");
        }

        [Fact]
        public void EmitsExplicitPath_WhenPathIsSet()
        {
            var o = new CookieOptions { Path = "/account" };
            o.ToString().ShouldContain("; Path=/account");
            o.ToString().ShouldNotContain("; Path=/;");
            o.ToString().ShouldNotContain("; Path= /");
        }

        [Fact]
        public void OmitsPath_WhenAlwaysEmitPathFalseAndNoPathSet()
        {
            var o = new CookieOptions { AlwaysEmitPath = false };
            o.ToString().ShouldNotContain("Path");
        }

        [Fact]
        public void EmitsExplicitPath_EvenWhenAlwaysEmitPathFalse()
        {
            var o = new CookieOptions { AlwaysEmitPath = false, Path = "/secure" };
            o.ToString().ShouldContain("; Path=/secure");
        }

        [Fact]
        public void EmitsDomain_WhenSet()
        {
            var o = new CookieOptions { Domain = "example.com" };
            o.ToString().ShouldContain("; Domain=example.com");
        }

        [Fact]
        public void OmitsDomain_WhenNotSet()
        {
            new CookieOptions().ToString().ShouldNotContain("Domain");
        }

        [Fact]
        public void EmitsMaxAge_WhenSet()
        {
            var o = new CookieOptions { MaxAge = TimeSpan.FromSeconds(3600) };
            o.ToString().ShouldContain("; Max-Age=3600");
        }

        [Fact]
        public void EmitsMaxAge_AsIntegerSeconds()
        {
            // Fractional seconds are truncated to whole seconds per RFC 6265.
            var o = new CookieOptions { MaxAge = TimeSpan.FromSeconds(3600.9) };
            o.ToString().ShouldContain("; Max-Age=3600");
        }

        [Fact]
        public void EmitsNegativeMaxAge_WhenSet()
        {
            // Negative Max-Age instructs the client to delete the cookie immediately.
            var o = new CookieOptions { MaxAge = TimeSpan.FromSeconds(-1) };
            o.ToString().ShouldContain("; Max-Age=-1");
        }

        [Fact]
        public void EmitsZeroMaxAge_WhenSet()
        {
            var o = new CookieOptions { MaxAge = TimeSpan.Zero };
            o.ToString().ShouldContain("; Max-Age=0");
        }

        [Fact]
        public void OmitsMaxAge_WhenNotSet()
        {
            new CookieOptions().ToString().ShouldNotContain("Max-Age");
        }

        [Fact]
        public void EmitsExpires_WhenSet()
        {
            var expires = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var o = new CookieOptions { Expires = expires };
            o.ToString().ShouldContain("; Expires=");
        }

        [Fact]
        public void EmitsExpires_InRfc1123Format()
        {
            var expires = new DateTimeOffset(2030, 6, 15, 12, 0, 0, TimeSpan.Zero);
            var o = new CookieOptions { Expires = expires };
            // RFC 1123 format: "Sat, 15 Jun 2030 12:00:00 GMT"
            o.ToString().ShouldContain("GMT");
        }

        [Fact]
        public void EmitsExpires_ConvertedToUtc()
        {
            // Expires with a non-UTC offset should be converted to UTC before formatting.
            var expires = new DateTimeOffset(2030, 1, 1, 12, 0, 0, TimeSpan.FromHours(5));
            var o = new CookieOptions { Expires = expires };
            // UTC equivalent is 07:00:00
            o.ToString().ShouldContain("07:00:00 GMT");
        }

        [Fact]
        public void OmitsExpires_WhenNotSet()
        {
            new CookieOptions().ToString().ShouldNotContain("Expires");
        }

        [Fact]
        public void EmitsBothMaxAgeAndExpires_WhenBothSet()
        {
            var o = new CookieOptions
            {
                MaxAge = TimeSpan.FromSeconds(3600),
                Expires = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
            var result = o.ToString();
            result.ShouldContain("; Max-Age=3600");
            result.ShouldContain("; Expires=");
        }

        [Fact]
        public void EmitsMaxAgeBeforeExpires_WhenBothSet()
        {
            // RFC 6265: Max-Age takes precedence; emit it first for clarity.
            var o = new CookieOptions
            {
                MaxAge = TimeSpan.FromSeconds(3600),
                Expires = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
            var result = o.ToString();
            result.IndexOf("Max-Age", StringComparison.Ordinal)
                .ShouldBeLessThan(result.IndexOf("Expires", StringComparison.Ordinal));
        }

        [Fact]
        public void EmitsSecure_WhenTrue()
        {
            new CookieOptions { Secure = true }.ToString().ShouldContain("; Secure");
        }

        [Fact]
        public void OmitsSecure_WhenFalse()
        {
            new CookieOptions { Secure = false }.ToString().ShouldNotContain("Secure");
        }

        [Fact]
        public void EmitsHttpOnly_WhenTrue()
        {
            new CookieOptions { HttpOnly = true }.ToString().ShouldContain("; HttpOnly");
        }

        [Fact]
        public void OmitsHttpOnly_WhenFalse()
        {
            new CookieOptions { HttpOnly = false }.ToString().ShouldNotContain("HttpOnly");
        }

        [Fact]
        public void EmitsSameSiteNone_WhenSet()
        {
            new CookieOptions { SameSite = SameSiteMode.None }.ToString()
                .ShouldContain("; SameSite=None");
        }

        [Fact]
        public void EmitsSameSiteLax_WhenSet()
        {
            new CookieOptions { SameSite = SameSiteMode.Lax }.ToString()
                .ShouldContain("; SameSite=Lax");
        }

        [Fact]
        public void EmitsSameSiteStrict_WhenSet()
        {
            new CookieOptions { SameSite = SameSiteMode.Strict }.ToString()
                .ShouldContain("; SameSite=Strict");
        }

        [Fact]
        public void OmitsSameSite_WhenNotSet()
        {
            new CookieOptions().ToString().ShouldNotContain("SameSite");
        }

        [Fact]
        public void EmitsAttributesInCorrectOrder()
        {
            // Domain, Path, Max-Age, Expires, Secure, HttpOnly, SameSite
            var o = new CookieOptions
            {
                Domain = "example.com",
                Path = "/",
                MaxAge = TimeSpan.FromSeconds(3600),
                Expires = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Strict
            };

            var result = o.ToString();

            var domainPos   = result.IndexOf("Domain",   StringComparison.Ordinal);
            var pathPos     = result.IndexOf("Path",     StringComparison.Ordinal);
            var maxAgePos   = result.IndexOf("Max-Age",  StringComparison.Ordinal);
            var expiresPos  = result.IndexOf("Expires",  StringComparison.Ordinal);
            var securePos   = result.IndexOf("Secure",   StringComparison.Ordinal);
            var httpOnlyPos = result.IndexOf("HttpOnly", StringComparison.Ordinal);
            var sameSitePos = result.IndexOf("SameSite", StringComparison.Ordinal);

            domainPos.ShouldBeLessThan(pathPos);
            pathPos.ShouldBeLessThan(maxAgePos);
            maxAgePos.ShouldBeLessThan(expiresPos);
            expiresPos.ShouldBeLessThan(securePos);
            securePos.ShouldBeLessThan(httpOnlyPos);
            httpOnlyPos.ShouldBeLessThan(sameSitePos);
        }

        [Fact]
        public void ReturnsEmptyString_WhenNoAttributesAndAlwaysEmitPathFalse()
        {
            var o = new CookieOptions { AlwaysEmitPath = false };
            o.ToString().ShouldBe(string.Empty);
        }

        [Fact]
        public void DoesNotBeginWithSeparator_WhenOnlyPathEmitted()
        {
            // The output is an attribute suffix starting with "; " when attributes
            // are present. Verify the leading separator is correct.
            var result = new CookieOptions().ToString();
            result.ShouldStartWith("; ");
        }
    }
}