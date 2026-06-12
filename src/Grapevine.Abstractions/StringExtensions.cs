using System.Text;

namespace Grapevine;

/// <summary>
/// Provides extension methods for <see cref="string"/> values used internally
/// for path normalization and prefix, suffix, and substring matching.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Returns <see langword="true"/> if the string contains any of the provided
    /// values, using a case-insensitive, culture-aware comparison.
    /// </summary>
    /// <param name="source">The string to search within.</param>
    /// <param name="values">One or more substrings to search for.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="source"/> contains at least one
    /// entry in <paramref name="values"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool ContainsAny(this string? source, params string[] values)
        => source.ContainsAny(values, StringComparison.CurrentCultureIgnoreCase);

    /// <summary>
    /// Returns <see langword="true"/> if the string contains any of the provided
    /// values, using the specified <see cref="StringComparison"/>.
    /// </summary>
    /// <param name="source">The string to search within.</param>
    /// <param name="comparison">The comparison rule to apply.</param>
    /// <param name="values">One or more substrings to search for.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="source"/> contains at least one
    /// entry in <paramref name="values"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool ContainsAny(this string? source, StringComparison comparison, params string[] values)
        => source.ContainsAny(values, comparison);

    /// <summary>
    /// Returns <see langword="true"/> if the string contains any of the provided
    /// values, using the specified <see cref="StringComparison"/>.
    /// </summary>
    /// <param name="source">The string to search within.</param>
    /// <param name="values">A collection of substrings to search for.</param>
    /// <param name="comparison">The comparison rule to apply.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="source"/> contains at least one
    /// entry in <paramref name="values"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool ContainsAny(this string? source, IEnumerable<string>? values, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
    {
        if (string.IsNullOrEmpty(source) || values == null)
            return false;

        foreach (var value in values)
        {
            if (source.ContainsIgnoreCase(value, comparison))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the string contains the specified value,
    /// using the specified <see cref="StringComparison"/>.
    /// </summary>
    /// <param name="source">The string to search within.</param>
    /// <param name="value">The substring to search for.</param>
    /// <param name="comparison">The comparison rule to apply.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="source"/> contains
    /// <paramref name="value"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool ContainsIgnoreCase(this string? source, string? value, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
            return false;

        return source!.IndexOf(value, comparison) >= 0;
    }

    /// <summary>
    /// Determines whether the string ends with any of the provided suffixes,
    /// using a case-insensitive, culture-aware comparison.
    /// </summary>
    /// <param name="value">The string to test.</param>
    /// <param name="suffixes">One or more suffixes to check against.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> ends with at least one
    /// entry in <paramref name="suffixes"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool EndsWithAny(this string value, params string[] suffixes)
    {
        foreach (var suffix in suffixes)
        {
            if (value.EndsWith(suffix, StringComparison.CurrentCultureIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Normalizes a URL path segment by trimming whitespace, removing leading and
    /// trailing slashes, and collapsing any internal consecutive slashes to a single
    /// slash, then prepending exactly one leading slash. Returns an empty string if
    /// the input is null, empty, or whitespace.
    /// </summary>
    /// <param name="input">The path string to trim.</param>
    /// <returns>
    /// A path string with exactly one leading slash, no trailing slash, and no
    /// consecutive internal slashes, or <see cref="string.Empty"/> if the input
    /// contains no meaningful content.
    /// </returns>
    public static string TrimPath(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var trimmed = input!.Trim();

        var start = 0;
        while (start < trimmed.Length && trimmed[start] == '/')
            start++;

        var end = trimmed.Length - 1;
        while (end >= start && trimmed[end] == '/')
            end--;

        if (start > end)
            return string.Empty;

        var sb = new StringBuilder(trimmed.Length + 1);
        sb.Append('/');

        var previousWasSlash = false;
        for (var i = start; i <= end; i++)
        {
            var c = trimmed[i];
            if (c == '/')
            {
                if (!previousWasSlash)
                {
                    sb.Append('/');
                    previousWasSlash = true;
                }
            }
            else
            {
                sb.Append(c);
                previousWasSlash = false;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Determines whether the string starts with any of the provided prefixes,
    /// using a case-insensitive, culture-aware comparison.
    /// </summary>
    /// <param name="value">The string to test.</param>
    /// <param name="prefixes">One or more prefixes to check against.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> starts with at least one
    /// entry in <paramref name="prefixes"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool StartsWithAny(this string value, params string[] prefixes)
        => value.StartsWithAny((IEnumerable<string>)prefixes);

    /// <summary>
    /// Determines whether the string starts with any of the provided prefixes,
    /// using a case-insensitive, culture-aware comparison.
    /// </summary>
    /// <param name="value">The string to test.</param>
    /// <param name="prefixes">A collection of prefixes to check against.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> starts with at least one
    /// entry in <paramref name="prefixes"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool StartsWithAny(this string value, IEnumerable<string> prefixes)
    {
        foreach (var prefix in prefixes)
        {
            if (value.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
                return true;
        }

        return false;
    }
}