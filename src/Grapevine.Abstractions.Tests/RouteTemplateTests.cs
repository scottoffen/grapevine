using Grapevine.Abstractions.RouteConstraints;
using Shouldly;

namespace Grapevine.Abstractions.Tests;

public class RouteTemplateTests
{
    public class Parse
    {
        [Fact]
        public void ParsesLiteralSegments()
        {
            var template = RouteTemplate.Parse("/users/orders");
            template.Segments.Length.ShouldBe(2);
            template.Segments[0].IsParameter.ShouldBeFalse();
            template.Segments[1].IsParameter.ShouldBeFalse();
        }

        [Fact]
        public void ParsesParameterSegments()
        {
            var template = RouteTemplate.Parse("/users/{id:int}");
            template.Segments.Length.ShouldBe(2);
            template.Segments[1].IsParameter.ShouldBeTrue();
            template.Segments[1].ParameterName.ShouldBe("id");
            template.Segments[1].ConstraintKey.ShouldBe("int");
        }

        [Fact]
        public void NormalizesLeadingSlash_WhenAbsent()
        {
            var template = RouteTemplate.Parse("users/{id}");
            template.RawTemplate.ShouldBe("/users/{id}");
        }

        [Fact]
        public void NormalizesTrailingSlash_WhenPresent()
        {
            var template = RouteTemplate.Parse("/users/{id}/");
            template.RawTemplate.ShouldBe("/users/{id}");
        }

        [Fact]
        public void ProducesEquivalentTemplate_WhenLeadingSlashDiffers()
        {
            var a = RouteTemplate.Parse("/users/{id}");
            var b = RouteTemplate.Parse("users/{id}");
            a.RawTemplate.ShouldBe(b.RawTemplate);
        }

        [Fact]
        public void SetsParameterCount()
        {
            var template = RouteTemplate.Parse("/users/{id:int}/orders/{orderId:guid}");
            template.ParameterCount.ShouldBe(2);
        }

        [Fact]
        public void SetsSegmentCount()
        {
            var template = RouteTemplate.Parse("/users/{id}/orders");
            template.SegmentCount.ShouldBe(3);
        }

        [Fact]
        public void CompilesRegex()
        {
            var template = RouteTemplate.Parse("/users/{id:int}");
            template.CompiledRegex.ShouldNotBeNull();
            template.CompiledRegex.IsMatch("/users/42").ShouldBeTrue();
            template.CompiledRegex.IsMatch("/users/abc").ShouldBeFalse();
        }

        [Fact]
        public void PopulatesCaptureGroupNames_ForParameterSegments()
        {
            var template = RouteTemplate.Parse("/users/{id:int}/orders/{orderId:guid}");
            template.CaptureGroupNames.ShouldContain("id");
            template.CaptureGroupNames.ShouldContain("orderId");
        }

        [Fact]
        public void PopulatesCaptureGroupNames_IncludingInnerRegexGroups()
        {
            var template = RouteTemplate.Parse("/users/{id:regex((?<year>[0-9]{4})-(?<month>[0-9]{2}))}");
            template.CaptureGroupNames.ShouldContain("id");
            template.CaptureGroupNames.ShouldContain("year");
            template.CaptureGroupNames.ShouldContain("month");
        }

        [Fact]
        public void DoesNotIncludeNumericGroupNames_InCaptureGroupNames()
        {
            var template = RouteTemplate.Parse("/users/{id:int}");
            foreach (var name in template.CaptureGroupNames)
                int.TryParse(name, out _).ShouldBeFalse();
        }

        [Fact]
        public void Throws_WhenTemplateIsNull()
        {
            Should.Throw<ArgumentException>(() => RouteTemplate.Parse(null!));
        }

        [Fact]
        public void Throws_WhenTemplateIsEmpty()
        {
            Should.Throw<ArgumentException>(() => RouteTemplate.Parse(string.Empty));
        }

        [Fact]
        public void Throws_WhenTemplateIsWhitespace()
        {
            Should.Throw<ArgumentException>(() => RouteTemplate.Parse("   "));
        }

