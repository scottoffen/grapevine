namespace Grapevine;

/// <summary>
/// Identifies a standard HTTP response header by name.
/// </summary>
/// <remarks>
/// <para>
/// These values are used as a strongly-typed, discoverable alternative to raw
/// string header names when working with <see cref="IResponseHeaderCollection"/>.
/// Each member maps to a corresponding <see langword="static readonly"/> field
/// on <see cref="Grapevine.Abstractions.HeaderNames"/>.
/// </para>
/// <para>
/// The integer values of this enum are used as indices into a compile-time
/// mapping array on <see cref="Grapevine.Abstractions.ResponseHeaderCollection"/>.
/// Do not reorder, remove, or assign explicit integer values to members without
/// also updating that mapping array.
/// </para>
/// </remarks>
public enum ResponseHeader
{
    /// <summary>
    /// Specifies the range units supported by the server for partial content requests.
    /// </summary>
    AcceptRanges,

    /// <summary>
    /// Indicates the age of the cached response in seconds.
    /// </summary>
    Age,

    /// <summary>
    /// Lists the HTTP methods permitted for the target resource.
    /// </summary>
    Allow,

    /// <summary>
    /// Advertises alternative services available for the resource.
    /// </summary>
    AltSvc,

    /// <summary>
    /// Directives for caching mechanisms in the response chain.
    /// </summary>
    CacheControl,

    /// <summary>
    /// Controls whether the network connection should remain open after the response.
    /// </summary>
    Connection,

    /// <summary>
    /// Indicates how the client should handle the response body (e.g. inline or attachment).
    /// </summary>
    ContentDisposition,

    /// <summary>
    /// The encoding transformation applied to the response body (e.g. gzip).
    /// </summary>
    ContentEncoding,

    /// <summary>
    /// The natural language(s) of the intended audience for the response body.
    /// </summary>
    ContentLanguage,

    /// <summary>
    /// The size of the response body in bytes.
    /// </summary>
    ContentLength,

    /// <summary>
    /// An alternate URL at which the response content can be accessed.
    /// </summary>
    ContentLocation,

    /// <summary>
    /// The byte range being returned for a partial content response.
    /// </summary>
    ContentRange,

    /// <summary>
    /// The media type of the response body (e.g. <c>text/html</c>, <c>application/json</c>).
    /// </summary>
    ContentType,

    /// <summary>
    /// The date and time at which the response was generated.
    /// </summary>
    Date,

    /// <summary>
    /// A version identifier for the target resource, used for conditional requests and caching.
    /// </summary>
    ETag,

    /// <summary>
    /// The date and time after which the response is considered stale.
    /// </summary>
    Expires,

    /// <summary>
    /// HTTP/2 connection settings sent during an HTTP/1.1 upgrade request.
    /// </summary>
    HTTP2Settings,

    /// <summary>
    /// The date and time at which the target resource was last modified.
    /// </summary>
    LastModified,

    /// <summary>
    /// Describes relationships between this response and other resources.
    /// </summary>
    Link,

    /// <summary>
    /// The URL to redirect the client to. Used in 3xx redirection responses.
    /// </summary>
    Location,

    /// <summary>
    /// Implementation-specific directives for backward compatibility with HTTP/1.0 caches.
    /// </summary>
    [Obsolete("The Pragma header is deprecated in HTTP/1.1. Use Cache-Control instead.")]
    Pragma,

    /// <summary>
    /// The authentication scheme required by a proxy server.
    /// </summary>
    ProxyAuthenticate,

    /// <summary>
    /// Indicates how long the client should wait before retrying after a rate limit or service unavailability.
    /// </summary>
    RetryAfter,

    /// <summary>
    /// Identifies the server software that handled the request.
    /// </summary>
    Server,

    /// <summary>
    /// Sends a cookie from the server to the client for storage.
    /// </summary>
    SetCookie,

    /// <summary>
    /// Instructs the client to access the site only over HTTPS for a specified duration.
    /// </summary>
    StrictTransportSecurity,

    /// <summary>
    /// The transfer encoding applied to the response body (e.g. chunked).
    /// </summary>
    TransferEncoding,

    /// <summary>
    /// Requests the client to switch to a different protocol.
    /// </summary>
    Upgrade,

    /// <summary>
    /// Lists the request headers that influenced the response, for cache differentiation.
    /// </summary>
    Vary,

    /// <summary>
    /// Records intermediate proxies or gateways that handled the request.
    /// </summary>
    Via,

    /// <summary>
    /// Specifies the authentication scheme required to access the target resource.
    /// </summary>
    WWWAuthenticate
}