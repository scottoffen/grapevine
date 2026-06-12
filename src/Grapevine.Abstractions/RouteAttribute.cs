using System.Diagnostics.CodeAnalysis;

namespace Grapevine;

/// <summary>
/// Marks a method as a route handler, optionally specifying the HTTP method and
/// route template it responds to.
/// </summary>
/// <remarks>
/// A method marked with this attribute can be discovered by a route scanner and
/// registered as a route handler. Multiple instances may be applied to the same
/// method to register it under different HTTP methods or route templates.
/// </remarks>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RouteAttribute : Attribute
{
    /// <summary>
    /// Gets or sets an optional description for the route.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the route is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets the HTTP method this route responds to. Defaults to
    /// <see cref="HttpMethod.Any"/>.
    /// </summary>
    public HttpMethod HttpMethod { get; init; } = HttpMethod.Any;

    /// <summary>
    /// Gets the unique name for this route, used internally for identification.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the route template this route responds to. Defaults to an empty string,
    /// which matches any path.
    /// </summary>
    public string RouteTemplate { get; init; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of <see cref="RouteAttribute"/> with the specified
    /// HTTP method and route template.
    /// </summary>
    /// <param name="httpMethod">
    /// The HTTP method to match, e.g. <c>"GET"</c>. Defaults to
    /// <see cref="HttpMethod.Any"/> if not specified.
    /// </param>
    /// <param name="routeTemplate">
    /// The route template to match, e.g. <c>"/api/users"</c>. Defaults to an empty
    /// string, which matches any path.
    /// </param>
    public RouteAttribute(string? httpMethod = null, string? routeTemplate = null)
    {
        HttpMethod = httpMethod ?? HttpMethod.Any;
        RouteTemplate = routeTemplate ?? string.Empty;
    }
}

/// <summary>
/// Marks a method as a route handler that responds to HTTP GET requests.
/// </summary>
/// <remarks>
/// Equivalent to <c>[Route("GET", routeTemplate)]</c>.
/// </remarks>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HttpGetAttribute : RouteAttribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="HttpGetAttribute"/> with an optional
    /// route template.
    /// </summary>
    /// <param name="routeTemplate">
    /// The route template to match. Defaults to an empty string, which matches any path.
    /// </param>
    public HttpGetAttribute(string? routeTemplate = null) : base("GET", routeTemplate) { }
}

/// <summary>
/// Marks a method as a route handler that responds to HTTP POST requests.
/// </summary>
/// <remarks>
/// Equivalent to <c>[Route("POST", routeTemplate)]</c>.
/// </remarks>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HttpPostAttribute : RouteAttribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="HttpPostAttribute"/> with an optional
    /// route template.
    /// </summary>
    /// <param name="routeTemplate">
    /// The route template to match. Defaults to an empty string, which matches any path.
    /// </param>
    public HttpPostAttribute(string? routeTemplate = null) : base("POST", routeTemplate) { }
}

/// <summary>
/// Marks a method as a route handler that responds to HTTP PUT requests.
/// </summary>
/// <remarks>
/// Equivalent to <c>[Route("PUT", routeTemplate)]</c>.
/// </remarks>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HttpPutAttribute : RouteAttribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="HttpPutAttribute"/> with an optional
    /// route template.
    /// </summary>
    /// <param name="routeTemplate">
    /// The route template to match. Defaults to an empty string, which matches any path.
    /// </param>
    public HttpPutAttribute(string? routeTemplate = null) : base("PUT", routeTemplate) { }
}

/// <summary>
/// Marks a method as a route handler that responds to HTTP DELETE requests.
/// </summary>
/// <remarks>
/// Equivalent to <c>[Route("DELETE", routeTemplate)]</c>.
/// </remarks>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HttpDeleteAttribute : RouteAttribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="HttpDeleteAttribute"/> with an optional
    /// route template.
    /// </summary>
    /// <param name="routeTemplate">
    /// The route template to match. Defaults to an empty string, which matches any path.
    /// </param>
    public HttpDeleteAttribute(string? routeTemplate = null) : base("DELETE", routeTemplate) { }
}

/// <summary>
/// Marks a method as a route handler that responds to HTTP PATCH requests.
/// </summary>
/// <remarks>
/// Equivalent to <c>[Route("PATCH", routeTemplate)]</c>.
/// </remarks>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HttpPatchAttribute : RouteAttribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="HttpPatchAttribute"/> with an optional
    /// route template.
    /// </summary>
    /// <param name="routeTemplate">
    /// The route template to match. Defaults to an empty string, which matches any path.
    /// </param>
    public HttpPatchAttribute(string? routeTemplate = null) : base("PATCH", routeTemplate) { }
}

/// <summary>
/// Marks a method as a route handler that responds to HTTP HEAD requests.
/// </summary>
/// <remarks>
/// Equivalent to <c>[Route("HEAD", routeTemplate)]</c>.
/// </remarks>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HttpHeadAttribute : RouteAttribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="HttpHeadAttribute"/> with an optional
    /// route template.
    /// </summary>
    /// <param name="routeTemplate">
    /// The route template to match. Defaults to an empty string, which matches any path.
    /// </param>
    public HttpHeadAttribute(string? routeTemplate = null) : base("HEAD", routeTemplate) { }
}

/// <summary>
/// Marks a method as a route handler that responds to HTTP OPTIONS requests.
/// </summary>
/// <remarks>
/// Equivalent to <c>[Route("OPTIONS", routeTemplate)]</c>.
/// </remarks>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HttpOptionsAttribute : RouteAttribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="HttpOptionsAttribute"/> with an optional
    /// route template.
    /// </summary>
    /// <param name="routeTemplate">
    /// The route template to match. Defaults to an empty string, which matches any path.
    /// </param>
    public HttpOptionsAttribute(string? routeTemplate = null) : base("OPTIONS", routeTemplate) { }
}

/// <summary>
/// Marks a method as a REST route handler.
/// </summary>
/// <remarks>
/// This attribute is obsolete. Use <see cref="RouteAttribute"/> instead.
/// </remarks>
[ExcludeFromCodeCoverage]
[Obsolete("RestRouteAttribute is obsolete. Use RouteAttribute instead.")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RestRouteAttribute : RouteAttribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="RestRouteAttribute"/>.
    /// </summary>
    public RestRouteAttribute() : base() { }

    /// <summary>
    /// Initializes a new instance of <see cref="RestRouteAttribute"/> with the
    /// specified HTTP method.
    /// </summary>
    /// <param name="httpMethod">The HTTP method to match.</param>
    public RestRouteAttribute(string httpMethod) : base(httpMethod) { }

    /// <summary>
    /// Initializes a new instance of <see cref="RestRouteAttribute"/> with the
    /// specified HTTP method and route template.
    /// </summary>
    /// <param name="httpMethod">The HTTP method to match.</param>
    /// <param name="routeTemplate">The route template to match.</param>
    public RestRouteAttribute(string httpMethod, string routeTemplate)
        : base(httpMethod, routeTemplate) { }
}