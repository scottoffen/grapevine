namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// Resolves the <c>text</c> and <c>len</c> route constraints to a regular expression
/// pattern that matches one or more non-slash characters.
/// </summary>
public static class DefaultResolver
{
    internal static readonly int Strictness = 100;
    internal static readonly int Group      = (int)ConstraintGroup.Text;

    private static readonly string _basePattern = "[^/]";

    /// <summary>
    /// Returns a named capture group pattern matching one or more non-slash characters,
    /// with an optional length constraint.
    /// </summary>
    /// <remarks>
    /// Supported length argument formats:
    /// <list type="bullet">
    ///   <item><term>null or empty</term><description>one or more non-slash characters</description></item>
    ///   <item><term><c>3</c></term><description>exactly 3 characters</description></item>
    ///   <item><term><c>1,</c></term><description>at least 1 character</description></item>
    ///   <item><term><c>,5</c></term><description>up to 5 characters</description></item>
    ///   <item><term><c>1,5</c></term><description>between 1 and 5 characters</description></item>
    /// </list>
    /// </remarks>
    /// <param name="name">The route parameter name for the named capture group.</param>
    /// <param name="args">An optional length constraint string.</param>
    /// <returns>
    /// A tuple containing the named capture group pattern, a strictness value of
    /// <c>100</c>, and a group of <see cref="ConstraintGroup.Text"/>.
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