using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Grapevine
{
    public interface IRouteScanner
    {
        /// <summary>
        /// Generates a list of routes for all RestRoute methods in all RestResource classes found in all assemblies in the current AppDomain.
        /// </summary>
        /// <returns></returns>
        IList<IRoute> Scan(IServiceCollection services = null, string basePath = null);

        /// <summary>
        /// Generates a list of routes for all RestRoute methods in all RestResource classes found in the specified assembly.
        /// </summary>
        /// <returns></returns>
        IList<IRoute> Scan(Assembly assembly, IServiceCollection services = null, string basePath = null);
    }

    public static class IRouteScannerExtensions
    {
        public static IList<IRoute> ScanAssemblyContainingType<T>(this IRouteScanner scanner, IServiceCollection services = null, string basePath = null)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(T));
            return scanner.Scan(assembly, services, basePath);
        }
    }
}