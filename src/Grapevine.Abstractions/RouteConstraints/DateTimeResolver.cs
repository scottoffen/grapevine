namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// Resolves the <c>datetime</c> route constraint to a regular expression pattern
/// that matches common date and time formats.
/// </summary>
/// <remarks>
/// For date-only matching without a time component, use the <c>date</c> constraint instead.
/// </remarks>
public static class DateTimeResolver
{
    private static readonly Dictionary<string, string> _patterns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["iso"]   = @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}",
        ["time"]  = @"\d{2}:\d{2}:\d{2}",
        ["basic"] = @"\d{8}",
        ["rfc"]   = @"[A-Za-z]{3}, \d{2} [A-Za-z]{3} \d{4} \d{2}:\d{2}:\d{2} GMT"
    };

    /// <summary>
    /// Returns a named capture group pattern matching a date/time string in the specified format.
    /// </summary>
    /// <remarks>
    /// Supported format arguments:
    /// <list type="bullet">
    ///   <item><term>null or empty</term><description>ISO 8601: <c>2023-05-21T14:30:00</c></description></item>
    ///   <item><term><c>iso</c></term><description><c>2023-05-21T14:30:00</c></description></item>
    ///   <item><term><c>time</c></term><description>time only: <c>14:30:00</c></description></item>
    ///   <item><term><c>basic</c></term><description>compact date: <c>20230521</c></description></item>
    ///   <item><term><c>rfc</c></term><description>RFC 1123: <c>Sun, 21 May 2023 14:30:00 GMT</c></description></item>
    /// </list>
    /// For date-only matching, use the <c>date</c> constraint instead of <c>datetime</c>.
    /// </remarks>
    /// <param name="name">The route parameter name for the named capture group.</param>
    /// <param name="args">An optional format identifier string.</param>
    /// <returns>A named capture group regular expression pattern.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="args"/> is not a supported format.</exception>
    public static string Resolve(string name, string? args)
    {
        if (string.IsNullOrWhiteSpace(args))
            return $"(?<{name}>{_patterns["iso"]})";

        if (_patterns.TryGetValue(args!.Trim(), out var value))
            return $"(?<{name}>{value})";

        throw new ArgumentException(
            $"The 'datetime' constraint does not support the argument '{args}'. Supported values: {string.Join(", ", _patterns.Keys)}. For date-only matching use the 'date' constraint.",
            nameof(args));
    }
}