namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// Resolves the <c>date</c> route constraint to a regular expression pattern
/// that matches common date formats.
/// </summary>
/// <remarks>
/// For date and time matching including a time component, use the <c>datetime</c>
/// constraint instead.
/// </remarks>
public static class DateResolver
{
    internal static readonly int    Strictness             = 30;
    internal static readonly int    Group                  = (int)ConstraintGroup.Date;
    internal static readonly string UnsupportedFormatMessage =
        "The 'date' constraint does not support the argument '{0}'. Supported values: {1}. For date and time matching use the 'datetime' constraint.";

    private static readonly Dictionary<string, string> _patterns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["iso"]   = @"\d{4}-\d{2}-\d{2}",
        ["ymd"]   = @"\d{4}[-/]\d{2}[-/]\d{2}",
        ["mdy"]   = @"\d{1,2}[-/]\d{1,2}[-/]\d{4}",
        ["dmy"]   = @"\d{1,2}[-/]\d{1,2}[-/]\d{4}",
        ["basic"] = @"\d{8}"
    };

    /// <summary>
    /// Returns a named capture group pattern matching a date string in the specified format.
    /// </summary>
    /// <remarks>
    /// Supported format arguments:
    /// <list type="bullet">
    ///   <item><term>null or empty</term><description>ISO format: <c>2023-05-21</c></description></item>
    ///   <item><term><c>iso</c></term><description><c>2023-05-21</c></description></item>
    ///   <item><term><c>ymd</c></term><description><c>2023/05/21</c> or <c>2023-05-21</c></description></item>
    ///   <item><term><c>mdy</c></term><description><c>05/21/2023</c> or <c>05-21-2023</c></description></item>
    ///   <item><term><c>dmy</c></term><description><c>21/05/2023</c> or <c>21-05-2023</c></description></item>
    ///   <item><term><c>basic</c></term><description>compact format: <c>20230521</c></description></item>
    /// </list>
    /// For date and time matching, use the <c>datetime</c> constraint instead.
    /// </remarks>
    /// <param name="name">The route parameter name for the named capture group.</param>
    /// <param name="args">An optional format identifier string.</param>
    /// <returns>
    /// A tuple containing the named capture group pattern, a strictness value of
    /// <c>30</c>, and a group of <see cref="ConstraintGroup.Date"/>.
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