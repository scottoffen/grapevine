namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// Resolves the <c>text</c> and <c>len</c> route constraints to a regular expression
/// pattern that matches one or more non-slash characters.
/// </summary>
public static class DefaultResolver
{
    private static readonly string _pattern = @"[^/]";

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
    /// <returns>A named capture group regular expression pattern.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="args"/> is invalid.</exception>
    public static string Resolve(string name, string? args)
    {
        if (string.IsNullOrWhiteSpace(args))
            return $"(?<{name}>{_pattern}+)";

        var length = LengthPatternResolver.Resolve(args);
        return $"(?<{name}>{_pattern}{length})";
    }
}