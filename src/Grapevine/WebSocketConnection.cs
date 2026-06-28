using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;

namespace Grapevine;

/// <summary>
/// Wraps a <see cref="System.Net.WebSockets.WebSocket"/> and exposes it as an
/// <see cref="IWebSocketConnection"/>.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class WebSocketConnection : IWebSocketConnection
{
    private readonly WebSocket _socket;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="WebSocketConnection"/> wrapping
    /// the specified <see cref="WebSocket"/>.
    /// </summary>
    /// <param name="socket">The underlying <see cref="WebSocket"/> to wrap.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="socket"/> is <see langword="null"/>.
    /// </exception>
    internal WebSocketConnection(WebSocket socket)
    {
        _socket = socket ?? throw new ArgumentNullException(nameof(socket));
    }

    /// <inheritdoc/>
    public WebSocketState State => _socket.State;

    /// <inheritdoc/>
    public string SubProtocol => _socket.SubProtocol ?? string.Empty;

    /// <inheritdoc/>
#if NETSTANDARD2_0
    public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        => _socket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
#else
    public Task SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        => _socket.SendAsync(buffer, messageType, endOfMessage, cancellationToken).AsTask();
#endif

    /// <inheritdoc/>
#if NETSTANDARD2_0
    public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        => _socket.ReceiveAsync(buffer, cancellationToken);
#else
    public ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        => _socket.ReceiveAsync(buffer, cancellationToken);
#endif

    /// <inheritdoc/>
    public Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        => _socket.CloseAsync(closeStatus, statusDescription, cancellationToken);

    /// <inheritdoc/>
    public Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        => _socket.CloseOutputAsync(closeStatus, statusDescription, cancellationToken);

    /// <summary>
    /// Releases the underlying <see cref="WebSocket"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _socket.Dispose();
        _disposed = true;
    }
}