using System.Diagnostics.CodeAnalysis;
using Grapevine.Abstractions;

namespace Grapevine.Abstractions;

/// <summary>
/// Default implementation of <see cref="IRequestHeaderCollection"/>.
/// </summary>
/// <remarks>
/// <para>
/// Consumers should type against <see cref="IRequestHeaderCollection"/> rather
/// than this concrete type. This class is public to support alternative server
/// implementations that extend <see cref="HeaderCollection"/>.
/// </para>
/// </remarks>
public sealed class RequestHeaderCollection : HeaderCollection, IRequestHeaderCollection
{
    /// <summary>
    /// Maps each <see cref="RequestHeader"/> enum member (by integer value) to its
    /// canonical wire-format header name string.
    /// </summary>
    /// <remarks>
    /// The order of entries must exactly match the declaration order of
    /// <see cref="RequestHeader"/> members. A startup assertion in the static
    /// constructor validates this invariant. Do not reorder entries without
    /// also reordering the enum.
    /// </remarks>
    internal static readonly string[] HeaderNameMap =
    {
        HeaderNames.Accept,             // Accept
        HeaderNames.AcceptCharset,      // AcceptCharset
        HeaderNames.AcceptEncoding,     // AcceptEncoding
        HeaderNames.AcceptLanguage,     // AcceptLanguage
        HeaderNames.Authorization,      // Authorization
        HeaderNames.CacheControl,       // CacheControl
        HeaderNames.Connection,         // Connection
        HeaderNames.ContentEncoding,    // ContentEncoding
        HeaderNames.ContentLength,      // ContentLength
        HeaderNames.ContentType,        // ContentType
        HeaderNames.Cookie,             // Cookie
        HeaderNames.Date,               // Date
        HeaderNames.Expect,             // Expect
        HeaderNames.Forwarded,          // Forwarded
        HeaderNames.From,               // From
        HeaderNames.Host,               // Host
        HeaderNames.IfMatch,            // IfMatch
        HeaderNames.IfModifiedSince,    // IfModifiedSince
        HeaderNames.IfNoneMatch,        // IfNoneMatch
        HeaderNames.IfRange,            // IfRange
        HeaderNames.IfUnmodifiedSince,  // IfUnmodifiedSince
        HeaderNames.Origin,             // Origin
        HeaderNames.Pragma,             // Pragma
        HeaderNames.ProxyAuthorization, // ProxyAuthorization
        HeaderNames.Range,              // Range
        HeaderNames.Referer,            // Referer
        HeaderNames.TE,                 // TE
        HeaderNames.Trailer,            // Trailer
        HeaderNames.TransferEncoding,   // TransferEncoding
        HeaderNames.Upgrade,            // Upgrade
        HeaderNames.UserAgent,          // UserAgent
        HeaderNames.Via,                // Via
        HeaderNames.Warning,            // Warning
    };

    static RequestHeaderCollection()
    {
        // Validate that the mapping array covers every declared enum member.
        // This catches the case where a new RequestHeader member is added
        // without a corresponding entry in HeaderNameMap.
#if NET6_0_OR_GREATER
        var enumCount = Enum.GetValues<RequestHeader>().Length;
#else
        var enumCount = ((RequestHeader[])Enum.GetValues(typeof(RequestHeader))).Length;
#endif
        if (HeaderNameMap.Length != enumCount)
        {
            throw new InvalidOperationException(
                $"{nameof(RequestHeaderCollection)}.{nameof(HeaderNameMap)} has " +
                $"{HeaderNameMap.Length} entries but {nameof(RequestHeader)} has " +
                $"{enumCount} members. Update {nameof(HeaderNameMap)} to match.");
        }
    }

    // -------------------------------------------------------------------------
    // Parsed property backing fields (populated after Seal())
    // -------------------------------------------------------------------------

    private Header? _accept;
    private Header? _acceptEncoding;
    private Header? _acceptLanguage;
    private ContentType? _contentType;
    private long? _contentLength;
    private bool _contentLengthParsed;
    private IRequestCookieCollection? _cookies;

    /// <inheritdoc/>
    protected override void OnSealed()
    {
        // Intentionally left empty. Parsed properties use lazy initialization
        // on first access after sealing rather than eagerly populating all fields.
        // Override if eager population is needed in a subclass.
    }

    // -------------------------------------------------------------------------
    // Enum-keyed access
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the first raw value for the specified request header, or
    /// <see langword="null"/> if the header is not present.
    /// </summary>
    /// <param name="header">The request header to retrieve.</param>
    public string? this[RequestHeader header] => GetRaw(HeaderNameMap[(int)header]);

