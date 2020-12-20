using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Grapevine
{
    public class Router : IRouter
    {
        public static Func<IHttpContext, Exception, Task> DefaultErrorHandler { get; set; } = async (context, exception) =>
        {
            string content = context?.Response?.StatusCode.ToString() ?? HttpStatusCode.InternalServerError.ToString();

            if (exception != null)
            {
                content = $"Internal Server Error: {exception.Message}";
            }
            else if (context.Response.StatusCode == HttpStatusCode.NotFound)
            {
                content = $"File Not Found: {context.Request.PathInfo}";
            }
            else if (context.Response.StatusCode == HttpStatusCode.NotImplemented)
            {
                content = $"Method Not Implemented: {context.Request.PathInfo}";
            }

            await context.Response.SendResponseAsync(content);
        };

        public static Dictionary<HttpStatusCode, Func<IHttpContext, Exception, Task>> GlobalErrorHandlers =
            new Dictionary<HttpStatusCode, Func<IHttpContext, Exception, Task>>();

        public Dictionary<HttpStatusCode, Func<IHttpContext, Exception, Task>> LocalErrorHandlers =
            new Dictionary<HttpStatusCode, Func<IHttpContext, Exception, Task>>();

        /// <summary>
        /// Gets or sets a value to indicate whether request routing should continue even after a response has been sent.
        /// </summary>
        public bool ContinueRoutingAfterResponseSent { get; set; } = false;

        public bool EnableAutoScan { get; set; } = true;

        public ILogger<IRouter> Logger { get; }

        protected internal readonly IList<IRoute> RegisteredRoutes = new List<IRoute>();

        public IList<IRoute> RoutingTable => RegisteredRoutes.ToList().AsReadOnly();

        /// <summary>
        /// Gets or sets a value to indicate whether exception text, where available, should be sent in http responses
        /// </summary>
        public bool SendExceptionMessages { get; set; }

        public IServiceCollection Services { get; set; } = new ServiceCollection();

        protected internal IServiceProvider ServiceProvider { get; set; } = null;

        public event AsyncRoutingEventHandler AfterRouting;
        public event AsyncRoutingEventHandler BeforeRouting;

        public Router(ILogger<IRouter> logger)
        {
            Logger = logger ?? DefaultLogger.GetInstance<IRouter>();
        }

        public virtual IRouter Register(IRoute route)
        {
            if (RegisteredRoutes.All(r => !route.Equals(r))) RegisteredRoutes.Add(route);
            return this;
        }

        public virtual async void RouteAsync(object state)
        {
            var context = state as IHttpContext;
            if (context == null) return;

            try
            {
                Logger.LogTrace($"{context.Id} : Routing {context.Request.HttpMethod} {context.Request.PathInfo}");

                await RouteAsync(context, RoutesFor(context));
                if (!context.WasRespondedTo)
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
                Logger.LogError(e, $"An exception occured while routing request {context.Id}");
                await HandleErrorAsync(context, e);
            }
        }

        /// <summary>
        /// Routes the IHttpContext through the list of routes provided
        /// </summary>
        /// <param name="context"></param>
        /// <param name="routing"></param>
        public virtual async Task RouteAsync(IHttpContext context, IList<IRoute> routing)
        {
            // 0. If no routes are found, there is nothing to do here
            if (!(bool)routing?.Any()) return;
            Logger.LogTrace($"{context.Id} : Discovered {routing.Count} Routes");

            // 1. Create a scoped container for dependency injection
            if (ServiceProvider == null) ServiceProvider = Services.BuildServiceProvider();
            context.Services = ServiceProvider.CreateScope().ServiceProvider;

            // 2. Invoke before routing handlers
            Logger.LogTrace($"{context.Id} : Invoking before routing handlers");
            if (BeforeRouting != null) await BeforeRouting.Invoke(context);
            Logger.LogTrace($"{context.Id} : Before routing handlers invoked");

            // 3. Iterate over the routes until a response is sent
            var count = 0;
            foreach (var route in routing)
            {
                if (context.Response.StatusCode != HttpStatusCode.Ok) break;
                if (context.WasRespondedTo && !ContinueRoutingAfterResponseSent) break;
                Logger.LogTrace($"{context.Id} : Executing {route.Name}");
                await route.InvokeAsync(context);
                count++;
            }
            Logger.LogTrace($"{context.Id} : {count} of {routing.Count} routes invoked");

            // 4. Invoke after routing handlers
            Logger.LogTrace($"{context.Id} : Invoking after routing handlers");
            if (AfterRouting != null) await AfterRouting.Invoke(context);
            Logger.LogTrace($"{context.Id} : After routing handlers invoked");
        }

        /// <summary>
        /// Gets a list of registered routes that match the IHttpContext provided
        /// </summary>
        /// <param name="context"></param>
        /// <returns>IList&lt;IRoute&gt;</returns>
        public virtual IList<IRoute> RoutesFor(IHttpContext context)
        {
            return RegisteredRoutes.Where(r => r.Matches(context) && r.Enabled).ToList();
        }

        protected internal async Task HandleErrorAsync(IHttpContext context, Exception e = null)
        {
            if (context.WasRespondedTo) return;
            if (e != null && !SendExceptionMessages) e = null;

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
                await action(context, e).ConfigureAwait(false);
            }
            catch { }
        }
    }
}