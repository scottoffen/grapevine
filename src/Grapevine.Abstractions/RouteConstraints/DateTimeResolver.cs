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
    internal static readonly int    Strictness               = 40;
    internal static readonly int    Group                    = (int)ConstraintGroup.Date;
    internal static readonly string UnsupportedFormatMessage =
        "The 'datetime' constraint does not support the argument '{0}'. Supported values: {1}. For date-only matching use the 'date' constraint.";

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
    /// <returns>
    /// A tuple containing the named capture group pattern, a strictness value of
    /// <c>40</c>, and a group of <see cref="ConstraintGroup.Date"/>.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="args"/> is not a supported format.</exception>
    public static (string pattern, int strictness, int group) Resolve(string name, string? args)
    {
        if (string.IsNullOrWhiteSpace(args))
            return ($"(?<{name}>{_patterns["iso"]})", Strictness, Group);

        if (_patterns.TryGetValue(args!.Trim(), out var value))
            return ($"(?<{name}>{value})", Strictness, Group);

        throw new ArgumentException(
            string.Format(UnsupportedFormatMessage, args, string.Join(", ", _patterns.Keys)),
            nameof(args));
    }
}