    /// <summary>
    /// Adds a value to the collection under the specified request header name.
    /// </summary>
    /// <param name="header">The request header to add.</param>
    /// <param name="value">The value to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    public void Add(RequestHeader header, string value) =>
        Add(HeaderNameMap[(int)header], value);

    /// <summary>
    /// Sets the collection to contain exactly one value for the specified request
    /// header name, replacing any existing values.
    /// </summary>
    /// <param name="header">The request header to set.</param>
    /// <param name="value">The value to set.</param>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    public void Set(RequestHeader header, string value) =>
        Set(HeaderNameMap[(int)header], value);

    /// <summary>
    /// Removes all values for the specified request header from the collection.
    /// </summary>
    /// <param name="header">The request header to remove.</param>
    /// <returns>
    /// <see langword="true"/> if the header was found and removed;
    /// <see langword="false"/> if it was not present.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    public bool Remove(RequestHeader header) =>
        Remove(HeaderNameMap[(int)header]);

    /// <summary>
    /// Determines whether the collection contains at least one value for the
    /// specified request header.
    /// </summary>
    /// <param name="header">The request header to check.</param>
    public bool Contains(RequestHeader header) =>
        Contains(HeaderNameMap[(int)header]);

    /// <summary>
    /// Attempts to retrieve the first raw value for the specified request header.
    /// </summary>
    /// <param name="header">The request header to look up.</param>
    /// <param name="value">
    /// When this method returns, contains the first value, or <see langword="null"/>
    /// if not found.
    /// </param>
    /// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(RequestHeader header, [NotNullWhen(true)] out string? value) =>
        TryGetValue(HeaderNameMap[(int)header], out value);

    /// <summary>
    /// Attempts to retrieve all raw values for the specified request header.
    /// </summary>
    /// <param name="header">The request header to look up.</param>
    /// <param name="values">
    /// When this method returns, contains all values, or <see langword="null"/>
    /// if not found.
    /// </param>
    /// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
    public bool TryGetValues(RequestHeader header, [NotNullWhen(true)] out IReadOnlyList<string>? values) =>
        TryGetValues(HeaderNameMap[(int)header], out values);

    // -------------------------------------------------------------------------
    // Strongly-typed parsed properties (cached after Seal())
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the parsed <c>Accept</c> header, or <see langword="null"/> if not present.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="Header"/> contains <see cref="HeaderValue"/> instances
    /// with quality factors parsed from the raw header value, enabling RFC 7231
    /// content negotiation preference ordering. After the collection is sealed,
    /// this property is cached and returns the same instance on every access.
    /// </remarks>
    public Header? Accept
    {
        get
        {
            if (IsSealed)
                return _accept ??= ParseHeader(HeaderNames.Accept);
            return ParseHeader(HeaderNames.Accept);
        }
    }

    /// <summary>
    /// Gets the parsed <c>Accept-Encoding</c> header, or <see langword="null"/> if not present.
    /// </summary>
    /// <remarks>
    /// After the collection is sealed, this property is cached and returns the
    /// same instance on every access.
    /// </remarks>
    public Header? AcceptEncoding
    {
        get
        {
            if (IsSealed)
                return _acceptEncoding ??= ParseHeader(HeaderNames.AcceptEncoding);
            return ParseHeader(HeaderNames.AcceptEncoding);
        }
    }

    /// <summary>
    /// Gets the parsed <c>Accept-Language</c> header, or <see langword="null"/> if not present.
    /// </summary>
    /// <remarks>
    /// After the collection is sealed, this property is cached and returns the
    /// same instance on every access.
    /// </remarks>
    public Header? AcceptLanguage
    {
        get
        {
            if (IsSealed)
                return _acceptLanguage ??= ParseHeader(HeaderNames.AcceptLanguage);
            return ParseHeader(HeaderNames.AcceptLanguage);
        }
    }

    /// <summary>
    /// Gets the parsed <c>Content-Type</c> header as a <see cref="ContentType"/> instance,
    /// or <see langword="null"/> if not present or if the value cannot be parsed.
    /// </summary>
    /// <remarks>
    /// After the collection is sealed, this property is cached and returns the
    /// same instance on every access.
    /// </remarks>
    public ContentType? ContentType
    {
        get
        {
            if (IsSealed)
                return _contentType ??= ParseContentType();
            return ParseContentType();
        }
    }

