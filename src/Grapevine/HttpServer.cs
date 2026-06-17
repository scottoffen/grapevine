using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Channels;

namespace Grapevine;

// =============================================================================
// Fields and construction
// =============================================================================

/// <summary>
/// An embedded HTTP server built on top of <see cref="HttpListener"/> that
/// listens on one or more URI prefixes and processes incoming requests through
/// a middleware pipeline via an internal channel.
/// </summary>
/// <remarks>
/// <para>
/// Configure the server by adding prefixes to <see cref="Prefixes"/> and then
/// calling <see cref="Start"/> or <see cref="StartAsync"/>. The server seals
/// <see cref="Prefixes"/> on start and unseals them on stop, so prefixes can
/// only be modified while the server is not listening.
/// </para>
/// <para>
/// Incoming requests are dropped onto a bounded channel for processing by the
/// middleware pipeline. The channel capacity is configured via
/// <see cref="HttpServerOptions"/>.
/// </para>
/// <para>
/// The lifecycle events on this class are observability hooks intended for
/// logging, diagnostics, and UI integrations. They are not middleware extension
/// points and should not be used to participate in request processing.
/// </para>
/// </remarks>
[ExcludeFromCodeCoverage]
public partial class HttpServer : IHttpServer
{
    private readonly HttpListener _listener;
    private readonly Channel<IHttpContext> _channel;
    private readonly TimeSpan _shutdownTimeout;
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);

    private CancellationTokenSource? _cts;
    private Task? _requestLoopTask;
    private bool _disposed;

    private ServerState _state = ServerState.Stopped;

    /// <summary>
    /// Initializes a new instance of <see cref="HttpServer"/> with default
    /// options.
    /// </summary>
    public HttpServer() : this(new HttpServerOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="HttpServer"/> with the
    /// specified options.
    /// </summary>
    /// <param name="options">
    /// The options used to configure the server and the underlying
    /// <see cref="HttpListener"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    public HttpServer(HttpServerOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        _listener = new HttpListener();
        _listener.AuthenticationSchemes = options.AuthenticationSchemes;
        _listener.AuthenticationSchemeSelectorDelegate = options.AuthenticationSchemeSelectorDelegate;
        _listener.IgnoreWriteExceptions = options.IgnoreWriteExceptions;
        _listener.UnsafeConnectionNtlmAuthentication = options.UnsafeConnectionNtlmAuthentication;

        if (!string.IsNullOrWhiteSpace(options.Realm))
            _listener.Realm = options.Realm;

        _channel = Channel.CreateBounded<IHttpContext>(new BoundedChannelOptions(options.ChannelCapacity)
        {
            // Single writer: only the request loop writes to the channel.
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        _shutdownTimeout = options.ShutdownTimeout;

        Prefixes = new PrefixCollection();

        // Pre-populate prefixes from options, skipping any that are invalid.
        foreach (var prefix in options.Prefixes)
        {
            if (Prefixes.TryAdd(prefix))
                _listener.Prefixes.Add(prefix);
        }
    }
}

// =============================================================================
// IHttpServer properties
// =============================================================================

public partial class HttpServer
{
    /// <inheritdoc/>
    public PrefixCollection Prefixes { get; }

    /// <inheritdoc/>
    public bool IsListening => _state == ServerState.Listening;

    /// <summary>
    /// Gets the channel reader that the middleware pipeline reads
    /// <see cref="IHttpContext"/> instances from.
    /// </summary>
    /// <remarks>
    /// The pipeline consumer should read from this channel and process each
    /// context through the middleware pipeline. The channel is completed when
    /// the server stops.
    /// </remarks>
    public ChannelReader<IHttpContext> ContextChannel => _channel.Reader;
}

// =============================================================================
// Lifecycle methods
// =============================================================================

public partial class HttpServer
{
    /// <inheritdoc/>
    public void Start()
    {
        StartAsync().GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // No-op if already listening or starting.
            if (_state != ServerState.Stopped) return;

            _state = ServerState.Starting;

            FireEvent(BeforeStarting);

            // Sync prefixes added after construction to the underlying listener.
            foreach (var prefix in Prefixes)
            {
                if (!_listener.Prefixes.Contains(prefix))
                    _listener.Prefixes.Add(prefix);
            }

            Prefixes.Seal();

            // Create a fresh linked token source for this server lifetime.
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                _listener.Start();
            }
            catch (HttpListenerException ex)
            {
                // Translate the unhelpful Win32 exception into a ServerStartException
                // that identifies the offending prefixes and provides actionable guidance.
                _state = ServerState.Stopped;
                Prefixes.Unseal();
                throw new ServerStartException(Prefixes, ex);
            }

            _state = ServerState.Listening;

            // Launch the request loop as a background task and store it
            // so StopAsync can await it during shutdown.
            _requestLoopTask = RunRequestLoopAsync(_cts.Token);

            FireEvent(AfterStarting);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    /// <inheritdoc/>
    public void Stop()
    {
        StopAsync().GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // No-op if already stopped or stopping.
            if (_state != ServerState.Listening) return;

            _state = ServerState.Stopping;

            FireEvent(BeforeStopping);

            // Cancel the request loop token to unblock GetContextAsync.
            _cts?.Cancel();

            _listener.Stop();

            // Await the request loop with a timeout to allow in-flight
            // requests to complete before giving up.
            if (_requestLoopTask != null)
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(_shutdownTimeout);

                try
                {
                    await _requestLoopTask.WaitAsync(timeoutCts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Shutdown timeout elapsed or cancellation requested.
                    // Proceed with shutdown regardless.
                }
            }

            Prefixes.Unseal();

            _cts?.Dispose();
            _cts = null;
            _requestLoopTask = null;
            _state = ServerState.Stopped;

            FireEvent(AfterStopping);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    /// <inheritdoc/>
    public void Close()
    {
        Stop();
        _listener.Close();
        _channel.Writer.TryComplete();
    }

    /// <summary>
    /// Releases the resources used by this <see cref="HttpServer"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the managed resources used by this <see cref="HttpServer"/>.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release managed resources; <see langword="false"/>
    /// if called from a finalizer.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            Close();
            _lifecycleLock.Dispose();
            _cts?.Dispose();
        }

        _disposed = true;
    }
}

