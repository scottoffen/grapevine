using Grapevine.Abstractions.RouteConstraints;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class ResolverRegistryTests
{
    public class TryResolveSegment
    {
        [Fact]
        public void ReturnsTrue_WhenSegmentIsBareParameter()
        {
            var result = ResolverRegistry.TryResolveSegment("{name}", out var descriptor);
            result.ShouldBeTrue();
            descriptor.ParameterName.ShouldBe("name");
            descriptor.ConstraintKey.ShouldBe("text");
            descriptor.ConstraintArgs.ShouldBeNull();
        }

        [Fact]
        public void ReturnsTrue_WhenSegmentHasConstraint()
        {
            var result = ResolverRegistry.TryResolveSegment("{id:int}", out var descriptor);
            result.ShouldBeTrue();
            descriptor.ParameterName.ShouldBe("id");
            descriptor.ConstraintKey.ShouldBe("int");
            descriptor.ConstraintArgs.ShouldBeNull();
        }

        [Fact]
        public void ReturnsTrue_WhenSegmentHasConstraintWithArgs()
        {
            var result = ResolverRegistry.TryResolveSegment("{id:int(1,5)}", out var descriptor);
            result.ShouldBeTrue();
            descriptor.ParameterName.ShouldBe("id");
            descriptor.ConstraintKey.ShouldBe("int");
            descriptor.ConstraintArgs.ShouldBe("1,5");
        }

        [Fact]
        public void ReturnsFalse_WhenSegmentIsLiteral()
        {
            var result = ResolverRegistry.TryResolveSegment("users", out var descriptor);
            result.ShouldBeFalse();
            descriptor.ShouldBe(default(SegmentDescriptor));
        }

        [Fact]
        public void ReturnsFalse_WhenSegmentIsEmpty()
        {
            var result = ResolverRegistry.TryResolveSegment(string.Empty, out _);
            result.ShouldBeFalse();
        }

        [Fact]
        public void PopulatesDescriptorPattern_WhenResolved()
        {
            ResolverRegistry.TryResolveSegment("{id:guid}", out var descriptor);
            descriptor.Pattern.ShouldNotBeNullOrWhiteSpace();
            descriptor.IsParameter.ShouldBeTrue();
        }

        [Fact]
        public void Throws_WhenConstraintKeyIsUnregistered()
        {
            var key = Guid.NewGuid().ToString();
            Should.Throw<ArgumentException>(() =>
                ResolverRegistry.TryResolveSegment($"{{id:{key}}}", out _));
        }
    }

    public class RegisterResolver
    {
        [Fact]
        public void RegistersResolver_WhenKeyIsNew()
        {
            var key = Guid.NewGuid().ToString();
            ResolverRegistry.RegisterResolver(key, (name, args) => ("pattern", 50, 0));
            ResolverRegistry.TryResolveSegment($"{{id:{key}}}", out var descriptor);
            descriptor.Pattern.ShouldBe("pattern");
        }

        [Fact]
        public void Throws_WhenKeyIsAlreadyRegistered()
        {
            var key = Guid.NewGuid().ToString();
            ResolverRegistry.RegisterResolver(key, (name, args) => ("pattern", 50, 0));
            Should.Throw<InvalidOperationException>(() =>
                ResolverRegistry.RegisterResolver(key, (name, args) => ("other", 50, 0)));
        }

        [Fact]
        public void Throws_WhenKeyIsNull()
        {
            Should.Throw<ArgumentException>(() =>
                ResolverRegistry.RegisterResolver(null!, (name, args) => ("pattern", 50, 0)));
        }

        [Fact]
        public void Throws_WhenKeyIsWhitespace()
        {
            Should.Throw<ArgumentException>(() =>
                ResolverRegistry.RegisterResolver("   ", (name, args) => ("pattern", 50, 0)));
        }

        [Fact]
        public void Throws_WhenResolverIsNull()
        {
            var key = Guid.NewGuid().ToString();
            Should.Throw<ArgumentNullException>(() =>
                ResolverRegistry.RegisterResolver(key, null!));
        }
    }

    public class OverrideResolver
    {
        [Fact]
        public void AddsResolver_WhenKeyIsNew()
        {
            var key = Guid.NewGuid().ToString();
            ResolverRegistry.OverrideResolver(key, (name, args) => ("pattern", 50, 0));
            ResolverRegistry.TryResolveSegment($"{{id:{key}}}", out var descriptor);
            descriptor.Pattern.ShouldBe("pattern");
        }

        [Fact]
        public void ReplacesResolver_WhenKeyAlreadyExists()
        {
            var key = Guid.NewGuid().ToString();
            ResolverRegistry.RegisterResolver(key, (name, args) => ("original", 50, 0));
            ResolverRegistry.OverrideResolver(key, (name, args) => ("replaced", 50, 0));
            ResolverRegistry.TryResolveSegment($"{{id:{key}}}", out var descriptor);
            descriptor.Pattern.ShouldBe("replaced");
        }

        [Fact]
        public void Throws_WhenKeyIsNull()
        {
            Should.Throw<ArgumentException>(() =>
                ResolverRegistry.OverrideResolver(null!, (name, args) => ("pattern", 50, 0)));
        }

        [Fact]
        public void Throws_WhenKeyIsWhitespace()
        {
            Should.Throw<ArgumentException>(() =>
                ResolverRegistry.OverrideResolver("   ", (name, args) => ("pattern", 50, 0)));
        }

        [Fact]
        public void Throws_WhenResolverIsNull()
        {
            var key = Guid.NewGuid().ToString();
            Should.Throw<ArgumentNullException>(() =>
                ResolverRegistry.OverrideResolver(key, null!));
        }
    }
}