    /// <summary>
    /// Gets the <c>Content-Length</c> header value as a <see cref="long"/>,
    /// or <see langword="null"/> if not present or if the value cannot be parsed.
    /// </summary>
    /// <remarks>
    /// After the collection is sealed, this property is cached and returns the
    /// same value on every access.
    /// </remarks>
    public long? ContentLength
    {
        get
        {
            if (IsSealed)
            {
                if (!_contentLengthParsed)
                {
                    _contentLength = ParseContentLength();
                    _contentLengthParsed = true;
                }
                return _contentLength;
            }
            return ParseContentLength();
        }
    }

    /// <summary>
    /// Gets the parsed cookies from the <c>Cookie</c> request header as a
    /// read-only name-value collection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The raw <c>Cookie</c> header value is parsed on first access. If multiple
    /// <c>Cookie</c> headers are present (non-standard but permitted by RFC 6265),
    /// their values are joined with <c>"; "</c> before parsing so that all cookies
    /// are available in the collection.
    /// </para>
    /// <para>
    /// After the collection is sealed, the parsed result is cached and returned
    /// on every subsequent access. Before sealing, the header is re-parsed on
    /// every access to reflect any middleware modifications.
    /// </para>
    /// <para>
    /// Returns <see cref="RequestCookieCollection.Empty"/> when the <c>Cookie</c>
    /// header is absent or contains no valid pairs.
    /// </para>
    /// </remarks>
    public IRequestCookieCollection Cookies
    {
        get
        {
            if (IsSealed)
                return _cookies ??= ParseCookies();
            return ParseCookies();
        }
    }

    // -------------------------------------------------------------------------
    // String convenience properties
    // -------------------------------------------------------------------------

    /// <summary>Gets the raw <c>Accept-Charset</c> header value, or <see langword="null"/> if not present.</summary>
    public string? AcceptCharset => GetRaw(HeaderNames.AcceptCharset);

    /// <summary>Gets the raw <c>Authorization</c> header value, or <see langword="null"/> if not present.</summary>
    public string? Authorization => GetRaw(HeaderNames.Authorization);

    /// <summary>Gets the raw <c>Cache-Control</c> header value, or <see langword="null"/> if not present.</summary>
    public string? CacheControl => GetRaw(HeaderNames.CacheControl);

    /// <summary>Gets the raw <c>Connection</c> header value, or <see langword="null"/> if not present.</summary>
    public string? Connection => GetRaw(HeaderNames.Connection);

    /// <summary>Gets the raw <c>Content-Encoding</c> header value, or <see langword="null"/> if not present.</summary>
    public string? ContentEncoding => GetRaw(HeaderNames.ContentEncoding);

    /// <summary>Gets the raw <c>Cookie</c> header value, or <see langword="null"/> if not present.</summary>
    /// <remarks>
    /// For structured cookie access, use <see cref="Cookies"/> instead.
    /// </remarks>
    public string? Cookie => GetRaw(HeaderNames.Cookie);

    /// <summary>Gets the raw <c>Date</c> header value, or <see langword="null"/> if not present.</summary>
    public string? Date => GetRaw(HeaderNames.Date);

    /// <summary>Gets the raw <c>Expect</c> header value, or <see langword="null"/> if not present.</summary>
    public string? Expect => GetRaw(HeaderNames.Expect);

    /// <summary>Gets the raw <c>Forwarded</c> header value, or <see langword="null"/> if not present.</summary>
    public string? Forwarded => GetRaw(HeaderNames.Forwarded);

    /// <summary>Gets the raw <c>From</c> header value, or <see langword="null"/> if not present.</summary>
    public string? From => GetRaw(HeaderNames.From);

    /// <summary>Gets the raw <c>Host</c> header value, or <see langword="null"/> if not present.</summary>
    public string? Host => GetRaw(HeaderNames.Host);

    /// <summary>Gets the raw <c>If-Match</c> header value, or <see langword="null"/> if not present.</summary>
    public string? IfMatch => GetRaw(HeaderNames.IfMatch);

    /// <summary>Gets the raw <c>If-Modified-Since</c> header value, or <see langword="null"/> if not present.</summary>
    public string? IfModifiedSince => GetRaw(HeaderNames.IfModifiedSince);

    /// <summary>Gets the raw <c>If-None-Match</c> header value, or <see langword="null"/> if not present.</summary>
    public string? IfNoneMatch => GetRaw(HeaderNames.IfNoneMatch);

    /// <summary>Gets the raw <c>If-Range</c> header value, or <see langword="null"/> if not present.</summary>
    public string? IfRange => GetRaw(HeaderNames.IfRange);

