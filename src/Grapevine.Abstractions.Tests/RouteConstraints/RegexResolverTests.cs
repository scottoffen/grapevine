using Grapevine.Abstractions.RouteConstraints;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class RegexResolverTests : IDisposable
{
    // Clear the compiled regex cache before each test so that cross-test cache
    // pollution does not cause false positives or mask errors in cache behavior tests.
    public RegexResolverTests()
    {
        RegexResolver._cache.Clear();
    }

    public void Dispose()
    {
        RegexResolver._cache.Clear();
    }

    public class Resolve : IDisposable
    {
        public Resolve()
        {
            RegexResolver._cache.Clear();
        }

        public void Dispose()
        {
            RegexResolver._cache.Clear();
        }

        [Fact]
        public void ReturnsWrappedPattern_WhenPatternIsValid()
        {
            var (pattern, _, _) = RegexResolver.Resolve("val", "[0-9]+");
            pattern.ShouldBe("(?<val>[0-9]+)");
        }

        [Fact]
        public void ReturnsCorrectStrictness()
        {
            var (_, strictness, _) = RegexResolver.Resolve("val", "[0-9]+");
            strictness.ShouldBe(RegexResolver.Strictness);
        }

        [Fact]
        public void ReturnsCorrectGroup()
        {
            var (_, _, group) = RegexResolver.Resolve("val", "[0-9]+");
            group.ShouldBe(RegexResolver.Group);
        }

        [Fact]
        public void Throws_WhenArgsIsNull()
        {
            var ex = Should.Throw<ArgumentException>(() => RegexResolver.Resolve("val", null));
            ex.Message.ShouldContain(RegexResolver.EmptyPatternMessage);
        }

        [Fact]
        public void Throws_WhenArgsIsEmpty()
        {
            var ex = Should.Throw<ArgumentException>(() => RegexResolver.Resolve("val", string.Empty));
            ex.Message.ShouldContain(RegexResolver.EmptyPatternMessage);
        }

        [Fact]
        public void Throws_WhenPatternStartsWithCaret()
        {
            var ex = Should.Throw<ArgumentException>(() => RegexResolver.Resolve("val", "^[0-9]+"));
            ex.Message.ShouldContain(RegexResolver.AnchoredPatternMessage);
        }

        [Fact]
        public void Throws_WhenPatternEndsWithDollar()
        {
            var ex = Should.Throw<ArgumentException>(() => RegexResolver.Resolve("val", "[0-9]+$"));
            ex.Message.ShouldContain(RegexResolver.AnchoredPatternMessage);
        }

        [Fact]
        public void Throws_WhenPatternContainsUnnamedCaptureGroup()
        {
            var ex = Should.Throw<ArgumentException>(() => RegexResolver.Resolve("val", "([0-9]+)"));
            ex.Message.ShouldContain(RegexResolver.CaptureGroupsMessage);
        }

        [Fact]
        public void Throws_WhenPatternContainsNamedGroupMatchingParameterName()
        {
            var ex = Should.Throw<ArgumentException>(() => RegexResolver.Resolve("val", "(?<val>[0-9]+)"));
            ex.Message.ShouldContain(RegexResolver.DuplicateNameMessage);
        }

        [Fact]
        public void Throws_WhenPatternIsInvalidRegex()
        {
            var ex = Should.Throw<ArgumentException>(() => RegexResolver.Resolve("val", "[invalid"));
            ex.Message.ShouldContain(RegexResolver.InvalidPatternMessage);
        }

        [Fact]
        public void AllowsNonCapturingGroups()
        {
            var (pattern, _, _) = RegexResolver.Resolve("val", "(?:[0-9]+)");
            pattern.ShouldBe("(?<val>(?:[0-9]+))");
        }

        [Fact]
        public void AllowsNamedGroupsNotMatchingParameterName()
        {
            var (pattern, _, _) = RegexResolver.Resolve("val", "(?<year>[0-9]{4})");
            pattern.ShouldBe("(?<val>(?<year>[0-9]{4}))");
        }

        [Fact]
        public void CachesCompiledRegex_WhenSamePatternUsedTwice()
        {
            var pattern = $"[a-z]+-{Guid.NewGuid()}";

            RegexResolver.Resolve("a", pattern);
            var cached = RegexResolver._cache.TryGetValue(pattern, out var first);
            cached.ShouldBeTrue();

            RegexResolver.Resolve("b", pattern);
            RegexResolver._cache.TryGetValue(pattern, out var second);

            // Same compiled Regex instance should be reused.
            ReferenceEquals(first, second).ShouldBeTrue();
        }

        [Fact]
        public void AddsPatternToCache_AfterFirstCall()
        {
            var pattern = $"[a-z]+-{Guid.NewGuid()}";
            RegexResolver.Resolve("val", pattern);
            RegexResolver._cache.ContainsKey(pattern).ShouldBeTrue();
        }
    }
}