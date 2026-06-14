using System.Diagnostics.CodeAnalysis;

namespace Grapevine.Abstractions;

/// <summary>
/// Provides <see langword="static readonly"/> string constants for standard HTTP
/// header names, used as the canonical wire-format names throughout Grapevine.
/// </summary>
/// <remarks>
/// <para>
/// These fields serve as the single source of truth for header name strings.
/// They are used internally by <see cref="RequestHeaderCollection"/>
/// and <see cref="ResponseHeaderCollection"/> to populate their
/// enum-to-name mapping arrays, and are available to consumers for use in
/// string-keyed header operations without risk of typos.
/// </para>
/// <para>
/// Fields are <see langword="static readonly"/> rather than <see langword="const"/>
/// to avoid the binary-compatibility issues that arise when <see langword="const"/>
/// fields are inlined at compile time by referencing assemblies.
/// </para>
/// </remarks>
[ExcludeFromCodeCoverage]
public static class HeaderNames
{
    // -------------------------------------------------------------------------
    // Headers that appear in requests
    // -------------------------------------------------------------------------

    /// <summary>Media types acceptable for the response.</summary>
    public static readonly string Accept = "Accept";

    /// <summary>Character sets acceptable for the response.</summary>
    public static readonly string AcceptCharset = "Accept-Charset";

    /// <summary>Content-encoding algorithms acceptable for the response.</summary>
    public static readonly string AcceptEncoding = "Accept-Encoding";

    /// <summary>Natural languages acceptable for the response.</summary>
    public static readonly string AcceptLanguage = "Accept-Language";

    /// <summary>Authentication credentials for the request.</summary>
    public static readonly string Authorization = "Authorization";

    /// <summary>The URL of the resource from which the request was initiated.</summary>
    public static readonly string Referer = "Referer";

    /// <summary>Authentication credentials for a proxy server.</summary>
    public static readonly string ProxyAuthorization = "Proxy-Authorization";

    /// <summary>Indicates that the client expects specific server behaviors before sending the body.</summary>
    public static readonly string Expect = "Expect";

    /// <summary>Discloses the original client IP address and any intermediate proxies in the chain.</summary>
    public static readonly string Forwarded = "Forwarded";

    /// <summary>The email address of the user making the request.</summary>
    public static readonly string From = "From";

    /// <summary>Performs the request only if the target resource matches the given ETag.</summary>
    public static readonly string IfMatch = "If-Match";

    /// <summary>Performs the request only if the resource has been modified since the given date.</summary>
    public static readonly string IfModifiedSince = "If-Modified-Since";

    /// <summary>Performs the request only if the target resource does not match the given ETag.</summary>
    public static readonly string IfNoneMatch = "If-None-Match";

    /// <summary>Requests a byte range of the resource, but only if it has not changed since the given date or ETag.</summary>
    public static readonly string IfRange = "If-Range";

    /// <summary>Performs the request only if the resource has not been modified since the given date.</summary>
    public static readonly string IfUnmodifiedSince = "If-Unmodified-Since";

    /// <summary>Indicates the origin of a cross-site request.</summary>
    public static readonly string Origin = "Origin";

    /// <summary>Requests only a specific byte range of the target resource.</summary>
    public static readonly string Range = "Range";

    /// <summary>Indicates the transfer encodings the client is willing to accept in the response.</summary>
    public static readonly string TE = "TE";

    /// <summary>Identifies the client software originating the request.</summary>
    public static readonly string UserAgent = "User-Agent";

    /// <summary>Cookies previously sent by the server and stored by the client.</summary>
    public static readonly string Cookie = "Cookie";

    // -------------------------------------------------------------------------
    // Headers that appear in responses
    // -------------------------------------------------------------------------

    /// <summary>Specifies the range units supported by the server for partial content requests.</summary>
    public static readonly string AcceptRanges = "Accept-Ranges";

    /// <summary>Indicates the age of the cached response in seconds.</summary>
    public static readonly string Age = "Age";

    /// <summary>Lists the HTTP methods permitted for the target resource.</summary>
    public static readonly string Allow = "Allow";

