using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Grapevine
{
    /// <summary>
    /// Delegate for the <see cref="IRouter.BeforeRoutingAsync"/> and <see cref="IRouter.AfterRoutingAsync"/> events
    /// </summary>
    /// <param name="context">The <see cref="IHttpContext"/> that is being routed.</param>
    public delegate Task AsyncRoutingEventHandler(IHttpContext context);

    /// <summary>
    /// Provides a mechanism to register routes and invoke them according to the produced routing table
    /// </summary>
    public interface IRouter
    {
        /// <summary>
        /// Raised after a request has completed invoking matching routes
        /// </summary>
        event AsyncRoutingEventHandler AfterRoutingAsync;

        /// <summary>
        /// Gets or sets a value that indicates whether autoscan is enabled on this router.
        /// </summary>
        bool EnableAutoScan { get; set; }

        /// <summary>
        /// Raised prior to sending any request though matching routes
        /// </summary>
        event AsyncRoutingEventHandler BeforeRoutingAsync;

        /// <summary>
        /// Gets a list of registered routes in the order they were registered
        /// </summary>
        IList<IRoute> RoutingTable { get; }

        /// <summary>
        /// Register classes to be used for dependency injection
        /// </summary>
        IServiceCollection Services { get; set; }

        /// <summary>
        /// Adds the route to the routing table
        /// </summary>
        /// <param name="route"></param>
        /// <returns>IRouter</returns>
        IRouter Register(IRoute route);

        /// <summary>
        /// Routes the IHttpContext through all enabled registered routes that match the IHttpConext provided
        /// </summary>
        /// <param name="state"></param>
        void RouteAsync(object state);
    }

    public static class IRouterExtensions
    {
        public static IRouter Register(this IRouter router, IEnumerable<IRoute> routes)
        {
            foreach(var route in routes.ToList())
            {
                router.Register(route);
            }

            return router;
        }

        public static void ConfigureServices(this IRouter router, Action<IServiceCollection> action)
        {
            action(router.Services);
        }

        public static void MaybeAutoScan(this IRouter router, IRouteScanner scanner = null, string basePath = null)
        {
            if (router.RoutingTable.Count == 0 && router.EnableAutoScan)
            {
                router.AutoScan(scanner ?? new RouteScanner(DefaultLogger.GetInstance<IRouteScanner>()), basePath);
            }
        }

        public static void AutoScan(this IRouter router, string basePath)
        {
            router.AutoScan(new RouteScanner(DefaultLogger.GetInstance<IRouteScanner>()), basePath);
        }

        public static void AutoScan(this IRouter router, IRouteScanner scanner, string basePath = null)
        {
            foreach (var route in scanner.Scan(router.Services, basePath))
            {
                router.Register(route);
            }
        }

        public static bool ConfigureOptions(this IRouter router, Action<RouterOptions> action)
        {
            var options = router as RouterOptions;
            if (options == null) return false;

            action(options);
            return true;
        }
    }
}