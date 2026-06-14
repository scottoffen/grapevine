using System.Diagnostics.CodeAnalysis;
using Grapevine.Abstractions;

namespace Grapevine.Abstractions;

/// <summary>
/// Default implementation of <see cref="IResponseHeaderCollection"/>.
/// </summary>
/// <remarks>
/// <para>
/// Consumers should type against <see cref="IResponseHeaderCollection"/> rather
/// than this concrete type. This class is public to support alternative server
/// implementations that extend <see cref="HeaderCollection"/>.
/// </para>
/// </remarks>
public sealed class ResponseHeaderCollection : HeaderCollection, IResponseHeaderCollection
{
    /// <summary>
    /// Maps each <see cref="ResponseHeader"/> enum member (by integer value) to its
    /// canonical wire-format header name string.
    /// </summary>
    /// <remarks>
    /// The order of entries must exactly match the declaration order of
    /// <see cref="ResponseHeader"/> members. A startup assertion in the static
    /// constructor validates this invariant. Do not reorder entries without
    /// also reordering the enum.
    /// </remarks>
    internal static readonly string[] HeaderNameMap =
    {
        HeaderNames.AcceptRanges,           // AcceptRanges
        HeaderNames.Age,                    // Age
        HeaderNames.Allow,                  // Allow
        HeaderNames.AltSvc,                 // AltSvc
        HeaderNames.CacheControl,           // CacheControl
        HeaderNames.Connection,             // Connection
        HeaderNames.ContentDisposition,     // ContentDisposition
        HeaderNames.ContentEncoding,        // ContentEncoding
        HeaderNames.ContentLanguage,        // ContentLanguage
        HeaderNames.ContentLength,          // ContentLength
        HeaderNames.ContentLocation,        // ContentLocation
        HeaderNames.ContentRange,           // ContentRange
        HeaderNames.ContentType,            // ContentType
        HeaderNames.Date,                   // Date
        HeaderNames.ETag,                   // ETag
        HeaderNames.Expires,                // Expires
        HeaderNames.HTTP2Settings,          // HTTP2Settings
        HeaderNames.LastModified,           // LastModified
        HeaderNames.Link,                   // Link
        HeaderNames.Location,               // Location
        HeaderNames.Pragma,                 // Pragma
        HeaderNames.ProxyAuthenticate,      // ProxyAuthenticate
        HeaderNames.RetryAfter,             // RetryAfter
        HeaderNames.Server,                 // Server
        HeaderNames.SetCookie,              // SetCookie
        HeaderNames.StrictTransportSecurity,// StrictTransportSecurity
        HeaderNames.TransferEncoding,       // TransferEncoding
        HeaderNames.Upgrade,                // Upgrade
        HeaderNames.Vary,                   // Vary
        HeaderNames.Via,                    // Via
        HeaderNames.WWWAuthenticate,        // WWWAuthenticate
    };

    static ResponseHeaderCollection()
    {
        // Validate that the mapping array covers every declared enum member.
        // This catches the case where a new ResponseHeader member is added
        // without a corresponding entry in HeaderNameMap.
        var enumCount = Enum.GetValues(typeof(ResponseHeader)).Length;
        if (HeaderNameMap.Length != enumCount)
        {
            throw new InvalidOperationException(
                $"{nameof(ResponseHeaderCollection)}.{nameof(HeaderNameMap)} has " +
                $"{HeaderNameMap.Length} entries but {nameof(ResponseHeader)} has " +
                $"{enumCount} members. Update {nameof(HeaderNameMap)} to match.");
        }
    }

    // -------------------------------------------------------------------------
    // Parsed property backing fields (populated after Seal())
    // -------------------------------------------------------------------------

    private ContentType? _contentType;
    private long? _contentLength;
    private bool _contentLengthParsed;

