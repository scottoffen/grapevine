using Grapevine.Abstractions.RouteConstraints;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class GuidResolverTests
{
    public class Resolve
    {
        [Fact]
        public void ReturnsPattern_WhenArgsIsNull()
        {
            var (pattern, _, _) = GuidResolver.Resolve("id", null);
            pattern.ShouldBe(@"(?<id>[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})");
        }

        [Fact]
        public void ReturnsPattern_WhenArgsIsEmpty()
        {
            var (pattern, _, _) = GuidResolver.Resolve("id", string.Empty);
            pattern.ShouldBe(@"(?<id>[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})");
        }

        [Fact]
        public void ReturnsCorrectStrictness()
        {
            var (_, strictness, _) = GuidResolver.Resolve("id", null);
            strictness.ShouldBe(GuidResolver.Strictness);
        }

        [Fact]
        public void ReturnsCorrectGroup()
        {
            var (_, _, group) = GuidResolver.Resolve("id", null);
            group.ShouldBe(GuidResolver.Group);
        }

        [Fact]
        public void Throws_WhenArgsIsNonEmpty()
        {
            var ex = Should.Throw<ArgumentException>(() => GuidResolver.Resolve("id", "something"));
            ex.Message.ShouldContain(GuidResolver.NoArgumentsMessage);
        }
    }
}