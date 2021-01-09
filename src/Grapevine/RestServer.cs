using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Grapevine
{
    public class RestServer : Locals, IRestServer
    {
        protected CancellationTokenSource TokenSource { get; set; }
        public readonly Thread RequestHandler;

        public bool IsDisposed { get; private set; }
        public bool IsStopping { get; protected set; }
        public bool IsStarting { get; protected set; }

        public HttpListener Listener { get; protected internal set; }

        public ILogger<IRestServer> Logger { get; protected internal set; }

        public IList<IContentFolder> ContentFolders { get; } = new List<IContentFolder>();

        public IList<GlobalResponseHeader> GlobalResponseHeaders { get; set; } = new List<GlobalResponseHeader>();

        public HttpContextFactory HttpContextFactory { get; set; } = (context, server, token) => new HttpContext(context, server, token);

        public bool IsListening => (bool)(Listener?.IsListening);

        public HttpListenerPrefixCollection Prefixes => Listener?.Prefixes;

        public IRouter Router { get; set; }

        public IRouteScanner RouteScanner { get; set; }

        public event ServerEventHandler AfterStarting;
        public event ServerEventHandler AfterStopping;
        public event ServerEventHandler BeforeStarting;
        public event ServerEventHandler BeforeStopping;
        public event RequestReceivedAsyncEventHandler OnRequestAsync;

        public RestServer(IRouter router, IRouteScanner scanner, ILogger<IRestServer> logger)
        {
            if (!HttpListener.IsSupported)
                throw new PlatformNotSupportedException("Windows Server 2003 (or higher) or Windows XP SP2 (or higher) is required to use instances of this class.");

            Router = router ?? new Router(DefaultLogger.GetInstance<IRouter>());
            RouteScanner = scanner ?? new RouteScanner(DefaultLogger.GetInstance<IRouteScanner>());
            Logger = logger ?? DefaultLogger.GetInstance<IRestServer>();

            Listener = new HttpListener();
            RequestHandler = new Thread(RequestListenerAsync);
        }

        public void Dispose()
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

        public void Start()
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
                Router.MaybeAutoScan(RouteScanner);

                // 4. Configure and start the listener
                Listener.Start();

                // 5. Start the listening thread
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

        public void Stop()
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
            var context = HttpContextFactory(state as HttpListenerContext, this, TokenSource.Token);
            Logger.LogTrace($"{context.Id} : Request Received {context.Request.Name}");

            // 2. Execute OnRequest event handlers
            try
            {
                Logger.LogTrace($"{context.Id} : Invoking OnRequest Handlers");
                if (OnRequestAsync != null) await OnRequestAsync.Invoke(context);
                Logger.LogTrace($"{context.Id} : OnRequest Handlers Invoked");
            }
            catch (System.Net.HttpListenerException hl) when (hl.ErrorCode == 1229)
            {
                Logger.LogDebug("The remote connection was closed before a response could be sent.");
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"An exception occured while routing request {context.Request.Name} ({context.Id})");
            }

            // 3. Optionally route request
            if (!context.WasRespondedTo)
            {
                Logger.LogTrace($"{context.Id} : Sending to router");
                ThreadPool.QueueUserWorkItem(Router.RouteAsync, context);
            }
        }
    }
}