    /// <summary>Gets the raw <c>If-Unmodified-Since</c> header value, or <see langword="null"/> if not present.</summary>
    public string? IfUnmodifiedSince => GetRaw(HeaderNames.IfUnmodifiedSince);

    /// <summary>Gets the raw <c>Origin</c> header value, or <see langword="null"/> if not present.</summary>
    public string? Origin => GetRaw(HeaderNames.Origin);

    /// <summary>Gets the raw <c>Pragma</c> header value, or <see langword="null"/> if not present.</summary>
    public string? Pragma => GetRaw(HeaderNames.Pragma);

    /// <summary>Gets the raw <c>Proxy-Authorization</c> header value, or <see langword="null"/> if not present.</summary>
    public string? ProxyAuthorization => GetRaw(HeaderNames.ProxyAuthorization);

    /// <summary>Gets the raw <c>Range</c> header value, or <see langword="null"/> if not present.</summary>
    public string? Range => GetRaw(HeaderNames.Range);

    /// <summary>Gets the raw <c>Referer</c> header value, or <see langword="null"/> if not present.</summary>
    public string? Referer => GetRaw(HeaderNames.Referer);

    /// <summary>Gets the raw <c>TE</c> header value, or <see langword="null"/> if not present.</summary>
    public string? TE => GetRaw(HeaderNames.TE);

    /// <summary>Gets the raw <c>Trailer</c> header value, or <see langword="null"/> if not present.</summary>
    public string? Trailer => GetRaw(HeaderNames.Trailer);

    /// <summary>Gets the raw <c>Transfer-Encoding</c> header value, or <see langword="null"/> if not present.</summary>
    public string? TransferEncoding => GetRaw(HeaderNames.TransferEncoding);

    /// <summary>Gets the raw <c>Upgrade</c> header value, or <see langword="null"/> if not present.</summary>
    public string? Upgrade => GetRaw(HeaderNames.Upgrade);

    /// <summary>Gets the raw <c>User-Agent</c> header value, or <see langword="null"/> if not present.</summary>
    public string? UserAgent => GetRaw(HeaderNames.UserAgent);

    /// <summary>Gets the raw <c>Via</c> header value, or <see langword="null"/> if not present.</summary>
    public string? Via => GetRaw(HeaderNames.Via);

    /// <summary>Gets the raw <c>Warning</c> header value, or <see langword="null"/> if not present.</summary>
    public string? Warning => GetRaw(HeaderNames.Warning);

    // -------------------------------------------------------------------------
    // Private parse helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Parses the raw value for the given header name into a <see cref="Header"/>
    /// instance, or returns <see langword="null"/> if the header is not present.
    /// </summary>
    private Header? ParseHeader(string name)
    {
        var raw = GetRaw(name);
        return raw is null ? null : new Header(name, raw);
    }

    /// <summary>
    /// Parses the raw <c>Content-Type</c> header value into a <see cref="ContentType"/>
    /// instance, or returns <see langword="null"/> if not present or unparseable.
    /// </summary>
    private ContentType? ParseContentType()
    {
        var raw = GetRaw(HeaderNames.ContentType);
        if (raw is null) return null;

        return ContentType.TryParse(raw, out var contentType) ? contentType : null;
    }

    /// <summary>
    /// Parses the raw <c>Content-Length</c> header value into a <see cref="long"/>,
    /// or returns <see langword="null"/> if not present or unparseable.
    /// </summary>
    private long? ParseContentLength()
    {
        var raw = GetRaw(HeaderNames.ContentLength);
        if (raw is null) return null;

        return long.TryParse(raw, out var length) ? length : (long?)null;
    }

    /// <summary>
    /// Parses all <c>Cookie</c> header values into an
    /// <see cref="IRequestCookieCollection"/>.
    /// </summary>
    /// <remarks>
    /// Multiple <c>Cookie</c> header values are joined with <c>"; "</c> before
    /// parsing, per RFC 6265 section 5.4, to ensure all cookies are visible
    /// regardless of how many header lines the client sent.
    /// </remarks>
    private IRequestCookieCollection ParseCookies()
    {
        if (!TryGetValues(HeaderNames.Cookie, out var values))
            return RequestCookieCollection.Empty;

        // Join multiple Cookie header values per RFC 6265 section 5.4.
        var raw = values.Count == 1
            ? values[0]
            : string.Join("; ", values);

        return RequestCookieCollection.Parse(raw);
    }
}