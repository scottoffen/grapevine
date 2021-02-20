using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Grapevine
{
    /// <summary>
    /// Delegate for the <see cref="IRestServer.BeforeStarting"/>, <see cref="IRestServer.AfterStarting"/>, <see cref="IRestServer.BeforeStopping"/> and <see cref="IRestServer.AfterStopping"/> events
    /// </summary>
    /// <param name="server"></param>
    public delegate void ServerEventHandler(IRestServer server);

    /// <summary>
    /// Delegate for the <see cref="IRestServer.OnRequestAsync"/> and <see cref="IRestServer.OnResponse"/> events
    /// </summary>
    /// <param name="context"></param>
    public delegate Task RequestReceivedAsyncEventHandler(IHttpContext context, IRestServer server);

    public interface IRestServer : ILocals, IDisposable
    {
        /// <summary>
        /// Gets the list of all ContentFolder objects used for serving static content.
        /// </summary>
        /// <value></value>
        IList<IContentFolder> ContentFolders { get; }

        /// <summary>
        /// Gets or sets a list of header keys and values that should be applied to all outbound responses.
        /// </summary>
        /// <value></value>
        IList<GlobalResponseHeader> GlobalResponseHeaders { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the server is currently listening.
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Gets the logger for this IRestServer object.
        /// </summary>
        /// <value></value>
        ILogger<IRestServer> Logger { get; }

        /// <summary>
        /// Gets the server options object used by this IRestServer object
        /// </summary>
        /// <value></value>
        ServerOptions Options { get; }

        /// <summary>
        /// Gets the Uniform Resource Identifier (URI) prefixes handled by this IRestServer object.
        /// </summary>
        /// <value></value>
        IListenerPrefixCollection Prefixes { get; }

        /// <summary>
        /// Gets or sets the IRouter object to be used by this IRestServer to route incoming HTTP requests.
        /// </summary>
        IRouter Router { get; set; }

        /// <summary>
        /// Gets or sets the IRouteScanner object to be used by this IRestServer to scan for routes.
        /// </summary>
        /// <value></value>
        IRouteScanner RouteScanner { get; set; }

        /// <summary>
        /// Raised after the server has finished starting.
        /// </summary>
        event ServerEventHandler AfterStarting;

        /// <summary>
        /// Raised after the server has finished stopping.
        /// </summary>
        event ServerEventHandler AfterStopping;

        /// <summary>
        /// Raised before the server starts.
        /// </summary>
        event ServerEventHandler BeforeStarting;

        /// <summary>
        /// Raised before the server stops.
        /// </summary>
        event ServerEventHandler BeforeStopping;

        /// <summary>
        /// Raised after an incoming request is received, but before routing the request.
        /// </summary>
        event RequestReceivedAsyncEventHandler OnRequestAsync;

        /// <summary>
        /// Starts the server, raising BeforeStart and AfterStart events appropriately.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the server, raising BeforeStop and AfterStop events appropriately.
        /// </summary>
        void Stop();
    }

    public static class IRestServerExtensions
    {
        /// <summary>
        /// Applies the global headers to the provided response header collection.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="headers"></param>
        public static void ApplyGlobalResponseHeaders(this IRestServer server, WebHeaderCollection headers)
        {
            foreach (var header in server.GlobalResponseHeaders.Where(g => !g.Suppress)) headers.Add(header.Name, header.Value);
        }

        public static IRestServer SetDefaultLogger(this IRestServer server, ILoggerFactory factory)
        {
            DefaultLogger.LoggerFactory = factory;
            return server;
        }
    }
}