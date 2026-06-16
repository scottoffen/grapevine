using Grapevine.Abstractions.RouteConstraints;

namespace Grapevine.Abstractions.Tests;

public class SegmentDescriptorTests
{
    public class Literal
    {
        [Fact]
        public void SetsRawValue()
        {
            var descriptor = SegmentDescriptor.Literal("users");
            descriptor.RawValue.ShouldBe("users");
        }

        [Fact]
        public void SetsPatternAsEscapedValue()
        {
            var descriptor = SegmentDescriptor.Literal("users.v2");
            descriptor.Pattern.ShouldBe(@"users\.v2");
        }

        [Fact]
        public void SetsIsParameterFalse()
        {
            var descriptor = SegmentDescriptor.Literal("users");
            descriptor.IsParameter.ShouldBeFalse();
        }

        [Fact]
        public void SetsParameterNameNull()
        {
            var descriptor = SegmentDescriptor.Literal("users");
            descriptor.ParameterName.ShouldBeNull();
        }

        [Fact]
        public void SetsConstraintKeyNull()
        {
            var descriptor = SegmentDescriptor.Literal("users");
            descriptor.ConstraintKey.ShouldBeNull();
        }

        [Fact]
        public void SetsConstraintArgsNull()
        {
            var descriptor = SegmentDescriptor.Literal("users");
            descriptor.ConstraintArgs.ShouldBeNull();
        }

        [Fact]
        public void SetsStrictnessZero()
        {
            var descriptor = SegmentDescriptor.Literal("users");
            descriptor.Strictness.ShouldBe(0);
        }

        [Fact]
        public void SetsGroupNone()
        {
            var descriptor = SegmentDescriptor.Literal("users");
            descriptor.Group.ShouldBe((int)ConstraintGroup.None);
        }
    }

    public class Parameter
    {
        [Fact]
        public void SetsAllFields()
        {
            var rawValue       = Guid.NewGuid().ToString();
            var pattern        = Guid.NewGuid().ToString();
            var parameterName  = Guid.NewGuid().ToString();
            var constraintKey  = Guid.NewGuid().ToString();
            var constraintArgs = Guid.NewGuid().ToString();
            var strictness     = 42;
            var group          = 10;

            var descriptor = SegmentDescriptor.Parameter(
                rawValue, pattern, parameterName, constraintKey, constraintArgs, strictness, group);

            descriptor.RawValue.ShouldBe(rawValue);
            descriptor.Pattern.ShouldBe(pattern);
            descriptor.IsParameter.ShouldBeTrue();
            descriptor.ParameterName.ShouldBe(parameterName);
            descriptor.ConstraintKey.ShouldBe(constraintKey);
            descriptor.ConstraintArgs.ShouldBe(constraintArgs);
            descriptor.Strictness.ShouldBe(strictness);
            descriptor.Group.ShouldBe(group);
        }

        [Fact]
        public void SetsConstraintArgsNull_WhenNullPassed()
        {
            var descriptor = SegmentDescriptor.Parameter(
                "{id}", "(?<id>[^/]+)", "id", "text", null, 100, 0);
            descriptor.ConstraintArgs.ShouldBeNull();
        }
    }

    public class DebuggerDisplayTests
    {
        [Fact]
        public void ShowsLiteralFormat_WhenLiteralSegment()
        {
            var descriptor = SegmentDescriptor.Literal("users");
            descriptor.DebuggerDisplay.ShouldBe("[Literal] users");
        }

        [Fact]
        public void ShowsParameterFormat_WhenParameterWithNoArgs()
        {
            var descriptor = SegmentDescriptor.Parameter(
                "{id:int}", "(?<id>-?\\d+)", "id", "int", null, 60, 10);
            descriptor.DebuggerDisplay.ShouldBe("[Parameter] {id:int} (strictness=60, group=10)");
        }

        [Fact]
        public void ShowsParameterFormatWithArgs_WhenParameterHasArgs()
        {
            var descriptor = SegmentDescriptor.Parameter(
                "{id:int(1,5)}", "(?<id>-?\\d{1,5})", "id", "int", "1,5", 60, 10);
            descriptor.DebuggerDisplay.ShouldBe("[Parameter] {id:int(1,5)} (strictness=60, group=10)");
        }
    }

    public class CompareToTests
    {
        // Rule 1: literal before parameter
        [Fact]
        public void LiteralSortsBeforeParameter_WhenThisIsLiteralAndOtherIsParameter()
        {
            var literal   = SegmentDescriptor.Literal("users");
            var parameter = SegmentDescriptor.Parameter("{id}", "(?<id>[^/]+)", "id", "text", null, 100, 0);
            literal.CompareTo(parameter, false).ShouldBeLessThan(0);
        }

