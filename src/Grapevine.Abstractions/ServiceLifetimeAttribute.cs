using Microsoft.Extensions.DependencyInjection;

namespace Grapevine;

/// <summary>
/// Specifies the <see cref="Microsoft.Extensions.DependencyInjection.ServiceLifetime"/> to use
/// when registering a route group class with the dependency injection container during
/// assembly scanning.
/// </summary>
/// <remarks>
/// When applied to a class also marked with <see cref="RouteGroupAttribute"/>, the route
/// scanner will use the specified lifetime when registering the type with the service
/// collection. When omitted, the scanner uses its default lifetime.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ServiceLifetimeAttribute : Attribute
{
    /// <summary>
    /// Gets the service lifetime to use when registering this class with the
    /// dependency injection container.
    /// </summary>
    public ServiceLifetime ServiceLifetime { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ServiceLifetimeAttribute"/> with the
    /// specified service lifetime.
    /// </summary>
    /// <param name="serviceLifetime">
    /// The <see cref="Microsoft.Extensions.DependencyInjection.ServiceLifetime"/> to use
    /// when registering this class with the dependency injection container.
    /// </param>
    public ServiceLifetimeAttribute(ServiceLifetime serviceLifetime)
    {
        ServiceLifetime = serviceLifetime;
    }
}

/// <summary>
/// Specifies the service lifetime to use when registering a REST resource with the
/// dependency injection container during assembly scanning.
/// </summary>
/// <remarks>
/// This attribute is obsolete. Use <see cref="ServiceLifetimeAttribute"/> instead.
/// </remarks>
[Obsolete("ResourceLifetimeAttribute is obsolete. Use ServiceLifetimeAttribute instead.")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ResourceLifetimeAttribute : ServiceLifetimeAttribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="ResourceLifetimeAttribute"/> with the
    /// specified service lifetime.
    /// </summary>
    /// <param name="serviceLifetime">
    /// The <see cref="Microsoft.Extensions.DependencyInjection.ServiceLifetime"/> to use
    /// when registering this class with the dependency injection container.
    /// </param>
    public ResourceLifetimeAttribute(ServiceLifetime serviceLifetime) : base(serviceLifetime) { }
}