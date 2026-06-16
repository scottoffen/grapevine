namespace Grapevine.Abstractions.Tests;

public class HttpStatusCodeTests
{
    public class Constructor_Value
    {
        [Fact]
        public void SetsNumericValue()
        {
            var code = new HttpStatusCode(299);
            ((int)code).ShouldBe(299);
        }

        [Fact]
        public void FormatsMessageWithNoColonOrText()
        {
            var code = new HttpStatusCode(299);
            code.ToString().ShouldBe("299");
        }
    }

    public class Constructor_ValueAndText
    {
        [Fact]
        public void SetsNumericValue()
        {
            var code = new HttpStatusCode(299, "Some Status");
            ((int)code).ShouldBe(299);
        }

        [Fact]
        public void FormatsMessageWithColonAndText()
        {
            var code = new HttpStatusCode(299, "Some Status");
            code.ToString().ShouldBe("299:Some Status");
        }

        [Fact]
        public void FormatsMessageWithNoColonOrText_WhenTextIsEmpty()
        {
            var code = new HttpStatusCode(299, string.Empty);
            code.ToString().ShouldBe("299");
        }

        [Fact]
        public void FormatsMessageWithNoColonOrText_WhenTextIsWhitespace()
        {
            var code = new HttpStatusCode(299, "   ");
            code.ToString().ShouldBe("299");
        }
    }

    public class ImplicitConversion_IntToHttpStatusCode
    {
        [Fact]
        public void ReturnsWellKnownInstance_ForKnownCode()
        {
            HttpStatusCode code = 200;
            code.ShouldBeSameAs(HttpStatusCode.Ok);
        }

        [Fact]
        public void ReturnsNewInstance_ForUnknownCode()
        {
            HttpStatusCode code = 998;
            ((int)code).ShouldBe(998);
        }
    }

    public class ImplicitConversion_HttpStatusCodeToInt
    {
        [Fact]
        public void ReturnsNumericValue()
        {
            int value = HttpStatusCode.NotFound;
            value.ShouldBe(404);
        }
    }

    public class ImplicitConversion_HttpStatusCodeToString
    {
        [Fact]
        public void ReturnsFormattedMessage()
        {
            string message = HttpStatusCode.NotFound;
            message.ShouldBe("404:Not Found");
        }

        [Fact]
        public void ReturnsValueOnly_WhenNoTextPresent()
        {
            string message = new HttpStatusCode(299);
            message.ShouldBe("299");
        }

        [Fact]
        public void MatchesToString()
        {
            string implicitResult = HttpStatusCode.NotFound;
            implicitResult.ShouldBe(HttpStatusCode.NotFound.ToString());
        }
    }

    public class ToStringMethod
    {
        [Fact]
        public void MatchesImplicitStringConversion()
        {
            var code = HttpStatusCode.InternalServerError;
            code.ToString().ShouldBe((string)code);
        }
    }

    public class FromCode
    {
        [Fact]
        public void ReturnsWellKnownInstance_ForKnownCode()
        {
            HttpStatusCode.FromCode(404).ShouldBeSameAs(HttpStatusCode.NotFound);
        }

        [Fact]
        public void RegistersAndReturnsNewInstance_ForUnknownCode()
        {
            var code = HttpStatusCode.FromCode(997);
            ((int)code).ShouldBe(997);
        }

        [Fact]
        public void ReturnsSameInstance_OnSubsequentCallsForSameCode()
        {
            var first = HttpStatusCode.FromCode(996);
            var second = HttpStatusCode.FromCode(996);
            first.ShouldBeSameAs(second);
        }
    }

    public class Register
    {
        [Fact]
        public void RegistersNewCode_WithText()
        {
            HttpStatusCode.Register(991, "Custom Status");
            string message = HttpStatusCode.FromCode(991);
            message.ShouldBe("991:Custom Status");
        }

        [Fact]
        public void RegistersNewCode_WithoutText()
        {
            HttpStatusCode.Register(992);
            string message = HttpStatusCode.FromCode(992);
            message.ShouldBe("992");
        }

        [Fact]
        public void HasNoEffect_WhenCodeIsAlreadyRegistered()
        {
            HttpStatusCode.Register(993, "First");
            HttpStatusCode.Register(993, "Second");
            string message = HttpStatusCode.FromCode(993);
            message.ShouldBe("993:First");
        }
    }

    public class WellKnownStaticFields
    {
        [Fact]
        public void Informational_Continue_HasCorrectValue()
        {
            ((int)HttpStatusCode.Continue).ShouldBe(100);
            ((string)HttpStatusCode.Continue).ShouldBe("100:Continue");
        }

        [Fact]
        public void Success_Ok_HasCorrectValue()
        {
            ((int)HttpStatusCode.Ok).ShouldBe(200);
            ((string)HttpStatusCode.Ok).ShouldBe("200:Ok");
        }

        [Fact]
        public void Redirection_MovedPermanently_HasCorrectValue()
        {
            ((int)HttpStatusCode.MovedPermanently).ShouldBe(301);
            ((string)HttpStatusCode.MovedPermanently).ShouldBe("301:Moved Permanently");
        }

        [Fact]
        public void ClientError_NotFound_HasCorrectValue()
        {
            ((int)HttpStatusCode.NotFound).ShouldBe(404);
            ((string)HttpStatusCode.NotFound).ShouldBe("404:Not Found");
        }

        [Fact]
        public void ServerError_InternalServerError_HasCorrectValue()
        {
            ((int)HttpStatusCode.InternalServerError).ShouldBe(500);
            ((string)HttpStatusCode.InternalServerError).ShouldBe("500:Internal Server Error");
        }
    }
}