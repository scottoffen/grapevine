using Grapevine.Abstractions.RouteConstraints;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests.RouteConstraints;

public class ResolverRegistryTests
{
    [Theory]
    [InlineData("{id}",              @"(?<id>[^/]+)")]
    [InlineData("{id:int}",          @"(?<id>-?\d+)")]
    [InlineData("{id:int(3)}",       @"(?<id>-?\d{3})")]
    [InlineData("{val:alpha(2)}",    @"(?<val>[a-zA-Z]{2})")]
    [InlineData("{when:date(basic)}", @"(?<when>\d{8})")]
    [InlineData("{v:double(1,3)}",   @"(?<v>[-]?\d+(?:\.\d{1,3})?(?:[eE][-+]?\d+)?)")]
    public void TryResolveSegment_ShouldReturnExpectedPattern_WhenFormatIsValid(string segment, string expected)
    {
        var success = ResolverRegistry.TryResolveSegment(segment, out var pattern);
        success.ShouldBeTrue();
        pattern.ShouldBe(expected);
    }

    [Theory]
    [InlineData("{id:float}",   @"(?<id>[-]?\d+(?:\.\d+)?(?:[eE][-+]?\d+)?)")]
    [InlineData("{id:long}",    @"(?<id>-?\d+)")]
    [InlineData("{id:len}",     @"(?<id>[^/]+)")]
    [InlineData("{id:numeric}", @"(?<id>\d+)")]
    public void TryResolveSegment_ShouldResolveAliases_ToExpectedPattern(string segment, string expected)
    {
        var success = ResolverRegistry.TryResolveSegment(segment, out var pattern);
        success.ShouldBeTrue();
        pattern.ShouldBe(expected);
    }

    [Theory]
    [InlineData("id:int")]
    [InlineData("{:int}")]
    [InlineData("{id:int(3}")]
    public void TryResolveSegment_ShouldReturnFalse_WhenFormatIsInvalid(string segment)
    {
        var success = ResolverRegistry.TryResolveSegment(segment, out var pattern);
        success.ShouldBeFalse();
        pattern.ShouldBeNull();
    }

    [Fact]
    public void TryResolveSegment_ShouldThrow_WhenConstraintIsUnknown()
    {
        var ex = Should.Throw<ArgumentException>(() =>
            ResolverRegistry.TryResolveSegment("{id:unknown}", out _));
        ex.Message.ShouldContain("No resolver registered for constraint 'unknown'");
        ex.Message.ShouldContain("Known constraints");
    }

    [Fact]
    public void RegisterResolver_ShouldAddNewResolver_WhenKeyIsUnique()
    {
        var key = $"custom-{Guid.NewGuid():N}";
        ResolverRegistry.RegisterResolver(key, (name, arg) => $"(?<{name}>custom)");
        var success = ResolverRegistry.TryResolveSegment($"{{x:{key}}}", out var pattern);
        success.ShouldBeTrue();
        pattern.ShouldBe("(?<x>custom)");
    }

    [Fact]
    public void RegisterResolver_ShouldThrow_WhenKeyIsAlreadyRegistered()
    {
        var ex = Should.Throw<InvalidOperationException>(() =>
            ResolverRegistry.RegisterResolver("int", (name, arg) => $"(?<{name}>override)"));
        ex.Message.ShouldContain("already registered");
    }

    [Fact]
    public void OverrideResolver_ShouldReplaceExistingResolver()
    {
        var key = $"override-{Guid.NewGuid():N}";
        ResolverRegistry.RegisterResolver(key, (name, arg) => $"(?<{name}>first)");
        ResolverRegistry.TryResolveSegment($"{{val:{key}}}", out var pattern1);
        pattern1.ShouldBe("(?<val>first)");

        ResolverRegistry.OverrideResolver(key, (name, arg) => $"(?<{name}>second)");
        ResolverRegistry.TryResolveSegment($"{{val:{key}}}", out var pattern2);
        pattern2.ShouldBe("(?<val>second)");
    }

    [Fact]
    public void OverrideResolver_ShouldAddResolver_WhenKeyDoesNotExist()
    {
        var key = $"new-{Guid.NewGuid():N}";
        ResolverRegistry.OverrideResolver(key, (name, arg) => $"(?<{name}>added)");
        var success = ResolverRegistry.TryResolveSegment($"{{x:{key}}}", out var pattern);
        success.ShouldBeTrue();
        pattern.ShouldBe("(?<x>added)");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void RegisterResolver_ShouldThrow_WhenKeyIsNullOrWhitespace(string? key)
    {
        var ex = Should.Throw<ArgumentException>(() =>
            ResolverRegistry.RegisterResolver(key!, (name, arg) => $"(?<{name}>x)"));
        ex.ParamName.ShouldBe("key");
    }

    [Fact]
    public void RegisterResolver_ShouldThrow_WhenResolverIsNull()
    {
        var ex = Should.Throw<ArgumentNullException>(() =>
            ResolverRegistry.RegisterResolver($"null-{Guid.NewGuid():N}", null!));
        ex.ParamName.ShouldBe("resolver");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void OverrideResolver_ShouldThrow_WhenKeyIsNullOrWhitespace(string? key)
    {
        var ex = Should.Throw<ArgumentException>(() =>
            ResolverRegistry.OverrideResolver(key!, (name, arg) => $"(?<{name}>x)"));
        ex.ParamName.ShouldBe("key");
    }

    [Fact]
    public void OverrideResolver_ShouldThrow_WhenResolverIsNull()
    {
        var ex = Should.Throw<ArgumentNullException>(() =>
            ResolverRegistry.OverrideResolver("some-key", null!));
        ex.ParamName.ShouldBe("resolver");
    }
}