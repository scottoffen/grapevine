namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// Resolves the <c>numeric</c> route constraint to a regular expression pattern
/// that matches one or more digit characters (0-9) with no sign or other characters.
/// </summary>
/// <remarks>
/// Unlike <see cref="IntResolver"/>, this resolver does not allow a leading minus sign.
/// Every character in the matched value must be a digit. Use <c>int</c> or <c>long</c>
/// when negative values are needed.
/// </remarks>
public static class NumericResolver
{
    internal static readonly int Strictness = 80;
    internal static readonly int Group      = (int)ConstraintGroup.Numeric;

    private static readonly string _basePattern = @"\d";
    private static readonly string _unbound      = @"\d+";

    /// <summary>
    /// Returns a named capture group pattern matching one or more digit characters,
    /// with an optional length constraint.
    /// </summary>
    /// <remarks>
    /// Supported length argument formats:
    /// <list type="bullet">
    ///   <item><term>null or empty</term><description>any number of digits</description></item>
    ///   <item><term><c>3</c></term><description>exactly 3 digits</description></item>
    ///   <item><term><c>1,</c></term><description>at least 1 digit</description></item>
    ///   <item><term><c>,5</c></term><description>up to 5 digits</description></item>
    ///   <item><term><c>1,5</c></term><description>between 1 and 5 digits</description></item>
    /// </list>
    /// </remarks>
    /// <param name="name">The route parameter name for the named capture group.</param>
    /// <param name="args">An optional length constraint string.</param>
    /// <returns>
    /// A tuple containing the named capture group pattern, a strictness value of
    /// <c>80</c>, and a group of <see cref="ConstraintGroup.Numeric"/>.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="args"/> is invalid.</exception>
    public static (string pattern, int strictness, int group) Resolve(string name, string? args)
    {
        if (string.IsNullOrWhiteSpace(args))
            return ($"(?<{name}>{_unbound})", Strictness, Group);

        var quantifier = LengthPatternResolver.Resolve(args);
        return ($"(?<{name}>{_basePattern}{quantifier})", Strictness, Group);
    }
}