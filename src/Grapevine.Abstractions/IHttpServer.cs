using Grapevine.Abstractions;

namespace Grapevine;

/// <summary>
/// Represents a delegate that handles server lifecycle events.
/// </summary>
/// <param name="server">The server that raised the event.</param>
public delegate void ServerEventHandler(IHttpServer server);

/// <summary>
/// Represents an embedded HTTP server that listens on one or more URI prefixes
/// and processes incoming requests through a middleware pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Consumers configure the server by adding prefixes to <see cref="Prefixes"/>
/// before calling <see cref="Start"/> or <see cref="StartAsync"/>. The server
/// seals <see cref="Prefixes"/> when it starts and unseals them when it stops,
/// so prefixes can only be modified while the server is not listening.
/// </para>
/// <para>
/// Incoming requests are dropped onto an internal channel for processing by
/// the middleware pipeline. The lifecycle events on this interface are
/// observability hooks intended for logging, diagnostics, and UI integrations.
/// They are not middleware extension points and should not be used to
/// participate in request processing.
/// </para>
/// </remarks>
public interface IHttpServer : IDisposable
{
    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the collection of URI prefixes the server listens on.
    /// </summary>
    /// <remarks>
    /// Prefixes must be added before the server is started. The collection is
    /// sealed when the server starts and unsealed when the server stops.
    /// </remarks>
    PrefixCollection Prefixes { get; }

    /// <summary>
    /// Gets a value indicating whether the server is currently listening for
    /// incoming requests.
    /// </summary>
    bool IsListening { get; }

    /// <summary>
    /// Gets the queue from which the middleware pipeline reads incoming
    /// <see cref="IHttpContext"/> instances.
    /// </summary>
    /// <remarks>
    /// The server writes to this queue as requests arrive. The pipeline
    /// reads from it and processes each context in turn. The queue is
    /// completed when the server stops.
    /// </remarks>
    IContextQueue Queue { get; }

    // -------------------------------------------------------------------------
    // Lifecycle methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Starts the server synchronously, beginning to listen for incoming
    /// requests on all configured <see cref="Prefixes"/>.
    /// </summary>
    /// <remarks>
    /// For I/O-bound startup scenarios, prefer <see cref="StartAsync"/> to
    /// avoid blocking the calling thread.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the server is already listening.
    /// </exception>
    void Start();

    /// <summary>
    /// Starts the server asynchronously, beginning to listen for incoming
    /// requests on all configured <see cref="Prefixes"/>.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the start operation.
    /// </param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous start operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the server is already listening.
    /// </exception>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the server synchronously, ceasing to listen for incoming requests.
    /// In-flight requests are allowed to complete before the server stops.
    /// </summary>
    /// <remarks>
    /// For I/O-bound shutdown scenarios, prefer <see cref="StopAsync"/> to
    /// avoid blocking the calling thread.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the server is not listening.
    /// </exception>
    void Stop();

    /// <summary>
    /// Stops the server asynchronously, ceasing to listen for incoming requests.
    /// In-flight requests are allowed to complete before the server stops.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the stop operation.
    /// </param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous stop operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the server is not listening.
    /// </exception>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the server and releases all resources, including the underlying
    /// HTTP listener. Unlike <see cref="Stop"/>, the server cannot be restarted
    /// after <see cref="Close"/> is called.
    /// </summary>
    void Close();

    // -------------------------------------------------------------------------
    // Lifecycle events
    // -------------------------------------------------------------------------

    /// <summary>
    /// Raised immediately before the server begins listening for requests.
    /// </summary>
    /// <remarks>
    /// This event is an observability hook intended for logging, diagnostics,
    /// and UI integrations. It is not a middleware extension point and should
    /// not be used to participate in request processing.
    /// </remarks>
    event ServerEventHandler BeforeStarting;

    /// <summary>
    /// Raised immediately after the server has started listening for requests.
    /// </summary>
    /// <remarks>
    /// This event is an observability hook intended for logging, diagnostics,
    /// and UI integrations. It is not a middleware extension point and should
    /// not be used to participate in request processing.
    /// </remarks>
    event ServerEventHandler AfterStarting;

    /// <summary>
    /// Raised immediately before the server stops listening for requests.
    /// </summary>
    /// <remarks>
    /// This event is an observability hook intended for logging, diagnostics,
    /// and UI integrations. It is not a middleware extension point and should
    /// not be used to participate in request processing.
    /// </remarks>
    event ServerEventHandler BeforeStopping;

    /// <summary>
    /// Raised immediately after the server has stopped listening for requests.
    /// </summary>
    /// <remarks>
    /// This event is an observability hook intended for logging, diagnostics,
    /// and UI integrations. It is not a middleware extension point and should
    /// not be used to participate in request processing.
    /// </remarks>
    event ServerEventHandler AfterStopping;
}