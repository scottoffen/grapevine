using Grapevine.Abstractions.RouteConstraints;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class DateResolverTests
{
    public class Resolve
    {
        [Fact]
        public void ReturnsIsoPattern_WhenArgsIsNull()
        {
            var (pattern, _, _) = DateResolver.Resolve("d", null);
            pattern.ShouldBe(@"(?<d>\d{4}-\d{2}-\d{2})");
        }

        [Fact]
        public void ReturnsIsoPattern_WhenArgsIsEmpty()
        {
            var (pattern, _, _) = DateResolver.Resolve("d", string.Empty);
            pattern.ShouldBe(@"(?<d>\d{4}-\d{2}-\d{2})");
        }

        [Fact]
        public void ReturnsIsoPattern_WhenArgIsIso()
        {
            var (pattern, _, _) = DateResolver.Resolve("d", "iso");
            pattern.ShouldBe(@"(?<d>\d{4}-\d{2}-\d{2})");
        }

        [Fact]
        public void ReturnsYmdPattern_WhenArgIsYmd()
        {
            var (pattern, _, _) = DateResolver.Resolve("d", "ymd");
            pattern.ShouldBe(@"(?<d>\d{4}[-/]\d{2}[-/]\d{2})");
        }

        [Fact]
        public void ReturnsMdyPattern_WhenArgIsMdy()
        {
            var (pattern, _, _) = DateResolver.Resolve("d", "mdy");
            pattern.ShouldBe(@"(?<d>\d{1,2}[-/]\d{1,2}[-/]\d{4})");
        }

        [Fact]
        public void ReturnsDmyPattern_WhenArgIsDmy()
        {
            var (pattern, _, _) = DateResolver.Resolve("d", "dmy");
            pattern.ShouldBe(@"(?<d>\d{1,2}[-/]\d{1,2}[-/]\d{4})");
        }

        [Fact]
        public void ReturnsBasicPattern_WhenArgIsBasic()
        {
            var (pattern, _, _) = DateResolver.Resolve("d", "basic");
            pattern.ShouldBe(@"(?<d>\d{8})");
        }

        [Fact]
        public void IsCaseInsensitive_WhenArgIsUppercase()
        {
            var (pattern, _, _) = DateResolver.Resolve("d", "ISO");
            pattern.ShouldBe(@"(?<d>\d{4}-\d{2}-\d{2})");
        }

        [Fact]
        public void ReturnsCorrectStrictness()
        {
            var (_, strictness, _) = DateResolver.Resolve("d", null);
            strictness.ShouldBe(DateResolver.Strictness);
        }

        [Fact]
        public void ReturnsCorrectGroup()
        {
            var (_, _, group) = DateResolver.Resolve("d", null);
            group.ShouldBe(DateResolver.Group);
        }

        [Fact]
        public void Throws_WhenArgIsUnknownFormat()
        {
            var format = Guid.NewGuid().ToString();
            var ex     = Should.Throw<ArgumentException>(() => DateResolver.Resolve("d", format));
            ex.Message.ShouldContain(format);
        }
    }
}