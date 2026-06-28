using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Claims;

namespace Grapevine;

/// <summary>
/// Wraps an <see cref="HttpListenerContext"/> and exposes it as an
/// <see cref="IHttpContext"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class HttpContext : IHttpContext
{
    private readonly HttpListenerContext _context;
    private readonly CancellationTokenSource _cts;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="HttpContext"/> wrapping the
    /// specified <see cref="HttpListenerContext"/>.
    /// </summary>
    /// <param name="context">The underlying <see cref="HttpListenerContext"/> to wrap.</param>
    /// <param name="serverToken">
    /// The server-level <see cref="CancellationToken"/>. When this token is
    /// cancelled, <see cref="RequestAborted"/> is also cancelled automatically,
    /// allowing in-flight handlers to terminate cleanly during server shutdown.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public HttpContext(HttpListenerContext context, CancellationToken serverToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        _context = context;

        // Link the context's own source to the server token so that either a
        // server shutdown or an explicit call to Abort() will cancel RequestAborted.
        _cts = CancellationTokenSource.CreateLinkedTokenSource(serverToken);

        Id = Guid.NewGuid().ToString();
        Request = new HttpRequest(context.Request);
        Response = new HttpResponse(context.Response);
        User = context.User as ClaimsPrincipal;
        RequestAborted = _cts.Token;
    }

    /// <inheritdoc/>
    public string Id { get; init; }

    /// <inheritdoc/>
    public IHttpRequest Request { get; init; }

    /// <inheritdoc/>
    public IHttpResponse Response { get; init; }

    /// <inheritdoc/>
    public ClaimsPrincipal? User { get; set; }

    /// <inheritdoc/>
    public Locals Locals { get; init; } = new();

    /// <inheritdoc/>
    public CancellationToken RequestAborted { get; init; }

    /// <inheritdoc/>
    public bool WasRespondedTo => Response.WasRespondedTo;

    /// <inheritdoc/>
    public void Abort()
    {
        // Cancel the linked source to signal to all in-flight async operations
        // holding RequestAborted that the client is no longer connected.
        _cts.Cancel();
    }

    /// <inheritdoc/>
    public async Task<IWebSocketConnection> AcceptWebSocketAsync(
        string? subProtocol = null,
        CancellationToken cancellationToken = default)
    {
        var listenerWsContext = await _context
            .AcceptWebSocketAsync(subProtocol)
            .ConfigureAwait(false);

        return new WebSocketConnection(listenerWsContext.WebSocket);
    }

    /// <summary>
    /// Releases the resources used by this <see cref="HttpContext"/>, including
    /// the linked <see cref="CancellationTokenSource"/> and the underlying
    /// <see cref="Request"/> and <see cref="Response"/> instances.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the managed resources used by this <see cref="HttpContext"/>.
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
            // Only dispose the CancellationTokenSource. HttpListener owns the
            // request and response objects and manages their lifetime internally.
            // Disposing the underlying streams causes HttpListener to malfunction
            // on subsequent requests.
            _cts.Dispose();
        }

        _disposed = true;
    }
}