    /// <summary>
    /// Gets the structured cookie collection for this response.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The collection is owned by this instance and is created on construction.
    /// Consumers add cookies here; the server infrastructure reads
    /// <see cref="ResponseCookieCollection.ToHeaderValues"/> to produce the
    /// corresponding <c>Set-Cookie</c> response headers.
    /// </para>
    /// <para>
    /// Calling <see cref="HeaderCollection.Seal"/> on this collection also seals
    /// <see cref="Cookies"/>, preventing any further cookie modifications after
    /// the response begins sending.
    /// </para>
    /// </remarks>
    public IResponseCookieCollection Cookies { get; } = new ResponseCookieCollection();

    /// <inheritdoc/>
    protected override void OnSealed()
    {
        // Cascade the seal to the cookie collection so that cookies and headers
        // are locked together in a single atomic operation. The cast is safe
        // because the field is always initialised to a ResponseCookieCollection
        // instance; Seal() is intentionally not on IResponseCookieCollection.
        ((ResponseCookieCollection)Cookies).Seal();
    }

    // -------------------------------------------------------------------------
    // Enum-keyed access
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the first raw value for the specified response header, or
    /// <see langword="null"/> if the header is not present.
    /// </summary>
    /// <param name="header">The response header to retrieve.</param>
    public string? this[ResponseHeader header] => GetRaw(HeaderNameMap[(int)header]);

    /// <summary>
    /// Adds a value to the collection under the specified response header name.
    /// </summary>
    /// <param name="header">The response header to add.</param>
    /// <param name="value">The value to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    public void Add(ResponseHeader header, string value) =>
        Add(HeaderNameMap[(int)header], value);

    /// <summary>
    /// Sets the collection to contain exactly one value for the specified response
    /// header name, replacing any existing values.
    /// </summary>
    /// <param name="header">The response header to set.</param>
    /// <param name="value">The value to set.</param>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    public void Set(ResponseHeader header, string value) =>
        Set(HeaderNameMap[(int)header], value);

    /// <summary>
    /// Removes all values for the specified response header from the collection.
    /// </summary>
    /// <param name="header">The response header to remove.</param>
    /// <returns>
    /// <see langword="true"/> if the header was found and removed;
    /// <see langword="false"/> if it was not present.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    public bool Remove(ResponseHeader header) =>
        Remove(HeaderNameMap[(int)header]);

    /// <summary>
    /// Determines whether the collection contains at least one value for the
    /// specified response header.
    /// </summary>
    /// <param name="header">The response header to check.</param>
    public bool Contains(ResponseHeader header) =>
        Contains(HeaderNameMap[(int)header]);

    /// <summary>
    /// Attempts to retrieve the first raw value for the specified response header.
    /// </summary>
    /// <param name="header">The response header to look up.</param>
    /// <param name="value">
    /// When this method returns, contains the first value, or <see langword="null"/>
    /// if not found.
    /// </param>
    /// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(ResponseHeader header, [NotNullWhen(true)] out string? value) =>
        TryGetValue(HeaderNameMap[(int)header], out value);

    /// <summary>
    /// Attempts to retrieve all raw values for the specified response header.
    /// </summary>
    /// <param name="header">The response header to look up.</param>
    /// <param name="values">
    /// When this method returns, contains all values, or <see langword="null"/>
    /// if not found.
    /// </param>
    /// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
    public bool TryGetValues(ResponseHeader header, [NotNullWhen(true)] out IReadOnlyList<string>? values) =>
        TryGetValues(HeaderNameMap[(int)header], out values);

