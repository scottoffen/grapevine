using Grapevine.Abstractions;

namespace Grapevine;

/// <summary>
/// Represents an incoming HTTP request.
/// </summary>
/// <remarks>
/// This interface is implemented by the Grapevine HTTP server and is available
/// to middleware, route handlers, and other pipeline components via
/// <see cref="IHttpContext.Request"/>. Consumers who need to unit test code that
/// depends on this interface can provide a mock implementation.
/// </remarks>
public interface IHttpRequest
{
    /// <summary>
    /// Gets the HTTP method used by the client.
    /// </summary>
    HttpMethod Method { get; }

    /// <summary>
    /// Gets the <see cref="Uri"/> requested by the client, or <see langword="null"/>
    /// if the URL could not be parsed.
    /// </summary>
    Uri? Url { get; }

    /// <summary>
    /// Gets the raw URL string exactly as sent by the client, before any parsing
    /// or normalization, or <see langword="null"/> if unavailable.
    /// </summary>
    string? RawUrl { get; }

    /// <summary>
    /// Gets the path component of the request URL, excluding the scheme, host,
    /// and query string.
    /// </summary>
    /// <remarks>
    /// This is the primary value used by the routing engine to match incoming
    /// requests against registered routes. It corresponds to
    /// <see cref="Uri.AbsolutePath"/> on the parsed <see cref="Url"/>.
    /// </remarks>
    string Path { get; }

    /// <summary>
    /// Gets the collection of HTTP headers sent by the client.
    /// </summary>
    IRequestHeaderCollection Headers { get; }

    /// <summary>
    /// Gets the parsed query string parameters from the request URL.
    /// </summary>
    IQueryParams QueryString { get; }

    /// <summary>
    /// Gets the cookies sent by the client.
    /// </summary>
    IRequestCookieCollection Cookies { get; }

    /// <summary>
    /// Gets the body of the request as a readable stream.
    /// </summary>
    /// <remarks>
    /// The stream is forward-only and should be read once. Use extension methods
    /// in the <c>Grapevine</c> package for common read operations such as reading
    /// the body as a string or as a byte array.
    /// </remarks>
    Stream InputStream { get; }

    /// <summary>
    /// Gets a value indicating whether the client was authenticated when making
    /// this request.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets a value indicating whether the request originated from the local
    /// machine.
    /// </summary>
    bool IsLocal { get; }

    /// <summary>
    /// Gets a value indicating whether the request was sent over a secure
    /// (HTTPS/TLS) connection.
    /// </summary>
    bool IsSecureConnection { get; }

    /// <summary>
    /// Gets a value indicating whether the request is a WebSocket upgrade
    /// request.
    /// </summary>
    bool IsWebSocketRequest { get; }

    /// <summary>
    /// Gets a value indicating whether the client requested a persistent
    /// connection.
    /// </summary>
    bool KeepAlive { get; }

    /// <summary>
    /// Gets the local endpoint on the server that received this request.
    /// </summary>
    ConnectionInfo LocalEndpoint { get; }

    /// <summary>
    /// Gets the remote endpoint of the client that sent this request.
    /// </summary>
    ConnectionInfo RemoteEndpoint { get; }
}