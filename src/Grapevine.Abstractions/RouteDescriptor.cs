using System.Diagnostics;
using Grapevine;

namespace Grapevine.Abstractions;

/// <summary>
/// Describes a single route in the routing table, pairing an <see cref="Grapevine.HttpMethod"/>
/// with a <see cref="Grapevine.Abstractions.RouteTemplate"/> to determine whether an incoming
/// request should be handled by this route.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="Matches"/> to test whether an incoming request matches this descriptor.
/// Use <see cref="GetRouteParams"/> to extract route parameter values from the request path
/// after a successful match.
/// </para>
/// <para>
/// Route descriptors are sorted during routing table construction using
/// <see cref="CompareTo"/>. More specific routes sort before less specific ones.
/// Routes with a specific <see cref="HttpMethod"/> always sort before routes using
/// <see cref="Grapevine.HttpMethod.Any"/>.
/// </para>
/// </remarks>
[DebuggerDisplay("{HttpMethod.Name} {RouteTemplate.RawTemplate}")]
public sealed class RouteDescriptor
{
    internal static readonly string AmbiguousRouteDescriptorMessage =
        "Ambiguous routes: '{0} {1}' and '{2} {3}' are indistinguishable and cannot be sorted.";

    /// <summary>
    /// Gets the HTTP method this route responds to. Use <see cref="Grapevine.HttpMethod.Any"/>
    /// to match any method.
    /// </summary>
    public HttpMethod HttpMethod { get; }

    /// <summary>
    /// Gets the route template used to match incoming request paths.
    /// </summary>
    public RouteTemplate RouteTemplate { get; }

    /// <summary>
    /// Initializes a new instance that matches any HTTP method and any path.
    /// </summary>
    public RouteDescriptor()
        : this(HttpMethod.Any, RouteTemplate.CatchAll) { }

    /// <summary>
    /// Initializes a new instance that matches the specified HTTP method and any path.
    /// </summary>
    /// <param name="method">The HTTP method to match.</param>
    public RouteDescriptor(HttpMethod method)
        : this(method, RouteTemplate.CatchAll) { }

    /// <summary>
    /// Initializes a new instance that matches any HTTP method and the specified route template.
    /// </summary>
    /// <param name="template">The route template string to match against incoming request paths.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="template"/> is null or whitespace, references an unregistered
    /// constraint, or contains duplicate parameter names.
    /// </exception>
    public RouteDescriptor(string template)
        : this(HttpMethod.Any, RouteTemplate.Parse(template)) { }

    /// <summary>
    /// Initializes a new instance that matches the specified HTTP method and route template.
    /// </summary>
    /// <param name="method">The HTTP method to match.</param>
    /// <param name="template">The route template string to match against incoming request paths.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="template"/> is null or whitespace, references an unregistered
    /// constraint, or contains duplicate parameter names.
    /// </exception>
    public RouteDescriptor(HttpMethod method, string template)
        : this(method, RouteTemplate.Parse(template)) { }

    private RouteDescriptor(HttpMethod method, RouteTemplate routeTemplate)
    {
        HttpMethod     = method;
        RouteTemplate  = routeTemplate;
    }

    /// <summary>
    /// Determines whether this route matches the specified request, checking both the
    /// HTTP method and the request path against the route template.
    /// </summary>
    /// <param name="request">The incoming HTTP request to test.</param>
    /// <returns>
    /// <see langword="true"/> if the request method matches and the request path matches
    /// the route template; otherwise <see langword="false"/>.
    /// </returns>
    public bool Matches(IHttpRequest request)
    {
        if (!HttpMethod.Matches(request.HttpMethod)) return false;
        return RouteTemplate.CompiledRegex.IsMatch(request.Endpoint);
    }

    /// <summary>
    /// Extracts route parameter values from the specified path.
    /// </summary>
    /// <param name="path">The request path to extract parameters from.</param>
    /// <returns>
    /// A <see cref="RouteParams"/> collection containing key-value pairs for each named
    /// capture group. If the path does not match, the collection is empty.
    /// </returns>
    public RouteParams GetRouteParams(string path)
        => RouteTemplate.GetRouteParams(path);

    /// <summary>
    /// Compares this route descriptor to another to determine sort order in the routing table.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sort rules applied in order:
    /// <list type="number">
    ///   <item>
    ///     Routes using <see cref="Grapevine.HttpMethod.Any"/> sort after routes with a
    ///     specific method. Otherwise, routes are sorted alphabetically by method name.
    ///   </item>
    ///   <item>
    ///     When both routes have the same HTTP method, sort by
    ///     <see cref="RouteTemplate.CompareTo"/>.
    ///   </item>
    ///   <item>
    ///     When both the method and template comparisons return equal, the routes are
    ///     ambiguous and an <see cref="InvalidOperationException"/> is thrown.
    ///   </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="other">The other <see cref="RouteDescriptor"/> to compare against.</param>
    /// <param name="ignoreGroupConflicts">
    /// Passed through to <see cref="RouteTemplate.CompareTo"/>. When <see langword="true"/>,
    /// same-group constraint conflicts at the segment level are resolved silently by strictness.
    /// </param>
    /// <returns>
    /// A negative integer if this descriptor sorts before <paramref name="other"/>,
    /// or a positive integer if it sorts after.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when both descriptors have the same HTTP method and their route templates
    /// are indistinguishable.
    /// </exception>
    public int CompareTo(RouteDescriptor other, bool ignoreGroupConflicts)
    {
        // Routes with Any sort after routes with a specific method. When one is Any
        // and the other is not, Any loses (sorts later).
        var thisIsAny  = HttpMethod == HttpMethod.Any;
        var otherIsAny = other.HttpMethod == HttpMethod.Any;

        if (thisIsAny && !otherIsAny) return 1;
        if (!thisIsAny && otherIsAny) return -1;

        // Both have specific methods or both are Any -- sort alphabetically by name.
        var methodComparison = string.Compare(
            HttpMethod.Name,
            other.HttpMethod.Name,
            StringComparison.OrdinalIgnoreCase);

        if (methodComparison != 0) return methodComparison;

        // Same method -- defer to route template comparison.
        var templateComparison = RouteTemplate.CompareTo(other.RouteTemplate, ignoreGroupConflicts);
        if (templateComparison != 0) return templateComparison;

        // Same method and indistinguishable templates -- truly ambiguous.
        throw new InvalidOperationException(string.Format(
            AmbiguousRouteDescriptorMessage,
            HttpMethod.Name, RouteTemplate.RawTemplate,
            other.HttpMethod.Name, other.RouteTemplate.RawTemplate));
    }
}