    // -------------------------------------------------------------------------
    // Strongly-typed parsed properties (cached after Seal(), settable before)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the parsed <c>Content-Type</c> header as a <see cref="ContentType"/> instance,
    /// or <see langword="null"/> if not present or if the value cannot be parsed.
    /// </summary>
    /// <remarks>
    /// After the collection is sealed, this property is cached and returns the
    /// same instance on every access. The setter stores the <see cref="ContentType"/>
    /// using its <see cref="ContentType.ToString"/> representation.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public ContentType? ContentType
    {
        get
        {
            if (IsSealed)
                return _contentType ??= ParseContentType();
            return ParseContentType();
        }
        set
        {
            if (value is null)
                Remove(HeaderNames.ContentType);
            else
                SetRaw(HeaderNames.ContentType, value.ToString());
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
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
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
        set
        {
            if (value is null)
                Remove(HeaderNames.ContentLength);
            else
                SetRaw(HeaderNames.ContentLength, value.Value.ToString());
        }
    }

    // -------------------------------------------------------------------------
    // String convenience properties
    // -------------------------------------------------------------------------

    /// <summary>Gets or sets the raw <c>Accept-Ranges</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? AcceptRanges
    {
        get => GetRaw(HeaderNames.AcceptRanges);
        set => SetOrRemove(HeaderNames.AcceptRanges, value);
    }

    /// <summary>Gets or sets the raw <c>Age</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? Age
    {
        get => GetRaw(HeaderNames.Age);
        set => SetOrRemove(HeaderNames.Age, value);
    }

    /// <summary>Gets or sets the raw <c>Allow</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? Allow
    {
        get => GetRaw(HeaderNames.Allow);
        set => SetOrRemove(HeaderNames.Allow, value);
    }

    /// <summary>Gets or sets the raw <c>Alt-Svc</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? AltSvc
    {
        get => GetRaw(HeaderNames.AltSvc);
        set => SetOrRemove(HeaderNames.AltSvc, value);
    }

    /// <summary>Gets or sets the raw <c>Cache-Control</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? CacheControl
    {
        get => GetRaw(HeaderNames.CacheControl);
        set => SetOrRemove(HeaderNames.CacheControl, value);
    }

    /// <summary>Gets or sets the raw <c>Connection</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? Connection
    {
        get => GetRaw(HeaderNames.Connection);
        set => SetOrRemove(HeaderNames.Connection, value);
    }

    /// <summary>Gets or sets the raw <c>Content-Disposition</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? ContentDisposition
    {
        get => GetRaw(HeaderNames.ContentDisposition);
        set => SetOrRemove(HeaderNames.ContentDisposition, value);
    }

    /// <summary>Gets or sets the raw <c>Content-Encoding</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? ContentEncoding
    {
        get => GetRaw(HeaderNames.ContentEncoding);
        set => SetOrRemove(HeaderNames.ContentEncoding, value);
    }

    /// <summary>Gets or sets the raw <c>Content-Language</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? ContentLanguage
    {
        get => GetRaw(HeaderNames.ContentLanguage);
        set => SetOrRemove(HeaderNames.ContentLanguage, value);
    }

    /// <summary>Gets or sets the raw <c>Content-Location</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? ContentLocation
    {
        get => GetRaw(HeaderNames.ContentLocation);
        set => SetOrRemove(HeaderNames.ContentLocation, value);
    }

    /// <summary>Gets or sets the raw <c>Content-Range</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? ContentRange
    {
        get => GetRaw(HeaderNames.ContentRange);
        set => SetOrRemove(HeaderNames.ContentRange, value);
    }

    /// <summary>Gets or sets the raw <c>Date</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? Date
    {
        get => GetRaw(HeaderNames.Date);
        set => SetOrRemove(HeaderNames.Date, value);
    }

    /// <summary>Gets or sets the raw <c>ETag</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? ETag
    {
        get => GetRaw(HeaderNames.ETag);
        set => SetOrRemove(HeaderNames.ETag, value);
    }

    /// <summary>Gets or sets the raw <c>Expires</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? Expires
    {
        get => GetRaw(HeaderNames.Expires);
        set => SetOrRemove(HeaderNames.Expires, value);
    }

    /// <summary>Gets or sets the raw <c>HTTP2-Settings</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? HTTP2Settings
    {
        get => GetRaw(HeaderNames.HTTP2Settings);
        set => SetOrRemove(HeaderNames.HTTP2Settings, value);
    }

    /// <summary>Gets or sets the raw <c>Last-Modified</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? LastModified
    {
        get => GetRaw(HeaderNames.LastModified);
        set => SetOrRemove(HeaderNames.LastModified, value);
    }

    /// <summary>Gets or sets the raw <c>Link</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? Link
    {
        get => GetRaw(HeaderNames.Link);
        set => SetOrRemove(HeaderNames.Link, value);
    }

    /// <summary>Gets or sets the raw <c>Location</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? Location
    {
        get => GetRaw(HeaderNames.Location);
        set => SetOrRemove(HeaderNames.Location, value);
    }

    /// <summary>Gets or sets the raw <c>Pragma</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? Pragma
    {
        get => GetRaw(HeaderNames.Pragma);
        set => SetOrRemove(HeaderNames.Pragma, value);
    }

    /// <summary>Gets or sets the raw <c>Proxy-Authenticate</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? ProxyAuthenticate
    {
        get => GetRaw(HeaderNames.ProxyAuthenticate);
        set => SetOrRemove(HeaderNames.ProxyAuthenticate, value);
    }

    /// <summary>Gets or sets the raw <c>Retry-After</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? RetryAfter
    {
        get => GetRaw(HeaderNames.RetryAfter);
        set => SetOrRemove(HeaderNames.RetryAfter, value);
    }

    /// <summary>Gets or sets the raw <c>Server</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? Server
    {
        get => GetRaw(HeaderNames.Server);
        set => SetOrRemove(HeaderNames.Server, value);
    }

    /// <summary>Gets or sets the raw <c>Set-Cookie</c> header value.</summary>
    /// <remarks>
    /// For structured cookie access, use <see cref="Cookies"/> instead.
    /// This property provides raw string access for cases where the full
    /// <c>Set-Cookie</c> header value is already formatted.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? SetCookie
    {
        get => GetRaw(HeaderNames.SetCookie);
        set => SetOrRemove(HeaderNames.SetCookie, value);
    }

    /// <summary>Gets or sets the raw <c>Strict-Transport-Security</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? StrictTransportSecurity
    {
        get => GetRaw(HeaderNames.StrictTransportSecurity);
        set => SetOrRemove(HeaderNames.StrictTransportSecurity, value);
    }

    /// <summary>Gets or sets the raw <c>Transfer-Encoding</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? TransferEncoding
    {
        get => GetRaw(HeaderNames.TransferEncoding);
        set => SetOrRemove(HeaderNames.TransferEncoding, value);
    }

    /// <summary>Gets or sets the raw <c>Upgrade</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? Upgrade
    {
        get => GetRaw(HeaderNames.Upgrade);
        set => SetOrRemove(HeaderNames.Upgrade, value);
    }

    /// <summary>Gets or sets the raw <c>Vary</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? Vary
    {
        get => GetRaw(HeaderNames.Vary);
        set => SetOrRemove(HeaderNames.Vary, value);
    }

    /// <summary>Gets or sets the raw <c>Via</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? Via
    {
        get => GetRaw(HeaderNames.Via);
        set => SetOrRemove(HeaderNames.Via, value);
    }

    /// <summary>Gets or sets the raw <c>WWW-Authenticate</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    public string? WWWAuthenticate
    {
        get => GetRaw(HeaderNames.WWWAuthenticate);
        set => SetOrRemove(HeaderNames.WWWAuthenticate, value);
    }

    // -------------------------------------------------------------------------
    // Private parse helpers
    // -------------------------------------------------------------------------

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
    /// Sets the specified header to <paramref name="value"/>, or removes it
    /// if <paramref name="value"/> is <see langword="null"/>.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The value to set, or <see langword="null"/> to remove.</param>
    private void SetOrRemove(string name, string? value)
    {
        if (value is null)
            Remove(name);
        else
            SetRaw(name, value);
    }
}