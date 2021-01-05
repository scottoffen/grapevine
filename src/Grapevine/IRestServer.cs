using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
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
    public delegate Task RequestReceivedAsyncEventHandler(IHttpContext context);

    public delegate IHttpContext HttpContextFactory(HttpListenerContext context, IRestServer server, CancellationToken token);

    public interface IRestServer : ILocals, IDisposable
    {
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
        /// Gets the list of all ContentFolder objects used for serving static content.
        /// </summary>
        IList<IContentFolder> ContentFolders { get; }

        IList<GlobalResponseHeader> GlobalResponseHeaders { get; set; }

        HttpContextFactory HttpContextFactory { get; set; }

        ILogger<IRestServer> Logger { get; }

        /// <summary>
        /// Gets a value that indicates whether the server has started listening.
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Raised after an incoming request is received, but before routing the request.
        /// </summary>
        event RequestReceivedAsyncEventHandler OnRequestAsync;

        HttpListenerPrefixCollection Prefixes { get; }

        /// <summary>
        /// Gets or sets the instance of IRouter to be used by this server to route incoming HTTP requests.
        /// </summary>
        IRouter Router { get; set; }

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
        public static void ApplyGlobalResponseHeaders(this IRestServer server, WebHeaderCollection headers)
        {
            foreach (var header in server.GlobalResponseHeaders.Where(g => !g.Suppress))
            {
                headers.Add(header.Name, header.Value);
            }
        }

        /// <summary>
        /// Starts the server and blocks the calling thread until the server shutsdown
        /// </summary>
        public static void Run(this IRestServer server)
        {
            server.Start();
            while (server.IsListening) { }
        }
    }
}