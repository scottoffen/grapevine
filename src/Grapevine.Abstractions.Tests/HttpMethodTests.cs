using HttpMethod = Grapevine.HttpMethod;

namespace Grapevine.Abstractions.Tests;

public class HttpMethodTests
{
    private static string UniqueMethod() => Guid.NewGuid().ToString("N").ToUpper();

    public class Constructor
    {
        [Fact]
        public void SetsMethodName()
        {
            var method = new HttpMethod("PROPFIND");
            method.Method.ShouldBe("PROPFIND");
        }
    }

    public class AnyField
    {
        [Fact]
        public void HasCorrectMethodName()
        {
            HttpMethod.Any.Method.ShouldBe("Any");
        }
    }

    public class ToStringMethod
    {
        [Fact]
        public void ReturnsMethodName()
        {
            var method = new HttpMethod("GET");
            method.ToString().ShouldBe("GET");
        }
    }

    public class EqualsMethod
    {
        [Fact]
        public void ReturnsTrue_ForEqualHttpMethods()
        {
            var a = new HttpMethod("GET");
            var b = new HttpMethod("GET");
            a.Equals(b).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_ForEqualHttpMethods_DifferentCase()
        {
            var a = new HttpMethod("get");
            var b = new HttpMethod("GET");
            a.Equals(b).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenComparedToMatchingString()
        {
            var method = new HttpMethod("GET");
            method.Equals("GET").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenComparedToMatchingString_DifferentCase()
        {
            var method = new HttpMethod("GET");
            method.Equals("get").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenComparedToMatchingSystemHttpMethod()
        {
            var method = new HttpMethod("GET");
            method.Equals(System.Net.Http.HttpMethod.Get).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_ForDifferentMethods()
        {
            var a = new HttpMethod("GET");
            var b = new HttpMethod("POST");
            a.Equals(b).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_ForUnrelatedType()
        {
            var method = new HttpMethod("GET");
            method.Equals(42).ShouldBeFalse();
        }
    }

    public class GetHashCodeMethod
    {
        [Fact]
        public void IsConsistentWithEquality_ForEqualInstances()
        {
            var a = new HttpMethod("GET");
            var b = new HttpMethod("get");
            a.GetHashCode().ShouldBe(b.GetHashCode());
        }
    }

    public class EqualityOperators
    {
        public class HttpMethodVsHttpMethod
        {
            [Fact]
            public void ReturnsTrue_WhenMethodsAreEqual()
            {
                var a = new HttpMethod("GET");
                var b = new HttpMethod("GET");
                (a == b).ShouldBeTrue();
            }

            [Fact]
            public void ReturnsTrue_WhenMethodsAreEqual_DifferentCase()
            {
                var a = new HttpMethod("get");
                var b = new HttpMethod("GET");
                (a == b).ShouldBeTrue();
            }

            [Fact]
            public void ReturnsFalse_WhenMethodsDiffer()
            {
                var a = new HttpMethod("GET");
                var b = new HttpMethod("POST");
                (a == b).ShouldBeFalse();
            }

            [Fact]
            public void InequalityReturnsTrue_WhenMethodsDiffer()
            {
                var a = new HttpMethod("GET");
                var b = new HttpMethod("POST");
                (a != b).ShouldBeTrue();
            }
        }

        public class HttpMethodVsString
        {
            [Fact]
            public void ReturnsTrue_WhenStringMatches()
            {
                (HttpMethod.FromMethod("GET") == "GET").ShouldBeTrue();
            }

            [Fact]
            public void ReturnsTrue_WhenStringMatches_DifferentCase()
            {
                (HttpMethod.FromMethod("GET") == "get").ShouldBeTrue();
            }

            [Fact]
            public void ReturnsFalse_WhenStringDoesNotMatch()
            {
                (HttpMethod.FromMethod("GET") == "POST").ShouldBeFalse();
            }

            [Fact]
            public void InequalityReturnsTrue_WhenStringDoesNotMatch()
            {
                (HttpMethod.FromMethod("GET") != "POST").ShouldBeTrue();
            }
        }

        public class StringVsHttpMethod
        {
            [Fact]
            public void ReturnsTrue_WhenStringMatches()
            {
                ("GET" == HttpMethod.FromMethod("GET")).ShouldBeTrue();
            }

            [Fact]
            public void ReturnsFalse_WhenStringDoesNotMatch()
            {
                ("GET" == HttpMethod.FromMethod("POST")).ShouldBeFalse();
            }

            [Fact]
            public void InequalityReturnsTrue_WhenStringDoesNotMatch()
            {
                ("GET" != HttpMethod.FromMethod("POST")).ShouldBeTrue();
            }
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
                method.Method.ShouldBe("GET");
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
                method.Method.ShouldBe(name);
            }
        }

        public class HttpMethodToString
        {
            [Fact]
            public void ReturnsMethodName()
            {
                string method = HttpMethod.FromMethod("GET");
                method.ShouldBe("GET");
            }

            [Fact]
            public void MatchesMethodProperty()
            {
                var method = HttpMethod.FromMethod("POST");
                string implicitResult = method;
                implicitResult.ShouldBe(method.Method);
            }
        }

        public class SystemHttpMethodToHttpMethod
        {
            [Fact]
            public void ReturnsCorrectInstance()
            {
                HttpMethod method = System.Net.Http.HttpMethod.Get;
                method.Method.ShouldBe("GET");
            }

            [Fact]
            public void ReturnsSameInstance_AsFromMethod()
            {
                HttpMethod fromConversion = System.Net.Http.HttpMethod.Get;
                HttpMethod fromLookup = HttpMethod.FromMethod("GET");
                fromConversion.ShouldBeSameAs(fromLookup);
            }
        }

        public class HttpMethodToSystemHttpMethod
        {
            [Fact]
            public void ReturnsSystemHttpMethodWithCorrectName()
            {
                System.Net.Http.HttpMethod method = HttpMethod.FromMethod("GET");
                method.Method.ShouldBe("GET");
            }
        }
    }

    public class EquivalentMethod
    {
        [Fact]
        public void ReturnsTrue_WhenThisIsAny()
        {
            HttpMethod.Any.Equivalent(HttpMethod.FromMethod("GET")).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenOtherIsAny()
        {
            HttpMethod.FromMethod("GET").Equivalent(HttpMethod.Any).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_WhenBothMethodsAreEqual()
        {
            var a = HttpMethod.FromMethod("GET");
            var b = HttpMethod.FromMethod("GET");
            a.Equivalent(b).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenMethodsDifferAndNeitherIsAny()
        {
            var get = HttpMethod.FromMethod("GET");
            var post = HttpMethod.FromMethod("POST");
            get.Equivalent(post).ShouldBeFalse();
        }
    }

    public class FromMethodMethod
    {
        [Fact]
        public void ReturnsPreRegisteredInstance_ForBaseClassMethod()
        {
            var first = HttpMethod.FromMethod("GET");
            var second = HttpMethod.FromMethod("GET");
            first.ShouldBeSameAs(second);
        }

        [Fact]
        public void ReturnsPreRegisteredInstance_ForAny()
        {
            HttpMethod.FromMethod("Any").ShouldBeSameAs(HttpMethod.Any);
        }

        [Fact]
        public void RegistersAndReturnsNewInstance_ForUnknownMethod()
        {
            var name = UniqueMethod();
            var result = HttpMethod.FromMethod(name);
            result.Method.ShouldBe(name);
        }

        [Fact]
        public void ReturnsSameInstance_OnSubsequentCalls()
        {
            var name = UniqueMethod();
            var first = HttpMethod.FromMethod(name);
            var second = HttpMethod.FromMethod(name);
            first.ShouldBeSameAs(second);
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var lower = HttpMethod.FromMethod("get");
            var upper = HttpMethod.FromMethod("GET");
            lower.ShouldBeSameAs(upper);
        }
    }

    public class RegisterMethod
    {
        [Fact]
        public void RegistersNewMethod()
        {
            var name = UniqueMethod();
            HttpMethod.Register(name);
            HttpMethod.FromMethod(name).Method.ShouldBe(name);
        }

        [Fact]
        public void HasNoEffect_WhenAlreadyRegistered()
        {
            var name = UniqueMethod();
            HttpMethod.Register(name);
            var first = HttpMethod.FromMethod(name);
            HttpMethod.Register(name);
            var second = HttpMethod.FromMethod(name);
            first.ShouldBeSameAs(second);
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var name = UniqueMethod();
            HttpMethod.Register(name.ToLower());
            HttpMethod.FromMethod(name.ToUpper()).Method.ShouldBe(name.ToLower());
        }
    }

    public class StaticRegistry
    {
        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("HEAD")]
        [InlineData("OPTIONS")]
        [InlineData("PATCH")]
        [InlineData("TRACE")]
        public void BaseClassMethod_IsPreRegistered(string methodName)
        {
            var result = HttpMethod.FromMethod(methodName);
            result.Method.ShouldBe(methodName);
        }

        [Fact]
        public void Any_IsPreRegistered()
        {
            HttpMethod.FromMethod("Any").ShouldBeSameAs(HttpMethod.Any);
        }
    }
}