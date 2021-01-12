using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Grapevine
{
    public abstract class RestServerBase : Locals, IRestServer
    {
        public IList<IContentFolder> ContentFolders { get; } = new List<IContentFolder>();

        public IList<GlobalResponseHeader> GlobalResponseHeaders { get; set; } = new List<GlobalResponseHeader>();

        public virtual bool IsListening { get; }

        public ILogger<IRestServer> Logger { get; protected set; }

        public ServerOptions Options { get; } = new ServerOptions();

        public virtual HttpListenerPrefixCollection Prefixes { get; }

        public IRouter Router { get; set; }

        public IRouteScanner RouteScanner { get; set; }

        /// <summary>
        /// Gets or sets the CancellationTokeSource for this RestServer object.
        /// </summary>
        /// <value></value>
        protected CancellationTokenSource TokenSource { get; set; }

        public abstract event ServerEventHandler AfterStarting;
        public abstract event ServerEventHandler AfterStopping;
        public abstract event ServerEventHandler BeforeStarting;
        public abstract event ServerEventHandler BeforeStopping;
        public abstract event RequestReceivedAsyncEventHandler OnRequestAsync;

        public abstract void Dispose();

        public abstract void Start();

        public abstract void Stop();
    }

    public class RestServer : RestServerBase
    {
        /// <summary>
        /// The thread that listens for incoming requests.
        /// </summary>
        public readonly Thread RequestHandler;

        /// <summary>
        /// Gets a value that indicates whether the object has been disposed.
        /// </summary>
        /// <value></value>
        public bool IsDisposed { get; private set; }

        public override bool IsListening => (bool)(Listener?.IsListening);

        /// <summary>
        /// Gets a value that indicates whether the server is in the process of stopping.
        /// </summary>
        /// <value></value>
        public bool IsStopping { get; protected set; }

        /// <summary>
        /// Gets a value that indicates whether the server is in the process of starting.
        /// </summary>
        /// <value></value>
        public bool IsStarting { get; protected set; }

        /// <summary>
        /// Gets the HttpListener object used by this RestServer object.
        /// </summary>
        /// <value></value>
        public HttpListener Listener { get; protected internal set; }

        public override HttpListenerPrefixCollection Prefixes => Listener?.Prefixes;

        public override event ServerEventHandler AfterStarting;
        public override event ServerEventHandler AfterStopping;
        public override event ServerEventHandler BeforeStarting;
        public override event ServerEventHandler BeforeStopping;
        public override event RequestReceivedAsyncEventHandler OnRequestAsync;

        public RestServer(IRouter router, IRouteScanner scanner, ILogger<IRestServer> logger)
        {
            if (!HttpListener.IsSupported)
                throw new PlatformNotSupportedException("Windows Server 2003 (or higher) or Windows XP SP2 (or higher) is required to use instances of this class.");

            Router = router ?? new Router(DefaultLogger.GetInstance<IRouter>());
            RouteScanner = scanner ?? new RouteScanner(DefaultLogger.GetInstance<IRouteScanner>());
            Logger = logger ?? DefaultLogger.GetInstance<IRestServer>();

            RouteScanner.Services = Router.Services;

            Listener = new HttpListener();
            RequestHandler = new Thread(RequestListenerAsync);
        }

        public override void Dispose()
        {
            if (IsDisposed) return;

            try
            {
                Stop();
                Listener.Close();
                TokenSource?.Dispose();
            }
            finally
            {
                IsDisposed = true;
            }
        }

        public override void Start()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (IsListening || IsStarting || IsStopping) return;
            IsStarting = true;

            var exceptionWasThrown = false;

            try
            {
                // 1. Reset CancellationTokenSource
                TokenSource?.Dispose();
                TokenSource = new CancellationTokenSource();

                // 2. Execute BeforeStarting event handlers
                BeforeStarting?.Invoke(this);

                // 3. Optionally autoscan for routes
                if (Router.RoutingTable.Count == 0 && Options.EnableAutoScan)
                    Router.Register(RouteScanner.Scan());

                // 4. Configure and start the listener
                Listener.Start();

                // 5. Start the request handler thread
                RequestHandler.Start();

                // 6. Execute AfterStarting event handlers
                AfterStarting?.Invoke(this);
            }
            catch (HttpListenerException hl) when (hl.ErrorCode == 32)
            {
                /*
                * When the port you are attempting to bind to is already in use
                * by another application, the error can sometimes be unintuitive.
                */

                exceptionWasThrown = true;

                var message = $"One or more ports are already in use by another application.";
                var exception = new ArgumentException(message, hl);

                Logger.LogCritical(exception, message);
                throw exception;
            }
            catch (Exception e)
            {
                exceptionWasThrown = true;

                Logger.LogCritical(e, "An unexpected error occured when attempting to start the server");
                throw;
            }
            finally
            {
                if (exceptionWasThrown)
                {
                    Listener.Stop();
                    TokenSource.Cancel();
                }

                IsStarting = false;
            }
        }

        public override void Stop()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (IsStopping || IsStarting) return;
            IsStopping = true;

            try
            {
                // 1. Execute BeforeStopping event handlers
                BeforeStopping?.Invoke(this);

                // 2. Stop the listener
                Listener?.Stop();

                // 3. Complete or cancel running routes
                TokenSource?.Cancel();

                // 4. Execute AfterStopping event handlers
                AfterStopping?.Invoke(this);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                IsStopping = false;
            }
        }

        protected async void RequestListenerAsync()
        {
            while (Listener.IsListening)
            {
                try
                {
                    var context = await Listener.GetContextAsync();
                    ThreadPool.QueueUserWorkItem(RequestHandlerAsync, context);
                }
                catch (HttpListenerException hl) when (hl.ErrorCode == 995 && (IsStopping || !IsListening))
                {
                    /*
                    * Ignore exceptions thrown by incomplete async methods listening for
                    * incoming requests during shutdown
                    */
                }
                catch (ObjectDisposedException) when (IsDisposed)
                {
                    /*
                    * Ignore object disposed exceptions thrown during shutdown
                    * see: https://stackoverflow.com/a/13352359
                    */
                }
                catch (Exception e)
                {
                    Logger.LogDebug(e, "An unexpected error occured while listening for incoming requests.");
                }
            }
        }

        protected async void RequestHandlerAsync(object state)
        {
            // 1. Create context
            var context = Options.HttpContextFactory(state, this, TokenSource.Token);
            Logger.LogTrace($"{context.Id} : Request Received {context.Request.Name}");

            // 2. Execute OnRequest event handlers
            try
            {
                Logger.LogTrace($"{context.Id} : Invoking OnRequest Handlers for {context.Request.Name}");
                if (OnRequestAsync != null) await OnRequestAsync.Invoke(context);
                Logger.LogTrace($"{context.Id} : OnRequest Handlers Invoked for {context.Request.Name}");
            }
            catch (System.Net.HttpListenerException hl) when (hl.ErrorCode == 1229)
            {
                Logger.LogDebug($"{context.Id} : The remote connection was closed before a response could be sent for {context.Request.Name}.");
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"{context.Id} An exception occured while routing request {context.Request.Name}");
            }

            // 3. Optionally route request
            if (!context.WasRespondedTo)
            {
                Logger.LogTrace($"{context.Id} : Routing request {context.Request.Name}");
                ThreadPool.QueueUserWorkItem(Router.RouteAsync, context);
            }
        }
    }
}