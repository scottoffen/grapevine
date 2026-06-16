namespace Grapevine.Abstractions.Tests;

public class HeaderAttributeTests
{
    public class Constructor
    {
        public class KeyOnly
        {
            [Fact]
            public void SetsKey()
            {
                var attr = new HeaderAttribute("Accept");
                attr.Key.ShouldBe("Accept");
            }

            [Fact]
            public void ValueMatchesAnyString()
            {
                var attr = new HeaderAttribute("Accept");
                attr.Value.IsMatch("application/json").ShouldBeTrue();
                attr.Value.IsMatch("text/html").ShouldBeTrue();
                attr.Value.IsMatch(string.Empty).ShouldBeTrue();
            }
        }

        public class WithPattern
        {
            [Fact]
            public void SetsKey()
            {
                var attr = new HeaderAttribute("Accept", pattern: @"application/.*");
                attr.Key.ShouldBe("Accept");
            }

            [Fact]
            public void MatchesValuesMatchingPattern()
            {
                var attr = new HeaderAttribute("Accept", pattern: @"application/.*");
                attr.Value.IsMatch("application/json").ShouldBeTrue();
                attr.Value.IsMatch("application/xml").ShouldBeTrue();
            }

            [Fact]
            public void DoesNotMatch_ValuesNotMatchingPattern()
            {
                var attr = new HeaderAttribute("Accept", pattern: @"application/.*");
                attr.Value.IsMatch("text/html").ShouldBeFalse();
            }
        }

        public class WithExact
        {
            [Fact]
            public void SetsKey()
            {
                var attr = new HeaderAttribute("Accept", exact: "application/json");
                attr.Key.ShouldBe("Accept");
            }

            [Fact]
            public void MatchesExactValue()
            {
                var attr = new HeaderAttribute("Accept", exact: "application/json");
                attr.Value.IsMatch("application/json").ShouldBeTrue();
            }

            [Fact]
            public void DoesNotMatch_DifferentValue()
            {
                var attr = new HeaderAttribute("Accept", exact: "application/json");
                attr.Value.IsMatch("application/xml").ShouldBeFalse();
            }

            [Fact]
            public void EscapesSpecialRegexCharacters()
            {
                var attr = new HeaderAttribute("Content-Type", exact: "text/html; charset=utf-8");
                attr.Value.IsMatch("text/html; charset=utf-8").ShouldBeTrue();
                attr.Value.IsMatch("text/html  charset=utf-8").ShouldBeFalse();
            }
        }

        public class WithBothPatternAndExact
        {
            [Fact]
            public void ExactTakesPrecedence()
            {
                var attr = new HeaderAttribute("Accept", pattern: @"application/.*", exact: "application/json");
                attr.Value.IsMatch("application/json").ShouldBeTrue();
                attr.Value.IsMatch("application/xml").ShouldBeFalse();
            }
        }
    }
}