        [Fact]
        public void Throws_WhenConstraintIsUnregistered()
        {
            var key = Guid.NewGuid().ToString();
            Should.Throw<ArgumentException>(() => RouteTemplate.Parse($"/users/{{id:{key}}}"));
        }

        [Fact]
        public void Throws_WhenDuplicateParameterNameExists()
        {
            var ex = Should.Throw<ArgumentException>(() =>
                RouteTemplate.Parse("/users/{id:int}/orders/{id:guid}"));
            ex.Message.ShouldContain("id");
        }

        [Fact]
        public void Throws_WhenInnerRegexGroupNameDuplicatesParameterName()
        {
            // The inner named group 'other' in the second segment conflicts with
            // the parameter name 'other' in the first segment.
            var ex = Should.Throw<ArgumentException>(() =>
                RouteTemplate.Parse("/users/{other:int}/orders/{id:regex((?<other>[0-9]+))}"));
            ex.Message.ShouldContain("other");
        }

        [Fact]
        public void ThrowsWithExpectedMessage_WhenDuplicateParameterNameExists()
        {
            var ex = Should.Throw<ArgumentException>(() =>
                RouteTemplate.Parse("/users/{id:int}/orders/{id:guid}"));
            ex.Message.ShouldContain(
                string.Format(RouteTemplate.DuplicateParameterNameMessage, "/users/{id:int}/orders/{id:guid}", "id"));
        }
    }

    public class GetRouteParams
    {
        [Fact]
        public void ReturnsPopulatedCollection_WhenPathMatches()
        {
            var template = RouteTemplate.Parse("/users/{id:int}");
            var result   = template.GetRouteParams("/users/42");
            result["id"].ShouldBe("42");
        }

        [Fact]
        public void ReturnsEmptyCollection_WhenPathDoesNotMatch()
        {
            var template = RouteTemplate.Parse("/users/{id:int}");
            var result   = template.GetRouteParams("/orders/42");
            result.Count.ShouldBe(0);
        }

        [Fact]
        public void ReturnsEmptyCollection_WhenPathFailsConstraint()
        {
            var template = RouteTemplate.Parse("/users/{id:int}");
            var result   = template.GetRouteParams("/users/abc");
            result.Count.ShouldBe(0);
        }

        [Fact]
        public void ReturnsAllParameters_WhenMultipleParameterSegments()
        {
            var template = RouteTemplate.Parse("/users/{userId:int}/orders/{orderId:guid}");
            var result   = template.GetRouteParams("/users/42/orders/3f2504e0-4f89-11d3-9a0c-0305e82c3301");
            result["userId"].ShouldBe("42");
            result["orderId"].ShouldBe("3f2504e0-4f89-11d3-9a0c-0305e82c3301");
        }

        [Fact]
        public void ReturnsInnerCaptureGroups_WhenRegexConstraintHasNamedGroups()
        {
            var template = RouteTemplate.Parse("/dates/{date:regex((?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2}))}");
            var result   = template.GetRouteParams("/dates/2023-05-21");
            result["date"].ShouldBe("2023-05-21");
            result["year"].ShouldBe("2023");
            result["month"].ShouldBe("05");
            result["day"].ShouldBe("21");
        }

        [Fact]
        public void ReturnsEmptyCollection_WhenTemplateHasNoParameters()
        {
            var template = RouteTemplate.Parse("/users/orders");
            var result   = template.GetRouteParams("/users/orders");
            result.Count.ShouldBe(0);
        }
    }

    public class CompiledRegexTests
    {
        [Fact]
        public void MatchesLiteralSegment_CaseInsensitively()
        {
            var template = RouteTemplate.Parse("/users/orders");
            template.CompiledRegex.IsMatch("/USERS/ORDERS").ShouldBeTrue();
            template.CompiledRegex.IsMatch("/Users/Orders").ShouldBeTrue();
        }

        [Fact]
        public void MatchesBoolConstraint_CaseInsensitively()
        {
            var template = RouteTemplate.Parse("/toggle/{flag:bool}");
            template.CompiledRegex.IsMatch("/toggle/True").ShouldBeTrue();
            template.CompiledRegex.IsMatch("/toggle/FALSE").ShouldBeTrue();
        }

