namespace Grapeseed.Tests;
using Grapevine;
using Shouldly;

public class HttpMethodTests
{
    public class Equivalent
    {
        private HttpMethod NewMethod = "NewMethod";

        [Fact]
        public void WhenEitherMethodIsAnyReturnTrue()
        {
            HttpMethod.Any.Equivalent(HttpMethod.Get).ShouldBeTrue();
            HttpMethod.Get.Equivalent(HttpMethod.Any).ShouldBeTrue();

            NewMethod.Equivalent(HttpMethod.Any).ShouldBeTrue();
            HttpMethod.Any.Equivalent(NewMethod).ShouldBeTrue();
        }

        [Fact]
        public void WhenBothMethodsAreSameReturnTrue()
        {
            NewMethod.Equivalent(NewMethod).ShouldBeTrue();
            HttpMethod.Post.Equivalent(HttpMethod.Post).ShouldBeTrue();
        }

        [Fact]
        public void WhenNeitherMethodIsAnyAndMethodsAreNotTheSameReturnFalse()
        {
            NewMethod.Equivalent(HttpMethod.Connect).ShouldBeFalse();
            HttpMethod.Delete.Equivalent(HttpMethod.Put).ShouldBeFalse();
        }

        [Fact]
        public void WhenMethodsImplictlyConvertThenTheyAreEquivalent()
        {
            HttpMethod post = "Post";
            HttpMethod.Post.Equivalent(post).ShouldBeTrue();
            HttpMethod.Get.Equivalent("Get").ShouldBeTrue();
        }
    }

}
