namespace Grapeseed.Tests;
using Grapevine;
using Xunit;

public class MultiPartBoundaryTests
{
    public class Generate
    {
        [Theory]
        [InlineData("exactly-thirty-characters-long")]
        [InlineData("between-thirty-and-seventy-characters-long")]
        [InlineData("exactly-seventy-characters-long---------------------------------------")]
        public void WhenPrefixLengthIsInBoundaryLengthRangeThenReturnsPrefix(string prefix)
        {
            prefix.Length.ShouldBeGreaterThanOrEqualTo(MultiPartBoundary.MIN_BOUNDARY_LENGTH);
            prefix.Length.ShouldBeLessThanOrEqualTo(MultiPartBoundary.MAX_BOUNDARY_LENGTH);
            MultiPartBoundary.Generate(prefix).ShouldBe(prefix);
        }

        [Fact]
        public void WhenPrefixLengthExceedsMaxBoundaryLengthThenReturnsPrefix()
        {
            var firstPart = "exceeds-seventy-characters-long-0123456789012345678901234567890123456789";
            firstPart.Length.ShouldBeGreaterThan(MultiPartBoundary.MAX_BOUNDARY_LENGTH);
            MultiPartBoundary.Generate(firstPart).ShouldBe(firstPart);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void WhenPrefixIsNullOrEmptyOrWhitespaceThenReturnsBoundaryUsingDefaultPrefix(string prefix)
        {
            var boundary = MultiPartBoundary.Generate(prefix);
            boundary.Length.ShouldBeGreaterThanOrEqualTo(MultiPartBoundary.MIN_BOUNDARY_LENGTH);
            boundary.Length.ShouldBeLessThanOrEqualTo(MultiPartBoundary.MAX_BOUNDARY_LENGTH);
            boundary.StartsWith(MultiPartBoundary.DEFAULT_BOUNDARY_PREFIX).ShouldBeTrue();
        }

        [Fact]
        public void WhenNoPrefixIsGivenThenReturnsBoundaryUsingDefaultPrefix()
        {
            var boundary = MultiPartBoundary.Generate();
            boundary.Length.ShouldBeGreaterThanOrEqualTo(MultiPartBoundary.MIN_BOUNDARY_LENGTH);
            boundary.Length.ShouldBeLessThanOrEqualTo(MultiPartBoundary.MAX_BOUNDARY_LENGTH);
            boundary.StartsWith(MultiPartBoundary.DEFAULT_BOUNDARY_PREFIX).ShouldBeTrue();
        }

        [Theory]
        [InlineData("MyCustomPrefix")]
        [InlineData(" MyCustomPrefix")]
        [InlineData("MyCustomPrefix ")]
        [InlineData(" MyCustomPrefix ")]
        public void WhenPrefixLengthIsLessThanMinBoundaryLengthThenReturnsBoundaryOfSufficientLengthUsingTrimmedPrefix(string prefix)
        {
            var boundary = MultiPartBoundary.Generate(prefix);
            boundary.Length.ShouldBeGreaterThanOrEqualTo(MultiPartBoundary.MIN_BOUNDARY_LENGTH);
            boundary.Length.ShouldBeLessThanOrEqualTo(MultiPartBoundary.MAX_BOUNDARY_LENGTH);
            boundary.StartsWith(prefix.Trim()).ShouldBeTrue();
        }
    }
}