        [Fact]
        public void MatchesGuidConstraint_CaseInsensitively()
        {
            var template = RouteTemplate.Parse("/items/{id:guid}");
            template.CompiledRegex.IsMatch("/items/3F2504E0-4F89-11D3-9A0C-0305E82C3301").ShouldBeTrue();
        }
    }

    public class CompareTo
    {
        [Fact]
        public void LongerTemplateSortsFirst_WhenSharedSegmentsAreEqual()
        {
            var longer  = RouteTemplate.Parse("/users/{id}/orders/items");
            var shorter = RouteTemplate.Parse("/users/{id}/orders");
            longer.CompareTo(shorter, false).ShouldBeLessThan(0);
        }

        [Fact]
        public void ShorterTemplateSortsAfter_WhenSharedSegmentsAreEqual()
        {
            var longer  = RouteTemplate.Parse("/users/{id}/orders/items");
            var shorter = RouteTemplate.Parse("/users/{id}/orders");
            shorter.CompareTo(longer, false).ShouldBeGreaterThan(0);
        }

        [Fact]
        public void FirstNonZeroSegmentWins_WhenSegmentsDiffer()
        {
            var a = RouteTemplate.Parse("/users/orders/{id}");
            var b = RouteTemplate.Parse("/users/{id}/orders");

            // 'orders' (literal) sorts before '{id}' (parameter) at position 1,
            // so 'a' should sort before 'b'.
            a.CompareTo(b, false).ShouldBeLessThan(0);
        }

        [Fact]
        public void ReturnsZero_WhenTemplatesAreIdentical()
        {
            var a = RouteTemplate.Parse("/users/{id:int}");
            var b = RouteTemplate.Parse("/users/{id:int}");
            a.CompareTo(b, false).ShouldBe(0);
        }

        [Fact]
        public void PassesIgnoreGroupConflictsToSegmentComparison()
        {
            // Two templates with same-group constraints on the same position.
            // With enforcing (false): both segments return 0, templates return 0.
            // With ignoring (true): stricter constraint wins.
            var a = RouteTemplate.Parse("/users/{id:int}");
            var b = RouteTemplate.Parse("/users/{id:numeric}");

            a.CompareTo(b, false).ShouldBe(0);
            a.CompareTo(b, true).ShouldBeLessThan(0);
        }
    }

    public class EqualsTests
    {
        [Fact]
        public void ReturnsTrue_WhenRawTemplatesAreEqual()
        {
            var a = RouteTemplate.Parse("/users/{id}");
            var b = RouteTemplate.Parse("/users/{id}");
            a.Equals(b).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenRawTemplatesDiffer()
        {
            var a = RouteTemplate.Parse("/users/{id}");
            var b = RouteTemplate.Parse("/orders/{id}");
            a.Equals(b).ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var a = RouteTemplate.Parse("/Users/{id}");
            var b = RouteTemplate.Parse("/users/{id}");
            a.Equals(b).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenOtherIsNull()
        {
            var a = RouteTemplate.Parse("/users/{id}");
            a.Equals(null).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsTrue_WhenSameReference()
        {
            var a = RouteTemplate.Parse("/users/{id}");
            a.Equals(a).ShouldBeTrue();
        }
    }

    public class GetHashCodeTests
    {
        [Fact]
        public void ReturnsSameHashCode_WhenTemplatesAreEqual()
        {
            var a = RouteTemplate.Parse("/users/{id}");
            var b = RouteTemplate.Parse("/users/{id}");
            a.GetHashCode().ShouldBe(b.GetHashCode());
        }

        [Fact]
        public void ReturnsSameHashCode_WhenTemplatesDifferOnlyByCase()
        {
            var a = RouteTemplate.Parse("/Users/{id}");
            var b = RouteTemplate.Parse("/users/{id}");
            a.GetHashCode().ShouldBe(b.GetHashCode());
        }
    }
}