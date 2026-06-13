using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// Resolves the <c>regex</c> route constraint to a named capture group wrapping a
/// user-supplied regular expression pattern.
/// </summary>
public static class RegexResolver
{
    internal static readonly int    Strictness              = 1;
    internal static readonly int    Group                   = (int)ConstraintGroup.None;
    internal static readonly string EmptyPatternMessage     = "The 'regex' constraint requires a non-empty pattern.";
    internal static readonly string AnchoredPatternMessage  = "The 'regex' constraint must not start with ^ or end with $.";
    internal static readonly string CaptureGroupsMessage    = "The 'regex' constraint must not contain unnamed capture groups. Use named groups (?<name>...) or non-capturing groups (?:...) instead.";
    internal static readonly string DuplicateNameMessage    = "The 'regex' constraint must not contain a named group that conflicts with the route parameter name.";
    internal static readonly string InvalidPatternMessage   = "Invalid regular expression pattern.";

    internal static readonly ConcurrentDictionary<string, Regex> _cache = new();

    /// <summary>
    /// Returns a named capture group wrapping the user-supplied regular expression pattern.
    /// The compiled regex is cached on first use and reused on subsequent calls with the
    /// same pattern.
    /// </summary>
    /// <remarks>
    /// The provided pattern must:
    /// <list type="bullet">
    ///   <item><description>Be non-empty.</description></item>
    ///   <item><description>Not be anchored -- must not start with <c>^</c> or end with <c>$</c>.</description></item>
    ///   <item><description>Not contain any additional capture groups beyond the outer named one.</description></item>
    ///   <item><description>Be a valid regular expression.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="name">The route parameter name for the named capture group.</param>
    /// <param name="args">The regular expression pattern to wrap.</param>
    /// <returns>
    /// A tuple containing the named capture group pattern, a strictness value of
    /// <c>1</c>, and a group of <see cref="ConstraintGroup.None"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the pattern is null or empty, anchored, contains capture groups,
    /// or fails to compile.
    /// </exception>
    public static (string pattern, int strictness, int group) Resolve(string name, string? args)
    {
        if (string.IsNullOrWhiteSpace(args))
            throw new ArgumentException(EmptyPatternMessage, nameof(args));

        var pattern = args!.Trim();

        if (pattern.StartsWith("^") || pattern.EndsWith("$"))
            throw new ArgumentException(AnchoredPatternMessage, nameof(args));

        _cache.GetOrAdd(pattern, p =>
        {
            Regex r;
            try
            {
                r = new Regex(p, RegexOptions.Compiled);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(InvalidPatternMessage, nameof(args), ex);
            }

            if (r.GetGroupNames().Any(n => int.TryParse(n, out var i) && i > 0))
                throw new ArgumentException(CaptureGroupsMessage, nameof(args));

            if (r.GetGroupNames().Any(n => !int.TryParse(n, out _) && n == name))
                throw new ArgumentException(DuplicateNameMessage, nameof(args));

            return r;
        });

        return ($"(?<{name}>{pattern})", Strictness, Group);
    }
}