namespace Grapevine;

/// <summary>
/// Identifies a standard HTTP request header by name.
/// </summary>
/// <remarks>
/// <para>
/// These values are used as a strongly-typed, discoverable alternative to raw
/// string header names when working with <see cref="IRequestHeaderCollection"/>.
/// Each member maps to a corresponding <see langword="static readonly"/> field
/// on <see cref="Grapevine.Abstractions.HeaderNames"/>.
/// </para>
/// <para>
/// The integer values of this enum are used as indices into a compile-time
/// mapping array on <see cref="Grapevine.Abstractions.RequestHeaderCollection"/>.
/// Do not reorder, remove, or assign explicit integer values to members without
/// also updating that mapping array.
/// </para>
/// </remarks>
public enum RequestHeader
{
    /// <summary>
    /// Media types acceptable for the response.
    /// </summary>
    Accept,

    /// <summary>
    /// Character sets acceptable for the response.
    /// </summary>
    [Obsolete("The Accept-Charset header is deprecated. Modern browsers ignore it.")]
    AcceptCharset,

    /// <summary>
    /// Content-encoding algorithms acceptable for the response.
    /// </summary>
    AcceptEncoding,

    /// <summary>
    /// Natural languages acceptable for the response.
    /// </summary>
    AcceptLanguage,

    /// <summary>
    /// Authentication credentials for the request.
    /// </summary>
    Authorization,

    /// <summary>
    /// Directives for caching mechanisms in the request chain.
    /// </summary>
    CacheControl,

    /// <summary>
    /// Controls whether the network connection should remain open after the request.
    /// </summary>
    Connection,

    /// <summary>
    /// The encoding transformation applied to the request body.
    /// </summary>
    ContentEncoding,

    /// <summary>
    /// The size of the request body in bytes.
    /// </summary>
    ContentLength,

    /// <summary>
    /// The media type of the request body.
    /// </summary>
    ContentType,

    /// <summary>
    /// Cookies previously sent by the server and stored by the client.
    /// </summary>
    Cookie,

    /// <summary>
    /// The date and time at which the request was originated.
    /// </summary>
    [Obsolete("The Date header is rarely used in requests and may be ignored by servers.")]
    Date,

    /// <summary>
    /// Indicates that the client expects specific server behaviors before sending the body.
    /// </summary>
    Expect,

    /// <summary>
    /// Discloses the original client IP address and any intermediate proxies in the chain.
    /// </summary>
    Forwarded,

    /// <summary>
    /// The email address of the user making the request.
    /// </summary>
    [Obsolete("The From header is deprecated for privacy reasons.")]
    From,

    /// <summary>
    /// The domain name or IP address of the server being requested.
    /// </summary>
    Host,

    /// <summary>
    /// Performs the request only if the target resource matches the given ETag.
    /// </summary>
    IfMatch,

    /// <summary>
    /// Performs the request only if the resource has been modified since the given date.
    /// </summary>
    IfModifiedSince,

    /// <summary>
    /// Performs the request only if the target resource does not match the given ETag.
    /// </summary>
    IfNoneMatch,

    /// <summary>
    /// Requests a byte range of the resource, but only if it has not changed since the given date or ETag.
    /// </summary>
    IfRange,

    /// <summary>
    /// Performs the request only if the resource has not been modified since the given date.
    /// </summary>
    IfUnmodifiedSince,

    /// <summary>
    /// Indicates the origin of a cross-site request.
    /// </summary>
    Origin,

    /// <summary>
    /// Implementation-specific directives for backward compatibility with HTTP/1.0 caches.
    /// </summary>
    [Obsolete("The Pragma header is obsolete. Use Cache-Control instead.")]
    Pragma,

    /// <summary>
    /// Authentication credentials for a proxy server in the request chain.
    /// </summary>
    ProxyAuthorization,

    /// <summary>
    /// Requests only a specific byte range of the target resource.
    /// </summary>
    Range,

    /// <summary>
    /// The URL of the resource from which the request was initiated.
    /// </summary>
    Referer,

    /// <summary>
    /// Indicates the transfer encodings the client is willing to accept in the response.
    /// </summary>
    TE,

    /// <summary>
    /// Lists headers that will be sent as trailers in a chunked transfer encoding.
    /// </summary>
    Trailer,

    /// <summary>
    /// The transfer encodings applied to the request body.
    /// </summary>
    TransferEncoding,

    /// <summary>
    /// Requests that the server switch to a different protocol.
    /// </summary>
    Upgrade,

    /// <summary>
    /// Identifies the client software originating the request.
    /// </summary>
    UserAgent,

    /// <summary>
    /// Records intermediate proxies or gateways that forwarded the request.
    /// </summary>
    Via,

    /// <summary>
    /// Carries additional status or transformation information about the request.
    /// </summary>
    [Obsolete("The Warning header is deprecated in HTTP/2 and later.")]
    Warning
}