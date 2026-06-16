using Grapevine.Abstractions.RouteConstraints;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class DateTimeResolverTests
{
    public class Resolve
    {
        [Fact]
        public void ReturnsIsoPattern_WhenArgsIsNull()
        {
            var (pattern, _, _) = DateTimeResolver.Resolve("dt", null);
            pattern.ShouldBe(@"(?<dt>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2})");
        }

        [Fact]
        public void ReturnsIsoPattern_WhenArgsIsEmpty()
        {
            var (pattern, _, _) = DateTimeResolver.Resolve("dt", string.Empty);
            pattern.ShouldBe(@"(?<dt>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2})");
        }

        [Fact]
        public void ReturnsIsoPattern_WhenArgIsIso()
        {
            var (pattern, _, _) = DateTimeResolver.Resolve("dt", "iso");
            pattern.ShouldBe(@"(?<dt>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2})");
        }

        [Fact]
        public void ReturnsTimePattern_WhenArgIsTime()
        {
            var (pattern, _, _) = DateTimeResolver.Resolve("dt", "time");
            pattern.ShouldBe(@"(?<dt>\d{2}:\d{2}:\d{2})");
        }

        [Fact]
        public void ReturnsBasicPattern_WhenArgIsBasic()
        {
            var (pattern, _, _) = DateTimeResolver.Resolve("dt", "basic");
            pattern.ShouldBe(@"(?<dt>\d{8})");
        }

        [Fact]
        public void ReturnsRfcPattern_WhenArgIsRfc()
        {
            var (pattern, _, _) = DateTimeResolver.Resolve("dt", "rfc");
            pattern.ShouldBe(@"(?<dt>[A-Za-z]{3}, \d{2} [A-Za-z]{3} \d{4} \d{2}:\d{2}:\d{2} GMT)");
        }

        [Fact]
        public void IsCaseInsensitive_WhenArgIsUppercase()
        {
            var (pattern, _, _) = DateTimeResolver.Resolve("dt", "ISO");
            pattern.ShouldBe(@"(?<dt>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2})");
        }

        [Fact]
        public void ReturnsCorrectStrictness()
        {
            var (_, strictness, _) = DateTimeResolver.Resolve("dt", null);
            strictness.ShouldBe(DateTimeResolver.Strictness);
        }

        [Fact]
        public void ReturnsCorrectGroup()
        {
            var (_, _, group) = DateTimeResolver.Resolve("dt", null);
            group.ShouldBe(DateTimeResolver.Group);
        }

        [Fact]
        public void Throws_WhenArgIsUnknownFormat()
        {
            var format = Guid.NewGuid().ToString();
            var ex     = Should.Throw<ArgumentException>(() => DateTimeResolver.Resolve("dt", format));
            ex.Message.ShouldContain(format);
        }
    }
}