        [Fact]
        public void ParameterSortsAfterLiteral_WhenThisIsParameterAndOtherIsLiteral()
        {
            var literal   = SegmentDescriptor.Literal("users");
            var parameter = SegmentDescriptor.Parameter("{id}", "(?<id>[^/]+)", "id", "text", null, 100, 0);
            parameter.CompareTo(literal, false).ShouldBeGreaterThan(0);
        }

        // Rule 2: both literals, alphabetical
        [Fact]
        public void SortsAlphabetically_WhenBothAreLiterals()
        {
            var a = SegmentDescriptor.Literal("apple");
            var b = SegmentDescriptor.Literal("banana");
            a.CompareTo(b, false).ShouldBeLessThan(0);
            b.CompareTo(a, false).ShouldBeGreaterThan(0);
        }

        [Fact]
        public void ReturnsZero_WhenBothLiteralsAreEqual()
        {
            var a = SegmentDescriptor.Literal("users");
            var b = SegmentDescriptor.Literal("users");
            a.CompareTo(b, false).ShouldBe(0);
        }

        [Fact]
        public void IsCaseInsensitive_WhenComparingLiterals()
        {
            var a = SegmentDescriptor.Literal("Users");
            var b = SegmentDescriptor.Literal("users");
            a.CompareTo(b, false).ShouldBe(0);
        }

        // Rule 3: same group returns 0 when enforcing
        [Fact]
        public void ReturnsZero_WhenBothParametersAreInSameGroupAndEnforcing()
        {
            var a = SegmentDescriptor.Parameter("{a}", "p", "a", "int",     null, 60, (int)ConstraintGroup.Numeric);
            var b = SegmentDescriptor.Parameter("{b}", "p", "b", "numeric", null, 80, (int)ConstraintGroup.Numeric);
            a.CompareTo(b, false).ShouldBe(0);
        }

        [Fact]
        public void ContinuesToStrictness_WhenBothParametersAreInSameGroupAndIgnoring()
        {
            var a = SegmentDescriptor.Parameter("{a}", "p", "a", "int",     null, 60, (int)ConstraintGroup.Numeric);
            var b = SegmentDescriptor.Parameter("{b}", "p", "b", "numeric", null, 80, (int)ConstraintGroup.Numeric);
            a.CompareTo(b, true).ShouldBeLessThan(0);
        }

        [Fact]
        public void DoesNotApplyGroupCheck_WhenGroupIsNone()
        {
            var a = SegmentDescriptor.Parameter("{a}", "p", "a", "guid", null, 10, (int)ConstraintGroup.None);
            var b = SegmentDescriptor.Parameter("{b}", "p", "b", "bool", null, 20, (int)ConstraintGroup.None);
            a.CompareTo(b, false).ShouldBeLessThan(0);
        }

        // Rule 4: sort by strictness
        [Fact]
        public void StricterConstraintSortsFirst_WhenStrictnessDiffers()
        {
            var a = SegmentDescriptor.Parameter("{a}", "p", "a", "guid", null, 10, (int)ConstraintGroup.None);
            var b = SegmentDescriptor.Parameter("{b}", "p", "b", "bool", null, 20, (int)ConstraintGroup.None);
            a.CompareTo(b, false).ShouldBeLessThan(0);
        }

        // Rule 5: equal strictness on non-regex returns 0
        [Fact]
        public void ReturnsZero_WhenBothParametersHaveEqualStrictnessAndAreNotRegex()
        {
            var a = SegmentDescriptor.Parameter("{a}", "p", "a", "int",  null, 60, (int)ConstraintGroup.Numeric);
            var b = SegmentDescriptor.Parameter("{b}", "p", "b", "long", null, 60, (int)ConstraintGroup.Numeric);
            // ignoreGroupConflicts=true so we get past rule 3 and reach rule 5
            a.CompareTo(b, true).ShouldBe(0);
        }

        // Rule 6: both regex, sort by pattern
        [Fact]
        public void SortsByPattern_WhenBothAreRegex()
        {
            // "[0-9]+" sorts before "[a-z]+" by ordinal comparison ('0' < 'a'),
            // so a.CompareTo(b) should return a negative value.
            var a = SegmentDescriptor.Parameter("{a}", "p", "a", "regex", "[0-9]+", RegexResolver.Strictness, (int)ConstraintGroup.None);
            var b = SegmentDescriptor.Parameter("{b}", "p", "b", "regex", "[a-z]+", RegexResolver.Strictness, (int)ConstraintGroup.None);
            a.CompareTo(b, false).ShouldBeLessThan(0);
        }

        [Fact]
        public void ReturnsZero_WhenBothRegexPatternsAreIdentical()
        {
            var a = SegmentDescriptor.Parameter("{a}", "p", "a", "regex", "[0-9]+", RegexResolver.Strictness, (int)ConstraintGroup.None);
            var b = SegmentDescriptor.Parameter("{b}", "p", "b", "regex", "[0-9]+", RegexResolver.Strictness, (int)ConstraintGroup.None);
            a.CompareTo(b, false).ShouldBe(0);
        }
    }
}