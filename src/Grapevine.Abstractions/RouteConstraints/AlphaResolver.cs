namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// Resolves the <c>alpha</c> route constraint to a regular expression pattern
/// that matches one or more alphabetic characters.
/// </summary>
public static class AlphaResolver
{
    internal static readonly int Strictness = 90;
    internal static readonly int Group      = (int)ConstraintGroup.Text;

    private static readonly string _basePattern = "[a-zA-Z]";

    /// <summary>
    /// Returns a named capture group pattern matching alphabetic characters,
    /// with an optional length constraint.
    /// </summary>
    /// <remarks>
    /// Supported length argument formats:
    /// <list type="bullet">
    ///   <item><term>null or empty</term><description>one or more letters</description></item>
    ///   <item><term><c>3</c></term><description>exactly 3 letters</description></item>
    ///   <item><term><c>1,</c></term><description>at least 1 letter</description></item>
    ///   <item><term><c>,5</c></term><description>up to 5 letters</description></item>
    ///   <item><term><c>1,5</c></term><description>between 1 and 5 letters</description></item>
    /// </list>
    /// </remarks>
    /// <param name="name">The route parameter name for the named capture group.</param>
    /// <param name="args">An optional length constraint string.</param>
    /// <returns>
    /// A tuple containing the named capture group pattern, a strictness value of
    /// <c>90</c>, and a group of <see cref="ConstraintGroup.Text"/>.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="args"/> is invalid.</exception>
    public static (string pattern, int strictness, int group) Resolve(string name, string? args)
    {
        var quantifier = string.IsNullOrWhiteSpace(args)
            ? "+"
            : LengthPatternResolver.Resolve(args);

        return ($"(?<{name}>{_basePattern}{quantifier})", Strictness, Group);
    }
}