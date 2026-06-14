using Grapevine;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests;

public class CookieTests
{
    public class TwoParameterConstructor
    {
        [Fact]
        public void SetsName()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            c.Name.ShouldBe("session");
        }

        [Fact]
        public void SetsValue()
        {
            var value = Guid.NewGuid().ToString();
            new Cookie("session", value).Value.ShouldBe(value);
        }

        [Fact]
        public void CreatesDefaultOptions()
        {
            new Cookie("session", Guid.NewGuid().ToString()).Options.ShouldNotBeNull();
        }

        [Fact]
        public void DefaultOptions_HasAlwaysEmitPathTrue()
        {
            new Cookie("session", Guid.NewGuid().ToString()).Options.AlwaysEmitPath.ShouldBeTrue();
        }

        [Fact]
        public void Throws_WhenNameIsNull()
        {
            var ex = Should.Throw<ArgumentException>(() => new Cookie(null!, Guid.NewGuid().ToString()));
            ex.Message.ShouldContain(Cookie.InvalidNameMessage);
        }

        [Fact]
        public void Throws_WhenNameIsEmpty()
        {
            var ex = Should.Throw<ArgumentException>(() => new Cookie(string.Empty, Guid.NewGuid().ToString()));
            ex.Message.ShouldContain(Cookie.InvalidNameMessage);
        }

        [Fact]
        public void Throws_WhenNameIsWhitespace()
        {
            var ex = Should.Throw<ArgumentException>(() => new Cookie("   ", Guid.NewGuid().ToString()));
            ex.Message.ShouldContain(Cookie.InvalidNameMessage);
        }

        [Fact]
        public void Throws_WhenNameContainsInvalidToken()
        {
            var ex = Should.Throw<ArgumentException>(() => new Cookie("my cookie", Guid.NewGuid().ToString()));
            ex.Message.ShouldContain(Cookie.InvalidNameTokenMessage);
        }

        [Fact]
        public void Throws_WhenValueIsNull()
        {
            var ex = Should.Throw<ArgumentNullException>(() => new Cookie("session", null!));
            ex.Message.ShouldContain(Cookie.NullValueMessage);
        }

