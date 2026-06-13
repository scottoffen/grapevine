using System.Diagnostics;
using System.Text.RegularExpressions;
using Grapevine.Abstractions.RouteConstraints;

namespace Grapevine.Abstractions;

/// <summary>
/// Describes a single parsed segment from a route template, including its original
/// value, resolved regular expression pattern, and all metadata needed for route
/// sorting.
/// </summary>
/// <remarks>
/// <para>
/// A segment is either a literal string (e.g. <c>users</c>) or a parameter
/// (e.g. <c>{id:int}</c>). Literal segments always sort before parameter segments
/// at the same position. When both segments are parameters, the sort order is
/// determined by strictness and constraint group.
/// </para>
/// <para>
/// <see cref="SegmentDescriptor"/> is a value type. Two descriptors are equal when
/// all of their fields are equal. Value equality is synthesized by the compiler.
/// </para>
/// </remarks>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly record struct SegmentDescriptor
{
    /// <summary>
    /// Gets the original segment string as it appeared in the route template,
    /// including any braces, constraint key, and arguments.
    /// </summary>
    public string RawValue { get; init; }

    /// <summary>
    /// Gets the resolved regular expression pattern for this segment.
    /// </summary>
    public string Pattern { get; init; }

    /// <summary>
    /// Gets a value indicating whether this segment is a route parameter.
    /// When <see langword="false"/>, the segment is a literal string.
    /// </summary>
    public bool IsParameter { get; init; }

    /// <summary>
    /// Gets the parameter name extracted from the segment, or <see langword="null"/>
    /// if this is a literal segment.
    /// </summary>
    public string? ParameterName { get; init; }

    /// <summary>
    /// Gets the constraint key extracted from the segment (e.g. <c>int</c>, <c>guid</c>),
    /// or <see langword="null"/> if this is a literal segment.
    /// </summary>
    public string? ConstraintKey { get; init; }

    /// <summary>
    /// Gets the constraint arguments extracted from the segment (e.g. <c>1,5</c>),
    /// or <see langword="null"/> if no arguments were supplied or this is a literal segment.
    /// </summary>
    public string? ConstraintArgs { get; init; }

    /// <summary>
    /// Gets the strictness value for this segment's constraint. Lower values indicate
    /// a more strict constraint. Literal segments have a strictness of <c>0</c>.
    /// </summary>
    /// <remarks>
    /// Built-in strictness values:
    /// <list type="bullet">
    ///   <item><term><c>regex</c></term><description>1</description></item>
    ///   <item><term><c>guid</c></term><description>10</description></item>
    ///   <item><term><c>bool</c></term><description>20</description></item>
    ///   <item><term><c>date</c></term><description>30</description></item>
    ///   <item><term><c>datetime</c></term><description>40</description></item>
    ///   <item><term><c>decimal</c></term><description>50</description></item>
    ///   <item><term><c>int</c> / <c>long</c></term><description>60</description></item>
    ///   <item><term><c>double</c> / <c>float</c></term><description>70</description></item>
    ///   <item><term><c>numeric</c></term><description>80</description></item>
    ///   <item><term><c>alpha</c></term><description>90</description></item>
    ///   <item><term><c>text</c> / <c>len</c></term><description>100</description></item>
    /// </list>
    /// </remarks>
    public int Strictness { get; init; }

    /// <summary>
    /// Gets the compatibility group for this segment's constraint, expressed as the
    /// underlying integer value of a <see cref="ConstraintGroup"/> or a consumer-defined
    /// group value. Literal segments use <c>(int)ConstraintGroup.None</c>.
    /// </summary>
    /// <remarks>
    /// Use <see cref="ConstraintGroup"/> values for the built-in groups, or define
    /// custom integer values for consumer-defined groups.
    /// </remarks>
    public int Group { get; init; }

    /// <summary>
    /// Gets a string used by the debugger to display a concise summary of this segment.
    /// </summary>
    /// <remarks>
    /// Literal segments display as <c>[Literal] users</c>. Parameter segments display
    /// as <c>[Parameter] {id:int} (strictness=60, group=10)</c>, including constraint
    /// arguments when present, e.g. <c>{id:int(1,5)}</c>.
    /// </remarks>
    internal string DebuggerDisplay
    {
        get
        {
            if (!IsParameter)
                return $"[Literal] {RawValue}";

            var args        = ConstraintArgs != null ? $"({ConstraintArgs})" : string.Empty;
            var constraint  = ConstraintKey != null ? $":{ConstraintKey}{args}" : string.Empty;
            return $"[Parameter] {{{ParameterName}{constraint}}} (strictness={Strictness}, group={Group})";
        }
    }

    /// <summary>
    /// Compares this segment descriptor to another to determine sort order between
    /// route templates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A return value of <c>0</c> means the two segments are indistinguishable at this
    /// position; the caller is responsible for determining whether that constitutes
    /// ambiguity across the full route.
    /// </para>
    /// <para>
    /// Sort rules applied in order:
    /// <list type="number">
    ///   <item>Literal segments sort before parameter segments.</item>
    ///   <item>When both are literals, sort alphabetically by <see cref="RawValue"/> ascending.</item>
    ///   <item>
    ///     When both are parameters and <paramref name="ignoreGroupConflicts"/> is
    ///     <see langword="false"/>, returns <c>0</c> if both segments belong to the same
    ///     non-None constraint group.
    ///   </item>
    ///   <item>Sort by <see cref="Strictness"/> ascending.</item>
    ///   <item>When strictness is equal and neither segment is a <c>regex</c> constraint, returns <c>0</c>.</item>
    ///   <item>
    ///     When both are <c>regex</c>, sort alphabetically by <see cref="ConstraintArgs"/>
    ///     (the raw pattern). Returns <c>0</c> if both patterns are identical.
    ///   </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="other">The other <see cref="SegmentDescriptor"/> to compare against.</param>
    /// <param name="ignoreGroupConflicts">
    /// When <see langword="true"/>, same-group constraint conflicts are silently bypassed
    /// and sorting continues by strictness. When <see langword="false"/>, a same-group
    /// conflict returns <c>0</c>.
    /// </param>
    /// <returns>
    /// A negative integer if this segment sorts before <paramref name="other"/>,
    /// a positive integer if it sorts after, or <c>0</c> if the segments are
    /// indistinguishable at this position.
    /// </returns>
    public int CompareTo(SegmentDescriptor other, bool ignoreGroupConflicts)
    {
        // Rule 1: literals always sort before parameters.
        if (!IsParameter && other.IsParameter) return -1;
        if (IsParameter && !other.IsParameter) return 1;

        // Rule 2: both literals -- sort alphabetically by raw value ascending.
        if (!IsParameter && !other.IsParameter)
            return string.Compare(RawValue, other.RawValue, StringComparison.OrdinalIgnoreCase);

        // Rules 3-6 apply only when both segments are parameters.

        // Rule 3: same-group check. Group None (0) is exempt -- singleton constraints
        // have no family overlap concern. Return 0 to signal indistinguishable to the caller.
        if (!ignoreGroupConflicts
            && Group != (int)ConstraintGroup.None
            && Group == other.Group)
        {
            return 0;
        }

        // Rule 4: sort by strictness ascending (lower = stricter = sorts first).
        var strictnessDiff = Strictness.CompareTo(other.Strictness);
        if (strictnessDiff != 0) return strictnessDiff;

        // Strictness is equal from here down.

        // Rule 5: equal strictness on non-regex constraints -- indistinguishable at this position.
        if (Strictness != RegexResolver.Strictness) return 0;

        // Rule 6: both are regex -- sort by raw pattern.
        // Identical patterns are indistinguishable at this position.
        return string.Compare(
            ConstraintArgs ?? string.Empty,
            other.ConstraintArgs ?? string.Empty,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates a <see cref="SegmentDescriptor"/> for a literal route segment.
    /// </summary>
    /// <param name="rawValue">The original literal string from the route template.</param>
    /// <returns>A descriptor representing a non-parameter segment.</returns>
    public static SegmentDescriptor Literal(string rawValue)
    {
        return new SegmentDescriptor
        {
            RawValue       = rawValue,
            Pattern        = Regex.Escape(rawValue),
            IsParameter    = false,
            ParameterName  = null,
            ConstraintKey  = null,
            ConstraintArgs = null,
            Strictness     = 0,
            Group          = (int)ConstraintGroup.None,
        };
    }

    /// <summary>
    /// Creates a <see cref="SegmentDescriptor"/> for a route parameter segment.
    /// </summary>
    /// <param name="rawValue">The original parameter string from the route template, including braces.</param>
    /// <param name="pattern">The resolved regular expression pattern for the parameter.</param>
    /// <param name="parameterName">The name of the route parameter.</param>
    /// <param name="constraintKey">The constraint key used to resolve the pattern.</param>
    /// <param name="constraintArgs">The constraint arguments, or <see langword="null"/> if none.</param>
    /// <param name="strictness">The strictness value returned by the resolver.</param>
    /// <param name="group">The compatibility group value returned by the resolver.</param>
    /// <returns>A descriptor representing a parameter segment.</returns>
    public static SegmentDescriptor Parameter(
        string  rawValue,
        string  pattern,
        string  parameterName,
        string  constraintKey,
        string? constraintArgs,
        int     strictness,
        int     group)
    {
        return new SegmentDescriptor
        {
            RawValue       = rawValue,
            Pattern        = pattern,
            IsParameter    = true,
            ParameterName  = parameterName,
            ConstraintKey  = constraintKey,
            ConstraintArgs = constraintArgs,
            Strictness     = strictness,
            Group          = group,
        };
    }
}