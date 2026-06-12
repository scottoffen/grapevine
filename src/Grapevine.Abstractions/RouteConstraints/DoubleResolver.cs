namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// Resolves the <c>double</c> and <c>float</c> route constraints to a regular
/// expression pattern that matches decimal or scientific notation numbers.
/// </summary>
public static class DoubleResolver
{
    private static readonly string _pattern = @"[-]?\d+(?:\.\d{length})?(?:[eE][-+]?\d+)?";
    private static readonly string _unbound = @"[-]?\d+(?:\.\d+)?(?:[eE][-+]?\d+)?";

    /// <summary>
    /// Returns a named capture group pattern matching a double or float number,
    /// including optional scientific notation, with an optional precision constraint.
    /// </summary>
    /// <remarks>
    /// Supported precision argument formats:
    /// <list type="bullet">
    ///   <item><term>null or empty</term><description>any precision, with optional scientific notation</description></item>
    ///   <item><term><c>0</c></term><description>no decimal places (whole numbers only)</description></item>
    ///   <item><term><c>2</c></term><description>exactly 2 decimal places</description></item>
    ///   <item><term><c>0,</c></term><description>zero or more decimal places</description></item>
    ///   <item><term><c>,3</c></term><description>up to 3 decimal places</description></item>
    ///   <item><term><c>1,3</c></term><description>between 1 and 3 decimal places</description></item>
    /// </list>
    /// </remarks>
    /// <param name="name">The route parameter name for the named capture group.</param>
    /// <param name="args">An optional precision constraint string.</param>
    /// <returns>A named capture group regular expression pattern.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="args"/> is invalid.</exception>
    public static string Resolve(string name, string? args)
    {
        if (string.IsNullOrWhiteSpace(args))
            return $"(?<{name}>{_unbound})";

        var length = PrecisionPatternResolver.Resolve(args);
        return $"(?<{name}>{_pattern.Replace("{length}", length)})";
    }
}