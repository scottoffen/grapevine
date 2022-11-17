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
    public delegate Task RoutingAsyncEventHandler(IHttpContext context);

    /// <summary>
    /// Provides a mechanism to register routes and invoke them according to the produced routing table
    /// </summary>
    public interface IRouter
    {
        /// <summary>
        /// Gets a unique identifier for this IRouter object
        /// </summary>
        /// <value></value>
        string Id { get; }

        /// <summary>
        /// Gets the router options object used by this IRouter object
        /// </summary>
        /// <value></value>
        RouterOptions Options { get; }

        /// <summary>
        /// Gets a list of registered routes in the order they were registered
        /// </summary>
        IList<IRoute> RoutingTable { get; }

        /// <summary>
        /// Register abstracts and/or implementations to be used for dependency injection
        /// </summary>
        IServiceCollection Services { get; set; }

        /// <summary>
        /// The mechanism for retrieving a service object; that is, an object that provides custom support to other objects
        /// </summary>
        /// <value></value>
        IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Raised after a request has completed invoking matching routes
        /// </summary>
        RequestRoutingEvent AfterRoutingAsync { get; set; }

        /// <summary>
        /// Raised prior to sending any request though matching routes
        /// </summary>
        RequestRoutingEvent BeforeRoutingAsync { get; set; }

        /// <summary>
        /// Adds the route to the routing table
        /// </summary>
        /// <param name="route"></param>
        /// <returns>IRouter</returns>
        IRouter Register(IRoute route);

        /// <summary>
        /// Routes the IHttpContext through all enabled registered routes that match the (IHttpContext)state provided
        /// </summary>
        /// <param name="state"></param>
        void RouteAsync(object state);
    }

    public static partial class IRouterExtensions
    {
        /// <summary>
        /// Adds all the routes specified to the routing table
        /// </summary>
        /// <param name="router"></param>
        /// <param name="routes"></param>
        /// <returns></returns>
        public static IRouter Register(this IRouter router, IEnumerable<IRoute> routes)
        {
            foreach (var route in routes.ToList())
            {
                router.Register(route);
            }

            return router;
        }
    }

    public static partial class IRouterExtensions
    {
        private static readonly IDictionary<string, IServiceProvider> _providers = new Dictionary<string, IServiceProvider>();

        public static IServiceProvider GetServiceProvider(this IRouter router)
        {
            if (!_providers.ContainsKey(router.Id)) _providers.Add(router.Id, router.Services.BuildServiceProvider());
            return _providers[router.Id];
        }
    }
}