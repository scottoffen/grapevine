using System.Diagnostics.CodeAnalysis;

namespace Grapevine;

/// <summary>
/// Marks a class as a container for route handlers, optionally specifying a base path
/// to prepend to all route templates defined within the class.
/// </summary>
/// <remarks>
/// <para>
/// A class marked with this attribute can be discovered by a route scanner, which will
/// register all route handler methods within it. Multiple instances of this attribute
/// may be applied to the same class to register its routes under different base paths.
/// </para>
/// <para>
/// The <see cref="BasePath"/> value is automatically normalized to ensure it has a
/// single leading slash and no trailing slash.
/// </para>
/// </remarks>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RouteGroupAttribute : Attribute
{
    private string _basePath = string.Empty;

    /// <summary>
    /// Gets or sets the base path to prepend to all route templates in the class.
    /// Defaults to an empty string. The value is automatically normalized to have
    /// a single leading slash and no trailing slash.
    /// </summary>
    public string BasePath
    {
        get => _basePath.TrimPath();
        init => _basePath = value;
    }
}

/// <summary>
/// Marks a class as a REST resource containing route handler methods.
/// </summary>
/// <remarks>
/// This attribute is obsolete. Use <see cref="RouteGroupAttribute"/> instead.
/// </remarks>
[Obsolete("RestResourceAttribute is obsolete. Use RouteGroupAttribute instead.")]
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RestResourceAttribute : RouteGroupAttribute { }