// =============================================================================
// Request loop
// =============================================================================

public partial class HttpServer
{
    /// <summary>
    /// Runs the request acceptance loop, pulling contexts from
    /// <see cref="HttpListener"/> and writing them to the channel until
    /// the server is stopped or the token is cancelled.
    /// </summary>
    /// <param name="cancellationToken">
    /// The token linked to the server lifetime. Cancelled when the server stops.
    /// </param>
    private async Task RunRequestLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var listenerContext = await _listener
                    .GetContextAsync()
                    .WaitAsync(cancellationToken)
                    .ConfigureAwait(false);

                var context = new HttpContext(listenerContext, cancellationToken);

                // Write to the channel and wait if the channel is full,
                // applying backpressure to the request loop naturally.
                await _channel.Writer
                    .WriteAsync(context, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Server is shutting down. Exit the loop cleanly.
                break;
            }
            catch (HttpListenerException)
            {
                // HttpListener was stopped externally or the connection was
                // reset. Exit the loop cleanly.
                break;
            }
            catch (ObjectDisposedException)
            {
                // HttpListener was disposed. Exit the loop cleanly.
                break;
            }
        }

        // Signal to channel consumers that no more contexts will be written.
        _channel.Writer.TryComplete();
    }
}

// =============================================================================
// Events
// =============================================================================

public partial class HttpServer
{
    /// <inheritdoc/>
    public event ServerEventHandler? BeforeStarting;

    /// <inheritdoc/>
    public event ServerEventHandler? AfterStarting;

    /// <inheritdoc/>
    public event ServerEventHandler? BeforeStopping;

    /// <inheritdoc/>
    public event ServerEventHandler? AfterStopping;

    /// <summary>
    /// Invokes all subscribers of the specified event, swallowing any
    /// exceptions thrown by individual subscribers so that a failing
    /// observability hook cannot prevent the server from changing state.
    /// </summary>
    /// <param name="eventHandler">The event to fire.</param>
    private void FireEvent(ServerEventHandler? eventHandler)
    {
        if (eventHandler == null) return;

        foreach (var handler in eventHandler.GetInvocationList())
        {
            try
            {
                ((ServerEventHandler)handler).Invoke(this);
            }
            catch
            {
                // Observability hooks must never prevent lifecycle transitions.
                // Exceptions from individual subscribers are swallowed here.
                // Consider logging these in a future iteration.
            }
        }
    }
}

// =============================================================================
// Internal types
// =============================================================================

/// <summary>
/// Represents the lifecycle state of an <see cref="HttpServer"/>.
/// </summary>
internal enum ServerState
{
    /// <summary>The server is not listening and can be started.</summary>
    Stopped,

    /// <summary>The server is in the process of starting.</summary>
    Starting,

    /// <summary>The server is listening for incoming requests.</summary>
    Listening,

    /// <summary>The server is in the process of stopping.</summary>
    Stopping
}