        [Fact]
        public void Throws_WhenValueContainsSemicolon()
        {
            var ex = Should.Throw<ArgumentException>(() => new Cookie("session", "val;ue"));
            ex.Message.ShouldContain(Cookie.InvalidValueMessage);
        }
    }

    public class ThreeParameterConstructor
    {
        [Fact]
        public void UsesProvidedOptions()
        {
            var options = new CookieOptions { Secure = true };
            var c = new Cookie("session", Guid.NewGuid().ToString(), options);
            c.Options.ShouldBeSameAs(options);
        }

        [Fact]
        public void Throws_WhenOptionsIsNull()
        {
            Should.Throw<ArgumentNullException>(() =>
                new Cookie("session", Guid.NewGuid().ToString(), null!));
        }
    }

    public class NameProperty
    {
        [Fact]
        public void AcceptsValidToken()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            c.Name = "new-name";
            c.Name.ShouldBe("new-name");
        }

        [Fact]
        public void Throws_WhenSetToNull()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            var ex = Should.Throw<ArgumentException>(() => c.Name = null!);
            ex.Message.ShouldContain(Cookie.InvalidNameMessage);
        }

        [Fact]
        public void Throws_WhenSetToNameWithSeparatorChar()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());

            // Each separator character should be rejected.
            foreach (var separator in "()<>@,;:\\\"/[]?={} \t")
            {
                var invalid = $"name{separator}x";
                Should.Throw<ArgumentException>(() => c.Name = invalid);
            }
        }

        [Fact]
        public void Throws_WhenSetToNameWithControlChar()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            Should.Throw<ArgumentException>(() => c.Name = "na\x01me");
        }

        [Fact]
        public void Throws_WhenSetToNameWithDeleteChar()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            Should.Throw<ArgumentException>(() => c.Name = "na\x7fme");
        }
    }

    public class ValueProperty
    {
        [Fact]
        public void AcceptsEmptyString()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            c.Value = string.Empty;
            c.Value.ShouldBe(string.Empty);
        }

        [Fact]
        public void AcceptsValueWithSpecialCharsOtherThanSemicolon()
        {
            var value = "abc=def&ghi+jkl";
            var c = new Cookie("session", value);
            c.Value.ShouldBe(value);
        }

        [Fact]
        public void Throws_WhenSetToNull()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            var ex = Should.Throw<ArgumentNullException>(() => c.Value = null!);
            ex.Message.ShouldContain(Cookie.NullValueMessage);
        }

        [Fact]
        public void Throws_WhenSetToValueContainingSemicolon()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            var ex = Should.Throw<ArgumentException>(() => c.Value = "val;ue");
            ex.Message.ShouldContain(Cookie.InvalidValueMessage);
        }
    }

    public class PassThroughProperties
    {
        [Fact]
        public void Domain_ReadsFromOptions()
        {
            var options = new CookieOptions { Domain = "example.com" };
            new Cookie("session", Guid.NewGuid().ToString(), options)
                .Domain.ShouldBe("example.com");
        }

        [Fact]
        public void Domain_WritesToOptions()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            c.Domain = "example.com";
            c.Options.Domain.ShouldBe("example.com");
        }

        [Fact]
        public void Path_ReadsFromOptions()
        {
            var options = new CookieOptions { Path = "/account" };
            new Cookie("session", Guid.NewGuid().ToString(), options)
                .Path.ShouldBe("/account");
        }

        [Fact]
        public void Path_WritesToOptions()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            c.Path = "/account";
            c.Options.Path.ShouldBe("/account");
        }

        [Fact]
        public void Expires_ReadsFromOptions()
        {
            var expires = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var options = new CookieOptions { Expires = expires };
            new Cookie("session", Guid.NewGuid().ToString(), options)
                .Expires.ShouldBe(expires);
        }

        [Fact]
        public void Expires_WritesToOptions()
        {
            var expires = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var c = new Cookie("session", Guid.NewGuid().ToString());
            c.Expires = expires;
            c.Options.Expires.ShouldBe(expires);
        }

        [Fact]
        public void MaxAge_ReadsFromOptions()
        {
            var maxAge = TimeSpan.FromHours(1);
            var options = new CookieOptions { MaxAge = maxAge };
            new Cookie("session", Guid.NewGuid().ToString(), options)
                .MaxAge.ShouldBe(maxAge);
        }

        [Fact]
        public void MaxAge_WritesToOptions()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            c.MaxAge = TimeSpan.FromHours(1);
            c.Options.MaxAge.ShouldBe(TimeSpan.FromHours(1));
        }

        [Fact]
        public void Secure_ReadsFromOptions()
        {
            var options = new CookieOptions { Secure = true };
            new Cookie("session", Guid.NewGuid().ToString(), options)
                .Secure.ShouldBeTrue();
        }

        [Fact]
        public void Secure_WritesToOptions()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            c.Secure = true;
            c.Options.Secure.ShouldBeTrue();
        }

        [Fact]
        public void HttpOnly_ReadsFromOptions()
        {
            var options = new CookieOptions { HttpOnly = true };
            new Cookie("session", Guid.NewGuid().ToString(), options)
                .HttpOnly.ShouldBeTrue();
        }

        [Fact]
        public void HttpOnly_WritesToOptions()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            c.HttpOnly = true;
            c.Options.HttpOnly.ShouldBeTrue();
        }

        [Fact]
        public void SameSite_ReadsFromOptions()
        {
            var options = new CookieOptions { SameSite = SameSiteMode.Strict };
            new Cookie("session", Guid.NewGuid().ToString(), options)
                .SameSite.ShouldBe(SameSiteMode.Strict);
        }

        [Fact]
        public void SameSite_WritesToOptions()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            c.SameSite = SameSiteMode.Lax;
            c.Options.SameSite.ShouldBe(SameSiteMode.Lax);
        }

        [Fact]
        public void AlwaysEmitPath_ReadsFromOptions()
        {
            var options = new CookieOptions { AlwaysEmitPath = false };
            new Cookie("session", Guid.NewGuid().ToString(), options)
                .AlwaysEmitPath.ShouldBeFalse();
        }

        [Fact]
        public void AlwaysEmitPath_WritesToOptions()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            c.AlwaysEmitPath = false;
            c.Options.AlwaysEmitPath.ShouldBeFalse();
        }
    }

    public class ToStringMethod
    {
        [Fact]
        public void IncludesNameAndValue()
        {
            var value = Guid.NewGuid().ToString();
            var c = new Cookie("session", value);
            c.ToString().ShouldStartWith($"session={value}");
        }

        [Fact]
        public void IncludesDefaultPathSuffix()
        {
            var c = new Cookie("session", Guid.NewGuid().ToString());
            c.ToString().ShouldContain("; Path=/");
        }

        [Fact]
        public void IncludesOptionsSuffix()
        {
            var value = Guid.NewGuid().ToString();
            var c = new Cookie("session", value)
            {
                Secure = true,
                HttpOnly = true
            };
            var result = c.ToString();
            result.ShouldContain("; Secure");
            result.ShouldContain("; HttpOnly");
        }

        [Fact]
        public void DelegatesAttributeSerialisation_ToOptions()
        {
            // ToString should produce exactly "name=value" + Options.ToString(),
            // confirming full delegation rather than independent serialisation.
            var value = Guid.NewGuid().ToString();
            var c = new Cookie("session", value)
            {
                Domain = "example.com",
                Secure = true
            };
            var expected = $"session={value}{c.Options}";
            c.ToString().ShouldBe(expected);
        }

        [Fact]
        public void ProducesValidSetCookieHeaderValue()
        {
            // Verify the full output is a well-formed Set-Cookie header value.
            var value = Guid.NewGuid().ToString();
            var c = new Cookie("session", value)
            {
                Domain = "example.com",
                Path = "/",
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Strict
            };

            var result = c.ToString();
            result.ShouldStartWith($"session={value}");
            result.ShouldContain("; Domain=example.com");
            result.ShouldContain("; Path=/");
            result.ShouldContain("; Secure");
            result.ShouldContain("; HttpOnly");
            result.ShouldContain("; SameSite=Strict");
        }

        [Fact]
        public void SharedOptions_AreReflectedInBothCookies()
        {
            // When two cookies share a CookieOptions instance, a change to the
            // options is reflected in both cookies' ToString output.
            var options = new CookieOptions { Secure = false };
            var c1 = new Cookie("a", Guid.NewGuid().ToString(), options);
            var c2 = new Cookie("b", Guid.NewGuid().ToString(), options);

            options.Secure = true;

            c1.ToString().ShouldContain("; Secure");
            c2.ToString().ShouldContain("; Secure");
        }
    }
}