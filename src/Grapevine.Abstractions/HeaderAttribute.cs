using System.Text.RegularExpressions;

namespace Grapevine;

/// <summary>
/// Restricts a route handler method to requests that contain a specific header,
/// optionally requiring the header value to match a pattern or exact value.
/// </summary>
/// <remarks>
/// <para>
/// Multiple instances of this attribute may be applied to the same method, in which
/// case all header constraints must be satisfied for the route to match.
/// </para>
/// <para>
/// Three matching modes are supported depending on which constructor parameters are provided:
/// </para>
/// <list type="bullet">
/// <item><description>Key only: the header must exist with any value.</description></item>
/// <item><description>Pattern: the header value must match the specified regular expression.</description></item>
/// <item><description>Exact: the header value must match the specified string exactly.</description></item>
/// </list>
/// <para>
/// Note that header values are matched using regular expressions internally. When using
/// the <c>pattern</c> parameter, special regex characters in the value must
/// be escaped manually. Use the <c>exact</c> parameter to match a literal
/// string without needing to escape special characters.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HeaderAttribute : Attribute
{
    /// <summary>
    /// Gets the header key to match against incoming request headers.
    /// </summary>
    public string Key { get; init; }

    /// <summary>
    /// Gets the compiled regular expression used to match the header value.
    /// Matches any value when neither <c>pattern</c> nor <c>exact</c> is specified.
    /// </summary>
    public Regex Value { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="HeaderAttribute"/> with the specified
    /// key and optional matching constraints.
    /// </summary>
    /// <param name="key">The header key to match against incoming request headers.</param>
    /// <param name="pattern">
    /// A regular expression pattern the header value must match. Special regex characters
    /// must be escaped manually. Mutually exclusive with <paramref name="exact"/>; if both
    /// are provided, <paramref name="exact"/> takes precedence.
    /// </param>
    /// <param name="exact">
    /// A literal string the header value must match exactly. Special characters are
    /// automatically escaped. Takes precedence over <paramref name="pattern"/> if both
    /// are provided.
    /// </param>
    public HeaderAttribute(string key, string? pattern = null, string? exact = null)
    {
        Key = key;
        Value = exact != null
            ? new Regex(Regex.Escape(exact), RegexOptions.Compiled)
            : pattern != null
                ? new Regex(pattern, RegexOptions.Compiled)
                : new Regex(".*", RegexOptions.Compiled);
    }
}