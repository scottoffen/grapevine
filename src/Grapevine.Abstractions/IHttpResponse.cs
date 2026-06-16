using Grapevine.Abstractions;

namespace Grapevine;

/// <summary>
/// Represents an outgoing HTTP response.
/// </summary>
/// <remarks>
/// This interface is implemented by the Grapevine HTTP server and is available
/// to middleware, route handlers, and other pipeline components via
/// <see cref="IHttpContext.Response"/>. Consumers who need to unit test code that
/// depends on this interface can provide a mock implementation.
/// </remarks>
public interface IHttpResponse
{
    /// <summary>
    /// Gets or sets the HTTP status code returned to the client.
    /// </summary>
    HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the response body.
    /// </summary>
    ContentType ContentType { get; set; }

    /// <summary>
    /// Gets the collection of HTTP headers to be sent to the client.
    /// </summary>
    IResponseHeaderCollection Headers { get; }

    /// <summary>
    /// Gets the collection of cookies to be sent to the client.
    /// </summary>
    IResponseCookieCollection Cookies { get; }

    /// <summary>
    /// Gets the stream to which the response body is written.
    /// </summary>
    /// <remarks>
    /// Write to this stream to send response body content to the client. Use
    /// extension methods in the <c>Grapevine</c> package for common write
    /// operations such as sending a string or a byte array. The stream should
    /// be flushed before calling <see cref="Close"/> or <see cref="CloseAsync"/>.
    /// </remarks>
    Stream OutputStream { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the response uses chunked transfer
    /// encoding.
    /// </summary>
    bool SendChunked { get; set; }

    /// <summary>
    /// Gets or sets the number of bytes in the response body. Set this before
    /// writing to <see cref="OutputStream"/> when the content length is known in
    /// advance and <see cref="SendChunked"/> is <see langword="false"/>.
    /// </summary>
    long ContentLength64 { get; set; }

    /// <summary>
    /// Gets a value indicating whether a response has been sent, or the connection
    /// has been closed or aborted.
    /// </summary>
    /// <remarks>
    /// This property is set to <see langword="true"/> automatically when
    /// <see cref="Abort"/>, <see cref="Close"/>, <see cref="CloseAsync"/>, or
    /// <see cref="Redirect"/> is called. Pipeline components that need to check
    /// or set this value should do so via <see cref="IHttpResponse"/> directly.
    /// Use <see cref="IHttpContext.WasRespondedTo"/> for a read-only view of the
    /// same value at the context level.
    /// </remarks>
    bool WasRespondedTo { get; set; }

    /// <summary>
    /// Closes the connection immediately without sending any response data.
    /// </summary>
    /// <remarks>
    /// Use this method when the connection must be terminated without completing
    /// the HTTP response, for example after detecting a protocol violation. To
    /// complete the response normally, use <see cref="Close"/> or
    /// <see cref="CloseAsync"/> instead.
    /// </remarks>
    void Abort();

    /// <summary>
    /// Flushes and closes the response, sending all buffered output to the client.
    /// </summary>
    /// <remarks>
    /// For I/O-bound scenarios, prefer <see cref="CloseAsync"/> to avoid blocking
    /// the calling thread.
    /// </remarks>
    void Close();

    /// <summary>
    /// Asynchronously flushes and closes the response, sending all buffered output
    /// to the client.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous close operation.</returns>
    Task CloseAsync();

    /// <summary>
    /// Redirects the client to the specified URL by sending a redirect response
    /// and closing the connection.
    /// </summary>
    /// <param name="url">The URL to redirect the client to.</param>
    void Redirect(string url);
}