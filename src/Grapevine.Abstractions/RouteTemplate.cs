using System.Diagnostics;
using System.Text.RegularExpressions;
using Grapevine.Abstractions.RouteConstraints;

namespace Grapevine.Abstractions;

/// <summary>
/// Represents a parsed route template, including the normalized template string,
/// the compiled regular expression used to match incoming request paths, and the
/// individual segment descriptors that describe each parsed segment.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="RouteTemplate"/> is parsed from a string such as
/// <c>/users/{id:int}/orders</c>. Each slash-delimited segment is parsed into a
/// <see cref="SegmentDescriptor"/>: literal segments are escaped directly, and
/// parameter segments are resolved through the <see cref="ResolverRegistry"/>.
/// </para>
/// <para>
/// The compiled <see cref="Regex"/> is anchored (<c>^...$</c>) and built from the
/// concatenation of all segment patterns, separated by <c>/</c>. It is compiled once
/// at parse time and reused for all subsequent request matches.
/// </para>
/// <para>
/// <see cref="RawTemplate"/> is always stored in normalized form with a leading slash
/// and no trailing slash. Two templates that differ only by a leading slash are
/// considered equal.
/// </para>
/// </remarks>
[DebuggerDisplay("{RawTemplate} [{SegmentCount} segments, {ParameterCount} parameters]")]
public sealed record RouteTemplate : IEquatable<RouteTemplate>
{
    internal static readonly string DuplicateParameterNameMessage =
        "Route template '{0}' contains duplicate parameter name '{1}'.";

