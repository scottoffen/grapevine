using System;
using Microsoft.Extensions.DependencyInjection;

namespace Grapevine
{
    /// <summary>
    /// <para>Class attribute for defining a RestResource</para>
    /// <para>Targets: Class</para>
    /// <para>&#160;</para>
    /// <para>A class with the RestResource attribute can be scanned for RestRoute attributed methods by a route scanner.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RestResourceAttribute : Attribute
    {
        /// <summary>
        /// This value will be prepended to the PathInfo value on all RestRoutes in the class; defaults to an empty string.
        /// </summary>
        public string BasePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// <para>Class attribute for defining the ServiceLifetime of a RestResource</para>
    /// <para>Targets: Class</para>
    /// <para>&#160;</para>
    /// <para>When used in conjunction with the RestResource attribute, the route scanner will use the service lifetime specified when registering the type with the service collection.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ResourceLifetimeAttribute : Attribute
    {
        public ResourceLifetimeAttribute(ServiceLifetime serviceLifetime)
        {
            ServiceLifetime = serviceLifetime;
        }

        public ServiceLifetime ServiceLifetime { get; }
    }
}