using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Grapevine
{
    public abstract class RouteScannerBase : IRouteScanner
    {
        public static List<Assembly> Assemblies { get; protected set; }

        public static readonly List<string> IgnoredAssemblies = new List<string>
        {
            "vshost",
            "xunit",
            "Shouldly",
            "System",
            "Microsoft",
            "netstandard",
            "TestPlatform",
        };

        public static void AddIgnoredAssembly(string assemblyName)
        {
            IgnoredAssemblies.Add(assemblyName);
        }

        public static void AddIgnoredAssemblies(string[] assemblyNames)
        {
            IgnoredAssemblies.AddRange(assemblyNames);
        }

        public abstract IList<IRoute> Scan(IServiceCollection services = null, string basePath = null);

        public abstract IList<IRoute> Scan(Assembly assembly, IServiceCollection services = null, string basePath = null);
    }

    public class RouteScanner : RouteScannerBase, IRouteScanner
    {
        public ILogger<IRouteScanner> Logger { get; }

        public string BasePath { get; set; }

        public RouteScanner(ILogger<IRouteScanner> logger)
        {
            Logger = logger ?? DefaultLogger.GetInstance<IRouteScanner>();
        }

        public override IList<IRoute> Scan(IServiceCollection services = null, string basePath = null)
        {
            RouteScanner.PopulateAssembliesList();
            var routes = new List<IRoute>();

            Logger.LogTrace("Begin Global Route Scanning");

            foreach (var assembly in Assemblies)
            {
                try
                {
                    routes.AddRange(Scan(assembly, services, basePath));
                }
                catch (ReflectionTypeLoadException ex)
                {
                    var message = $"Exception occured when scanning for routes";
                    foreach (var loaderEx in ex.LoaderExceptions)
                    {
                        Logger.LogDebug(loaderEx, message);
                    }
                }
            }

            Logger.LogTrace($"Global Route Scanning Complete: {routes.Count} total routes found");

            return routes;
        }

        public override IList<IRoute> Scan(Assembly assembly, IServiceCollection services = null, string basePath = null)
        {
            var routes = new List<IRoute>();

            var name = assembly.GetName().FullName;
            Logger.LogTrace($"Scanning assembly {name} for routes");

            try
            {
                var types = assembly.GetTypes()
                                .Where(t => t.IsClass && t.IsDefined(typeof(RestResourceAttribute), false))
                                .OrderBy(t => t.Name);

                foreach (var type in types)
                {
                    routes.AddRange(Scan(type, services, basePath));
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var message = $"Exception occured when scanning assembly {name} for routes";
                foreach (var loaderEx in ex.LoaderExceptions)
                {
                    Logger.LogDebug(loaderEx, message);
                }
            }

            Logger.LogTrace($"Scan of assembly {name} complete: {routes.Count} total routes found");

            return routes;
        }

        public virtual IList<IRoute> Scan(Type type, IServiceCollection services = null, string basePath = null)
        {
            var routes = new List<IRoute>();
            Logger.LogTrace($"Scanning type {type.Name} for routes");

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => !m.IsAbstract && m.IsDefined(typeof(RestRouteAttribute), true));

            if (methods.Any())
            {
                var attributes = type.GetCustomAttributes(typeof(RestResourceAttribute)).Cast<RestResourceAttribute>();

                foreach (var method in methods)
                {
                    if (type.IsAbstract && !method.IsStatic) continue;

                    if (!attributes.Any())
                    {
                        routes.AddRange(Scan(method, basePath));
                        continue;
                    }

                    foreach (var attribute in attributes)
                    {
                        var basepath = attribute.BasePath ?? basePath;
                        routes.AddRange(Scan(method, basePath));
                    }
                }
            }

            if (routes.Count > 0)
            {
                if (!type.IsAbstract)
                {
                    var lifetime = type.IsDefined(typeof(ResourceLifetimeAttribute), false)
                        ? type.GetCustomAttributes(typeof(ResourceLifetimeAttribute))
                            .Cast<ResourceLifetimeAttribute>()
                            .FirstOrDefault().ServiceLifetime
                        : ServiceLifetime.Transient;

                    services.Add(new ServiceDescriptor(type, type, lifetime));
                }
            }

            Logger.LogTrace($"Scan of type {type.Name} complete: {routes.Count} total routes found");

            return routes;
        }

        public virtual IList<IRoute> Scan(MethodInfo methodInfo, string basePath = null)
        {
            var basepath = basePath ?? BasePath;
            basepath.SanitizePath();

            var routes = new List<IRoute>();
            var type = methodInfo.ReflectedType;

            Logger.LogTrace($"Scanning method {methodInfo.Name} for routes");

            foreach (var attribute in methodInfo.GetCustomAttributes(true).Where(a => a is RestRouteAttribute).Cast<RestRouteAttribute>())
            {
                // 1. Create the arguments for Route<T> constructor
                var args = attribute.GenerateRouteConstructorArguments(methodInfo, basepath);

                // 2. Create the Route instance
                var genericType = (!methodInfo.IsStatic)
                    ? typeof(Route<>).MakeGenericType(type)
                    : typeof(Route);
                var route = (IRoute)Activator.CreateInstance(genericType, args);

                // 3. Add any header matches - implementation idea that failed
                // foreach (var key in attribute.Headers.AllKeys)
                // {
                //     var vals = attribute.Headers.GetValues(key);
                //     var val = (vals.Length > 1) ? $"^({string.Join("|", vals)})$" : $"^{vals[0]}$";
                //     route.MatchOn(key, new Regex(val));
                // }

                // 4. Add route to routing table
                routes.Add(route);

                Logger.LogTrace($"Generated route {route}");
            }

            Logger.LogTrace($"Scan of method {methodInfo.Name} complete: {routes.Count} total routes found");

            return routes;
        }

        private static void PopulateAssembliesList()
        {
            Assemblies = new List<Assembly>();
            foreach (
                var assembly in
                    AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => a.GetName().Name != "Grapevine"
                            && !a.GetName().Name.StartsWith(IgnoredAssemblies.ToArray())
                        )
#if NETSTANDARD
                        .Where(a => !a.GlobalAssemblyCache)
#endif
                        .OrderBy(a => a.FullName))
                { Assemblies.Add(assembly); }
        }
    }
}