    /// <summary>
    /// A catch-all route template that matches any path. Used as the default template
    /// when no route template is specified on a <see cref="RouteDescriptor"/>.
    /// </summary>
    internal static readonly RouteTemplate CatchAll = new RouteTemplate(
        "/.*",
        new Regex(@"^/.*$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase),
        Array.Empty<SegmentDescriptor>(),
        0,
        Array.Empty<string>());

    /// <summary>
    /// Gets the normalized route template string. Always begins with <c>/</c> and
    /// contains no trailing slash.
    /// </summary>
    public string RawTemplate { get; init; }

    /// <summary>
    /// Gets the compiled regular expression used to match incoming request paths
    /// against this route template.
    /// </summary>
    public Regex CompiledRegex { get; init; }

    /// <summary>
    /// Gets the ordered array of segment descriptors parsed from the route template.
    /// </summary>
    public SegmentDescriptor[] Segments { get; init; }

    /// <summary>
    /// Gets the total number of segments in the route template.
    /// </summary>
    public int SegmentCount => Segments.Length;

    /// <summary>
    /// Gets the number of parameter segments in the route template.
    /// </summary>
    /// <remarks>
    /// This value is tracked for diagnostic purposes and consumer inspection.
    /// It is not used during route sorting.
    /// </remarks>
    public int ParameterCount { get; init; }

    /// <summary>
    /// Gets the ordered array of named capture group names extracted from the compiled
    /// regular expression. Includes both outer parameter names and any inner named groups
    /// from <c>regex</c> constraints.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="GetRouteParams"/> to populate the returned
    /// <see cref="RouteParams"/> collection on each request match.
    /// </remarks>
    internal string[] CaptureGroupNames { get; init; }

    internal RouteTemplate(
        string              rawTemplate,
        Regex               compiledRegex,
        SegmentDescriptor[] segments,
        int                 parameterCount,
        string[]            captureGroupNames)
    {
        RawTemplate        = rawTemplate;
        CompiledRegex      = compiledRegex;
        Segments           = segments;
        ParameterCount     = parameterCount;
        CaptureGroupNames  = captureGroupNames;
    }

    /// <summary>
    /// Parses a route template string into a <see cref="RouteTemplate"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The template is split on <c>/</c>. Each segment is tested against the
    /// <see cref="ResolverRegistry"/>: if it resolves as a parameter, the resulting
    /// <see cref="SegmentDescriptor"/> is used directly; otherwise the segment is
    /// treated as a literal and escaped for use in a regular expression.
    /// </para>
    /// <para>
    /// The stored <see cref="RawTemplate"/> is always the normalized form: a leading
    /// slash is added if absent, and any trailing slash is removed. Both
    /// <c>/users/{id}</c> and <c>users/{id}</c> produce the same stored value of
    /// <c>/users/{id}</c>.
    /// </para>
    /// </remarks>
    /// <param name="template">The route template string to parse.</param>
    /// <returns>A fully parsed <see cref="RouteTemplate"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="template"/> is null or whitespace, when a
    /// parameter segment references an unregistered constraint, or when two segments
    /// use the same parameter name.
    /// </exception>
    public static RouteTemplate Parse(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Route template cannot be null or empty.", nameof(template));

        // Normalize to a canonical form: leading slash, no trailing slash.
        // This ensures equality comparisons are stable regardless of how the
        // consumer originally wrote the template.
        var normalized   = "/" + template.Trim('/');
        var parts        = normalized.TrimStart('/').Split('/');

        var segments          = new SegmentDescriptor[parts.Length];
        var parameterCount    = 0;
        var patternParts      = new string[parts.Length];
        var seenGroupNames    = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var captureGroupNames = new List<string>();

        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];

            if (ResolverRegistry.TryResolveSegment(part, out var descriptor))
            {
                // Compile the segment pattern in isolation to extract its named capture
                // groups. This is the only reliable way to catch duplicate names across
                // all constraint types, including custom consumer-defined resolvers.
                // The cost is paid once at startup during route registration.
                var segmentRegex = new Regex(descriptor.Pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

                foreach (var groupName in segmentRegex.GetGroupNames())
                {
                    if (int.TryParse(groupName, out _))
                        continue;

                    if (!seenGroupNames.Add(groupName))
                        throw new ArgumentException(
                            string.Format(DuplicateParameterNameMessage, normalized, groupName),
                            nameof(template));

                    captureGroupNames.Add(groupName);
                }

                segments[i]     = descriptor;
                patternParts[i] = descriptor.Pattern;
                parameterCount++;
            }
            else
            {
                // Treat as a literal segment; escape any regex metacharacters.
                var literal = SegmentDescriptor.Literal(part);
                segments[i]     = literal;
                patternParts[i] = literal.Pattern;
            }
        }

        // Anchor the combined pattern to the full path so partial matches are rejected.
        var combinedPattern = "^/" + string.Join("/", patternParts) + "$";
        var compiledRegex   = new Regex(combinedPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        return new RouteTemplate(normalized, compiledRegex, segments, parameterCount, captureGroupNames.ToArray());
    }

    /// <summary>
    /// Extracts route parameters from the specified path if it matches the route template.
    /// </summary>
    /// <remarks>
    /// Iterates <see cref="CaptureGroupNames"/> to populate the collection, which includes
    /// both outer parameter names and any inner named groups from <c>regex</c> constraints.
    /// Returns an empty collection when the path does not match.
    /// </remarks>
    /// <param name="path">The request path to match and extract parameters from.</param>
    /// <returns>
    /// A <see cref="RouteParams"/> collection containing key-value pairs for each named
    /// capture group. If the path does not match, the collection is empty.
    /// </returns>
    internal RouteParams GetRouteParams(string path)
    {
        var routeParams = new RouteParams();

        var match = CompiledRegex.Match(path);
        if (!match.Success) return routeParams;

        // Avoid LINQ on the hot path -- iterate the pre-computed array directly.
        for (var i = 0; i < CaptureGroupNames.Length; i++)
            routeParams.Add(CaptureGroupNames[i], match.Groups[CaptureGroupNames[i]].Value);

        return routeParams;
    }

    /// <summary>
    /// Compares this route template to another to determine sort order in the routing
    /// table, walking segments pairwise until a winner is found.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Comparison rules applied in order:
    /// <list type="number">
    ///   <item>
    ///     Segments are compared pairwise from left to right using
    ///     <see cref="SegmentDescriptor.CompareTo"/>. The first non-zero result
    ///     determines the sort order.
    ///   </item>
    ///   <item>
    ///     When one template has more segments than the other and all shared segments
    ///     compare equal, the longer template sorts first.
    ///   </item>
    ///   <item>
    ///     When both templates have the same number of segments and all pairwise
    ///     comparisons return zero, returns <c>0</c>. Ambiguity detection is deferred
    ///     to the containing <c>RouteDescriptor</c> comparison, where the HTTP method
    ///     is also known.
    ///   </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="other">The other <see cref="RouteTemplate"/> to compare against.</param>
    /// <param name="ignoreGroupConflicts">
    /// Passed through to each <see cref="SegmentDescriptor.CompareTo"/> call. When
    /// <see langword="true"/>, same-group constraint conflicts at the segment level are
    /// resolved silently by strictness rather than returning zero.
    /// </param>
    /// <returns>
    /// A negative integer if this template sorts before <paramref name="other"/>,
    /// a positive integer if it sorts after, or <c>0</c> if the templates are
    /// indistinguishable at the template level.
    /// </returns>
    public int CompareTo(RouteTemplate other, bool ignoreGroupConflicts)
    {
        var thisLength  = Segments.Length;
        var otherLength = other.Segments.Length;
        var minLength   = thisLength < otherLength ? thisLength : otherLength;

        for (var i = 0; i < minLength; i++)
        {
            var result = Segments[i].CompareTo(other.Segments[i], ignoreGroupConflicts);
            if (result != 0) return result;
        }

        // All shared segments compared equal. The longer template is more specific
        // and sorts first. When lengths are equal, return 0 and defer ambiguity
        // detection to RouteDescriptor where the HTTP method is also available.
        return otherLength - thisLength;
    }

    /// <summary>
    /// Determines equality based solely on the <see cref="RawTemplate"/> string
    /// using ordinal case-insensitive comparison.
    /// </summary>
    /// <param name="other">The other <see cref="RouteTemplate"/> to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both templates have the same raw template string;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool Equals(RouteTemplate? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(RawTemplate, other.RawTemplate, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(RawTemplate);
    }
}