    /// <summary>Advertises alternative services available for the resource.</summary>
    public static readonly string AltSvc = "Alt-Svc";

    /// <summary>Indicates how the client should handle the response body (e.g. inline or attachment).</summary>
    public static readonly string ContentDisposition = "Content-Disposition";

    /// <summary>The natural language(s) of the intended audience for the response body.</summary>
    public static readonly string ContentLanguage = "Content-Language";

    /// <summary>An alternate URL at which the response content can be accessed.</summary>
    public static readonly string ContentLocation = "Content-Location";

    /// <summary>The byte range being returned for a partial content response.</summary>
    public static readonly string ContentRange = "Content-Range";

    /// <summary>A version identifier for the target resource, used for conditional requests and caching.</summary>
    public static readonly string ETag = "ETag";

    /// <summary>The date and time after which the response is considered stale.</summary>
    public static readonly string Expires = "Expires";

    /// <summary>HTTP/2 connection settings sent during an HTTP/1.1 upgrade request.</summary>
    public static readonly string HTTP2Settings = "HTTP2-Settings";

    /// <summary>The date and time at which the target resource was last modified.</summary>
    public static readonly string LastModified = "Last-Modified";

    /// <summary>Describes relationships between this response and other resources.</summary>
    public static readonly string Link = "Link";

    /// <summary>The URL to redirect the client to. Used in 3xx redirection responses.</summary>
    public static readonly string Location = "Location";

    /// <summary>The authentication scheme required by a proxy server.</summary>
    public static readonly string ProxyAuthenticate = "Proxy-Authenticate";

    /// <summary>Indicates how long the client should wait before retrying after a rate limit or service unavailability.</summary>
    public static readonly string RetryAfter = "Retry-After";

    /// <summary>Identifies the server software that handled the request.</summary>
    public static readonly string Server = "Server";

    /// <summary>Sends a cookie from the server to the client for storage.</summary>
    public static readonly string SetCookie = "Set-Cookie";

    /// <summary>Instructs the client to access the site only over HTTPS for a specified duration.</summary>
    public static readonly string StrictTransportSecurity = "Strict-Transport-Security";

    /// <summary>Lists the request headers that influenced the response, for cache differentiation.</summary>
    public static readonly string Vary = "Vary";

    /// <summary>Specifies the authentication scheme required to access the target resource.</summary>
    public static readonly string WWWAuthenticate = "WWW-Authenticate";

    // -------------------------------------------------------------------------
    // Headers that appear in both requests and responses
    // -------------------------------------------------------------------------

    /// <summary>Directives for caching mechanisms.</summary>
    public static readonly string CacheControl = "Cache-Control";

    /// <summary>Controls whether the network connection should remain open.</summary>
    public static readonly string Connection = "Connection";

    /// <summary>The encoding transformation applied to the message body.</summary>
    public static readonly string ContentEncoding = "Content-Encoding";

    /// <summary>The size of the message body in bytes.</summary>
    public static readonly string ContentLength = "Content-Length";

    /// <summary>The media type of the message body.</summary>
    public static readonly string ContentType = "Content-Type";

    /// <summary>The date and time at which the message was originated.</summary>
    public static readonly string Date = "Date";

    /// <summary>Implementation-specific directives for backward compatibility with HTTP/1.0 caches.</summary>
    public static readonly string Pragma = "Pragma";

    /// <summary>Lists headers that will be sent as trailers in a chunked transfer encoding.</summary>
    public static readonly string Trailer = "Trailer";

    /// <summary>The transfer encoding applied to the message body.</summary>
    public static readonly string TransferEncoding = "Transfer-Encoding";

    /// <summary>Requests a protocol switch.</summary>
    public static readonly string Upgrade = "Upgrade";

    /// <summary>Records intermediate proxies or gateways that handled the message.</summary>
    public static readonly string Via = "Via";

    /// <summary>Carries additional status or transformation information.</summary>
    public static readonly string Warning = "Warning";

    /// <summary>The domain name or IP address of the server being requested.</summary>
    public static readonly string Host = "Host";
}