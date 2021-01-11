using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Grapevine
{
    /// <summary>
    /// Delegate for <see cref="Router.GlobalErrorHandlers"/> and <see cref="Router.LocalErrorHandlers"/>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public delegate Task HandleErrorAsync(IHttpContext context, Exception e);

    public abstract class RouterBase : IRouter
    {
        /// <summary>
        /// Gets or sets the default routing error handler
        /// </summary>
        /// <returns></returns>
        public static HandleErrorAsync DefaultErrorHandler { get; set; } = async (context, exception) =>
        {
            string content = context?.Response?.StatusCode.ToString() ?? HttpStatusCode.InternalServerError.ToString();

            if (exception != null)
            {
                content = $"Internal Server Error: {exception.Message}";
            }
            else if (context.Response.StatusCode == HttpStatusCode.NotFound)
            {
                content = $"File Not Found: {context.Request.Endpoint}";
            }
            else if (context.Response.StatusCode == HttpStatusCode.NotImplemented)
            {
                content = $"Method Not Implemented: {context.Request.Name}";
            }

            await context.Response.SendResponseAsync(content);
        };

        /// <summary>
        /// Collection of global error handlers keyed by HTTP status code
        /// </summary>
        /// <typeparam name="HttpStatusCode"></typeparam>
        /// <typeparam name="HandleErrorAsync"></typeparam>
        /// <returns></returns>
        public static Dictionary<HttpStatusCode, HandleErrorAsync> GlobalErrorHandlers =
            new Dictionary<HttpStatusCode, HandleErrorAsync>();

        /// <summary>
        /// Collection of error handlers specific to this Router object
        /// </summary>
        /// <typeparam name="HttpStatusCode"></typeparam>
        /// <typeparam name="HandleErrorAsync"></typeparam>
        /// <returns></returns>
        public Dictionary<HttpStatusCode, HandleErrorAsync> LocalErrorHandlers =
            new Dictionary<HttpStatusCode, HandleErrorAsync>();

        public RouterOptions Options { get; } = new RouterOptions();

        public abstract IList<IRoute> RoutingTable { get; }

        public IServiceCollection Services { get; set; }

        public abstract event RoutingAsyncEventHandler AfterRoutingAsync;
        public abstract event RoutingAsyncEventHandler BeforeRoutingAsync;

        public abstract IRouter Register(IRoute route);

        public abstract void RouteAsync(object state);

        /// <summary>
        /// Asychronously determines which error handler to invoke and invokes with the given context and exception
        /// </summary>
        /// <param name="context"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected internal async Task HandleErrorAsync(IHttpContext context, Exception e = null)
        {
            if (context.WasRespondedTo) return;

            if (context.Response.StatusCode == HttpStatusCode.Ok)
                context.Response.StatusCode = HttpStatusCode.InternalServerError;

            if (!LocalErrorHandlers.ContainsKey(context.Response.StatusCode))
            {
                LocalErrorHandlers[context.Response.StatusCode] = GlobalErrorHandlers.ContainsKey(context.Response.StatusCode)
                    ? GlobalErrorHandlers[context.Response.StatusCode]
                    : DefaultErrorHandler;
            }

            var action = LocalErrorHandlers[context.Response.StatusCode];

            try
            {
                await action(context, (Options.SendExceptionMessages) ? e : null).ConfigureAwait(false);
            }
            catch { }
        }
    }

    public class Router : RouterBase
    {
        /// <summary>
        /// Gets the logger for this Router object.
        /// </summary>
        /// <value></value>
        public ILogger<IRouter> Logger { get; }

        /// <summary>
        /// List of all registered routes.
        /// </summary>
        /// <typeparam name="IRoute"></typeparam>
        /// <returns></returns>
        protected internal readonly IList<IRoute> RegisteredRoutes = new List<IRoute>();

        public override IList<IRoute> RoutingTable => RegisteredRoutes.ToList().AsReadOnly();

        /// <summary>
        /// The mechanism for retrieving a service object; that is, an object that provides custom support to other objects
        /// </summary>
        /// <value></value>
        protected internal IServiceProvider ServiceProvider { get; set; } = null;

        public override event RoutingAsyncEventHandler AfterRoutingAsync;
        public override event RoutingAsyncEventHandler BeforeRoutingAsync;

        public Router(ILogger<IRouter> logger)
        {
            Logger = logger ?? DefaultLogger.GetInstance<IRouter>();
        }

        public override IRouter Register(IRoute route)
        {
            if (RegisteredRoutes.All(r => !route.Equals(r))) RegisteredRoutes.Add(route);
            return this;
        }

        public override async void RouteAsync(object state)
        {
            var context = state as IHttpContext;
            if (context == null) return;

            try
            {
                Logger.LogDebug($"{context.Id} : Routing {context.Request.Name}");

                var routesExecuted = await RouteAsync(context);
                if (routesExecuted == 0 || ((context.Response.StatusCode != HttpStatusCode.Ok || Options.RequireRouteResponse) && !context.WasRespondedTo))
                {
                    if (context.Response.StatusCode == HttpStatusCode.Ok)
                        context.Response.StatusCode = HttpStatusCode.NotImplemented;
                    await HandleErrorAsync(context);
                }
            }
            catch (HttpListenerException hl) when (hl.ErrorCode == 1229)
            {
                Logger.LogDebug("The remote connection was closed before a response could be sent.");
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"{context.Id}: An exception occured while routing request {context.Request.Name}");
                await HandleErrorAsync(context, e);
            }
        }

        /// <summary>
        /// Routes the IHttpContext through matching routes
        /// </summary>
        /// <param name="context"></param>
        public virtual async Task<int> RouteAsync(IHttpContext context)
        {
            // 0. If no routes are found, there is nothing to do here
            var routing = RoutesFor(context);
            if (!routing.Any()) return 0;
            Logger.LogDebug($"{context.Id} : Matching routes discovered for {context.Request.Name}");

            // 1. Create a scoped container for dependency injection
            if (ServiceProvider == null) ServiceProvider = Services.BuildServiceProvider();
            context.Services = ServiceProvider.CreateScope().ServiceProvider;

            // 2. Invoke before routing handlers
            Logger.LogTrace($"{context.Id} : Invoking before routing handlers for {context.Request.Name}");
            if (BeforeRoutingAsync != null) await BeforeRoutingAsync.Invoke(context);
            Logger.LogTrace($"{context.Id} : Before routing handlers invoked for {context.Request.Name}");

            // 3. Iterate over the routes until a response is sent
            var count = 0;
            foreach (var route in routing)
            {
                if (context.Response.StatusCode != HttpStatusCode.Ok) break;
                if (context.WasRespondedTo && !Options.ContinueRoutingAfterResponseSent) break;
                Logger.LogDebug($"{context.Id} : Executing {route.Name} for {context.Request.Name}");
                await route.InvokeAsync(context);
                count++;
            }
            Logger.LogDebug($"{context.Id} : {count} of {routing.Count()} routes invoked");

            // 4. Invoke after routing handlers
            Logger.LogTrace($"{context.Id} : Invoking after routing handlers for {context.Request.Name}");
            if (AfterRoutingAsync != null) await AfterRoutingAsync.Invoke(context);
            Logger.LogTrace($"{context.Id} : After routing handlers invoked for {context.Request.Name}");

            return count;
        }

        /// <summary>
        /// Gets an enumeration of registered routes that match the IHttpContext provided
        /// </summary>
        /// <param name="context"></param>
        /// <returns>IEnumerable&lt;IRoute&gt;</returns>
        public virtual IEnumerable<IRoute> RoutesFor(IHttpContext context)
        {
            foreach (var route in RegisteredRoutes.Where(r => r.IsMatch(context) && r.Enabled)) yield return route;
        }
    }
}