using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Grapevine
{
    public interface IRouteScanner
    {
        /// <summary>
        /// Gets or sets a value to be used as the base path for routes created by this IRouteScanner object
        /// </summary>
        /// <value></value>
        string BasePath { get; set; }

        /// <summary>
        /// List of assemblies to be ignored when scanning for routes
        /// </summary>
        /// <value></value>
        IList<string> IgnoredAssemblies { get; }

        /// <summary>
        /// Register implementations for types scanned that contained route methods
        /// </summary>
        /// <value></value>
        IServiceCollection Services { get; set; }

        /// <summary>
        /// Scans all assemblies and returns a list of discovered routes
        /// </summary>
        /// <param name="basePath"></param>
        /// <returns></returns>
        IList<IRoute> Scan(string basePath = null);

        /// <summary>
        /// Scans the specified assembly and returns a list of discovered routes
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        IList<IRoute> Scan(Assembly assembly, string basePath = null);

        /// <summary>
        /// Scans the specified type and returns a list of discovered routes
        /// </summary>
        /// <param name="type"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        IList<IRoute> Scan(Type type, string basePath = null);

        /// <summary>
        /// Scans the method info and returns a list of discovered routes
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        IList<IRoute> Scan(MethodInfo methodInfo, string basePath = null);
    }

    public static class IRouteScannerExtensions
    {
        /// <summary>
        /// Scans the assembly containing the specified type and returns a list of discovered routes
        /// </summary>
        /// <param name="scanner"></param>
        /// <param name="services"></param>
        /// <param name="basePath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IList<IRoute> ScanAssemblyContainingType<T>(this IRouteScanner scanner, IServiceCollection services = null, string basePath = null)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(T));
            return scanner.Scan(assembly, basePath);
        }

        /// <summary>
        /// Scans the type T and returns a list of discovered routes
        /// </summary>
        /// <param name="scanner"></param>
        /// <param name="basePath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IList<IRoute> Scan<T>(this IRouteScanner scanner, string basePath = null)
        {
            return scanner.Scan(typeof(T), basePath);
        }

        /// <summary>
        /// Add an assembly to be ignored when scanning all assemblies for routes
        /// </summary>
        /// <param name="assembly"></param>
        public static void AddIgnoredAssembly(this IRouteScanner scanner, Assembly assembly)
        {
            scanner.IgnoredAssemblies.Add(assembly.GetName().Name);
        }

        /// <summary>
        /// Add a list of assemblies to be ignored when scanning all assemblies for routes
        /// </summary>
        /// <param name="assemblies"></param>
        public static void AddIgnoredAssemblies(this IRouteScanner scanner, Assembly[] assemblies)
        {
            foreach (var assembly in assemblies) scanner.IgnoredAssemblies.Add(assembly.GetName().Name);
        }

        /// <summary>
        /// Add an assembly to be ignored when scanning all assemblies for routes
        /// </summary>
        /// <param name="assemblyName"></param>
        public static void AddIgnoredAssembly(this IRouteScanner scanner, string assemblyName)
        {
            scanner.IgnoredAssemblies.Add(assemblyName);
        }

        /// <summary>
        /// Add a list of assemblies to be ignored when scanning all assemblies for routes
        /// </summary>
        /// <param name="assemblyNames"></param>
        public static void AddIgnoredAssemblies(this IRouteScanner scanner, string[] assemblyNames)
        {
            foreach (var assemblyName in assemblyNames) scanner.IgnoredAssemblies.Add(assemblyName);
        }

        public static void IgnoreAssemblyContainingType<T>(this IRouteScanner scanner)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(T));
            scanner.IgnoredAssemblies.Add(assembly.GetName().Name);
        }
    }
}