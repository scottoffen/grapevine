using HttpMethod = Grapevine.HttpMethod;

namespace Grapeseed.Tests;

public class HttpMethodTests
{
    private static string UniqueMethod() => Guid.NewGuid().ToString("N").ToUpper();

    public class NameProperty
    {
        [Fact]
        public void ReturnsUppercaseName()
        {
            var method = HttpMethod.Parse("get");
            method.Name.ShouldBe("GET");
        }

        [Fact]
        public void TrimsWhitespace()
        {
            var method = HttpMethod.Parse("  GET  ");
            method.Name.ShouldBe("GET");
        }
    }

    public class ToStringMethod
    {
        [Fact]
        public void ReturnsName()
        {
            HttpMethod.Get.ToString().ShouldBe("GET");
        }
    }

    public class EqualsMethod
    {
        [Fact]
        public void ReturnsTrue_ForEqualMethods()
        {
            var a = HttpMethod.Parse("GET");
            var b = HttpMethod.Parse("GET");
            a.Equals(b).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenCaseDiffers()
        {
            var a = HttpMethod.Parse("get");
            var b = HttpMethod.Parse("GET");
            a.Equals(b).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_ForDifferentMethods()
        {
            HttpMethod.Get.Equals(HttpMethod.Post).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenOtherIsNull()
        {
            HttpMethod.Get.Equals((HttpMethod?)null).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenObjectIsUnrelatedType()
        {
            HttpMethod.Get.Equals(42).ShouldBeFalse();
        }
    }

    public class GetHashCodeMethod
    {
        [Fact]
        public void IsConsistentWithEquality_ForEqualInstances()
        {
            var a = HttpMethod.Parse("get");
            var b = HttpMethod.Parse("GET");
            a.GetHashCode().ShouldBe(b.GetHashCode());
        }
    }

    public class MatchesMethod
    {
        [Fact]
        public void ReturnsTrue_WhenThisIsAny()
        {
            HttpMethod.Any.Matches(HttpMethod.Get).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenOtherIsAny()
        {
            HttpMethod.Get.Matches(HttpMethod.Any).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenBothMethodsAreEqual()
        {
            HttpMethod.Get.Matches(HttpMethod.Get).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenMethodsDifferAndNeitherIsAny()
        {
            HttpMethod.Get.Matches(HttpMethod.Post).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenOtherIsNull()
        {
            HttpMethod.Get.Matches(null).ShouldBeFalse();
        }
    }

    public class EqualityOperators
    {
        [Fact]
        public void ReturnsTrue_WhenMethodsAreEqual()
        {
            (HttpMethod.Get == HttpMethod.Parse("GET")).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenMethodsDiffer()
        {
            (HttpMethod.Get == HttpMethod.Post).ShouldBeFalse();
        }

        [Fact]
        public void InequalityReturnsTrue_WhenMethodsDiffer()
        {
            (HttpMethod.Get != HttpMethod.Post).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenBothAreNull()
        {
            HttpMethod? a = null;
            HttpMethod? b = null;
            (a == b).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenOneIsNull()
        {
            HttpMethod? a = null;
            (a == HttpMethod.Get).ShouldBeFalse();
        }
    }

    public class ImplicitConversions
    {
        public class StringToHttpMethod
        {
            [Fact]
            public void ReturnsCorrectInstance_ForKnownMethod()
            {
                HttpMethod method = "GET";
                method.Name.ShouldBe("GET");
            }

            [Fact]
            public void IsCaseInsensitive()
            {
                HttpMethod lower = "get";
                HttpMethod upper = "GET";
                lower.ShouldBeSameAs(upper);
            }

            [Fact]
            public void RegistersAndReturnsNewInstance_ForUnknownMethod()
            {
                var name = UniqueMethod();
                HttpMethod method = name;
                method.Name.ShouldBe(name);
            }

            [Fact]
            public void Throws_WhenNameIsNull()
            {
                Should.Throw<ArgumentNullException>(() => { HttpMethod m = (string)null!; });
            }

            [Fact]
            public void Throws_WhenNameIsWhitespace()
            {
                Should.Throw<ArgumentNullException>(() => { HttpMethod m = "   "; });
            }
        }

        public class HttpMethodToString
        {
            [Fact]
            public void ReturnsName()
            {
                string method = HttpMethod.Get;
                method.ShouldBe("GET");
            }

            [Fact]
            public void MatchesNameProperty()
            {
                string implicitResult = HttpMethod.Post;
                implicitResult.ShouldBe(HttpMethod.Post.Name);
            }
        }
    }

    public class ParseMethod
    {
        [Fact]
        public void ReturnsWellKnownInstance_ForKnownMethod()
        {
            HttpMethod.Parse("GET").ShouldBeSameAs(HttpMethod.Get);
        }

        [Fact]
        public void ReturnsSameInstance_OnSubsequentCalls()
        {
            var name = UniqueMethod();
            var first = HttpMethod.Parse(name);
            var second = HttpMethod.Parse(name);
            first.ShouldBeSameAs(second);
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            HttpMethod.Parse("get").ShouldBeSameAs(HttpMethod.Get);
        }

        [Fact]
        public void NormalizesToUppercase()
        {
            HttpMethod.Parse("patch").Name.ShouldBe("PATCH");
        }
    }

    public class RegisterMethod
    {
        [Fact]
        public void RegistersNewMethod()
        {
            var name = UniqueMethod();
            HttpMethod.Register(name);
            HttpMethod.Parse(name).Name.ShouldBe(name);
        }

        [Fact]
        public void HasNoEffect_WhenAlreadyRegistered()
        {
            var name = UniqueMethod();
            HttpMethod.Register(name);
            var first = HttpMethod.Parse(name);
            HttpMethod.Register(name);
            var second = HttpMethod.Parse(name);
            first.ShouldBeSameAs(second);
        }
    }

    public class AnyField
    {
        [Fact]
        public void HasWildcardName()
        {
            HttpMethod.Any.Name.ShouldBe("*");
        }

        [Fact]
        public void IsPreRegistered()
        {
            HttpMethod.Parse("*").ShouldBeSameAs(HttpMethod.Any);
        }
    }

    public class WellKnownFields
    {
        [Theory]
        [InlineData("CONNECT")]
        [InlineData("DELETE")]
        [InlineData("GET")]
        [InlineData("HEAD")]
        [InlineData("OPTIONS")]
        [InlineData("PATCH")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("TRACE")]
        public void WellKnownMethod_IsPreRegistered(string name)
        {
            HttpMethod.Parse(name).Name.ShouldBe(name);
        }

        [Fact]
        public void KnownCollection_ContainsAllWellKnownMethods()
        {
            HttpMethod.Known.ShouldContain(HttpMethod.Get);
            HttpMethod.Known.ShouldContain(HttpMethod.Post);
            HttpMethod.Known.ShouldContain(HttpMethod.Put);
            HttpMethod.Known.ShouldContain(HttpMethod.Delete);
            HttpMethod.Known.ShouldContain(HttpMethod.Any);
        }
    }
}