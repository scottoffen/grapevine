namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// Identifies the compatibility group a route constraint belongs to, used during
/// route registration to detect potentially ambiguous routes.
/// </summary>
/// <remarks>
/// <para>
/// When two routes are structurally identical and their differing constraints belong
/// to the same group, the router can either throw an <see cref="InvalidOperationException"/>
/// or silently resolve the ambiguity by preferring the route with the lower strictness
/// value, depending on whether same-group ambiguity checking is enabled.
/// </para>
/// <para>
/// Consumers adding custom constraints may define their own group values. The built-in
/// values are spaced in increments of 10 to leave room for insertion. Use values that
/// do not conflict with existing entries.
/// </para>
/// <para>
/// Use <c>(int)ConstraintGroup.None</c> to compare against a raw <c>int</c> group value
/// when no group membership applies.
/// </para>
/// </remarks>
public enum ConstraintGroup : int
{
    /// <summary>
    /// The constraint does not belong to any compatibility group. Routes using
    /// constraints in this group are never considered ambiguous with each other
    /// based on group membership alone.
    /// </summary>
    None = 0,

    /// <summary>
    /// Numeric constraints: <c>decimal</c>, <c>int</c>, <c>long</c>, <c>double</c>,
    /// <c>float</c>, and <c>numeric</c>. These constraints have overlapping match sets
    /// and may produce ambiguous routes when used on the same segment position.
    /// </summary>
    Numeric = 10,

    /// <summary>
    /// Date and time constraints: <c>date</c> and <c>datetime</c>. These constraints
    /// share some format patterns and may produce ambiguous routes when used on the
    /// same segment position.
    /// </summary>
    Date = 20,

    /// <summary>
    /// Text constraints: <c>alpha</c>, <c>text</c>, and <c>len</c>. Every alphabetic
    /// string is also a valid text match, making routes using these constraints on the
    /// same segment position potentially ambiguous.
    /// </summary>
    Text = 30,
}