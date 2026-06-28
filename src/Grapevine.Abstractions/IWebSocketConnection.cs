using System.Net.WebSockets;

namespace Grapevine;

/// <summary>
/// Represents an accepted WebSocket connection, providing send, receive,
/// and close operations over the underlying socket.
/// </summary>
/// <remarks>
/// <para>
/// An instance is obtained by calling
/// <see cref="IHttpContext.AcceptWebSocketAsync"/> on a context whose
/// <see cref="IHttpRequest.IsWebSocketRequest"/> is <see langword="true"/>.
/// Once accepted, the HTTP connection is upgraded and the normal request
/// pipeline no longer applies.
/// </para>
/// <para>
/// Implementations of this interface exist to allow consumers to unit test
/// WebSocket handler code without a real network connection. The concrete
/// implementation in the <c>Grapevine</c> package wraps
/// <see cref="System.Net.WebSockets.WebSocket"/>.
/// </para>
/// </remarks>
public interface IWebSocketConnection : IDisposable
{
    /// <summary>
    /// Gets the current state of the WebSocket connection.
    /// </summary>
    WebSocketState State { get; }

    /// <summary>
    /// Gets the subprotocol that was negotiated during the opening handshake,
    /// or an empty string if no subprotocol was negotiated.
    /// </summary>
    string SubProtocol { get; }

    /// <summary>
    /// Sends data over the WebSocket connection.
    /// </summary>
    /// <param name="buffer">The data to send.</param>
    /// <param name="messageType">
    /// Indicates whether the data is text or binary.
    /// </param>
    /// <param name="endOfMessage">
    /// <see langword="true"/> if this is the final fragment of the message;
    /// <see langword="false"/> if more fragments follow.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the send operation.
    /// </param>
#if NETSTANDARD2_0
    Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
#else
    Task SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
#endif

    /// <summary>
    /// Receives data from the WebSocket connection.
    /// </summary>
    /// <param name="buffer">
    /// The buffer into which the received data is written.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the receive operation.
    /// </param>
    /// <returns>
    /// A <see cref="WebSocketReceiveResult"/> describing the received message
    /// fragment.
    /// </returns>
#if NETSTANDARD2_0
    Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);
#else
    ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken);
#endif

    /// <summary>
    /// Initiates a close handshake, sending a close frame to the remote endpoint.
    /// </summary>
    /// <param name="closeStatus">The reason for closing the connection.</param>
    /// <param name="statusDescription">
    /// A human-readable explanation of the closure reason, or
    /// <see langword="null"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the close operation.
    /// </param>
    Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a close frame to the remote endpoint without waiting for the
    /// acknowledgement close frame.
    /// </summary>
    /// <param name="closeStatus">The reason for closing the connection.</param>
    /// <param name="statusDescription">
    /// A human-readable explanation of the closure reason, or
    /// <see langword="null"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the operation.
    /// </param>
    /// <remarks>
    /// Use this method when you want to initiate a close without blocking on
    /// the full close handshake. The connection is not fully closed until the
    /// remote endpoint sends its close frame in response.
    /// </remarks>
    Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken);
}