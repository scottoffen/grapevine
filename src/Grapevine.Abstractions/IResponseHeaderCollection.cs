using System.Diagnostics.CodeAnalysis;
using Grapevine.Abstractions;

namespace Grapevine;

/// <summary>
/// Represents the HTTP headers associated with an outgoing response, providing
/// enum-keyed access, string convenience properties for all standard headers,
/// and strongly-typed parsed properties for headers where the parsed form
/// adds meaningful value.
/// </summary>
/// <remarks>
/// <para>
/// Headers are stored in a case-insensitive dictionary. Enum-keyed members map
/// directly to header name strings with no runtime reflection.
/// </para>
/// <para>
/// The collection is mutable until sealed by the server infrastructure
/// immediately before the response body begins writing. After sealing, no
/// further modifications are permitted and strongly-typed parsed properties
/// are cached on first access. Sealing also seals the <see cref="Cookies"/>
/// collection.
/// </para>
/// <para>
/// String convenience properties have both a getter and a setter. The setter
/// will throw <see cref="InvalidOperationException"/> if the collection is sealed.
/// </para>
/// </remarks>
public interface IResponseHeaderCollection : IEnumerable<KeyValuePair<string, IReadOnlyList<string>>>
{
    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether the collection has been sealed.
    /// </summary>
    bool IsSealed { get; }

    /// <summary>
    /// Gets the number of distinct header names in the collection.
    /// </summary>
    int Count { get; }

    // -------------------------------------------------------------------------
    // String-keyed operations
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds a value to the collection under the specified header name.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value to add.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="name"/> or <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    void Add(string name, string value);

    /// <summary>
    /// Adds multiple values to the collection under the specified header name.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="values">The header values to add.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="name"/> or <paramref name="values"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    void Add(string name, string[] values);

    /// <summary>
    /// Sets the collection to contain exactly one value for the specified header name,
    /// replacing any existing values.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value to set.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="name"/> or <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    void Set(string name, string value);

    /// <summary>
    /// Removes all values for the specified header name from the collection.
    /// </summary>
    /// <param name="name">The header name to remove.</param>
    /// <returns>
    /// <see langword="true"/> if the header was found and removed;
    /// otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    bool Remove(string name);

    /// <summary>
    /// Removes all headers from the collection.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    void Clear();

    /// <summary>
    /// Determines whether the collection contains at least one value for the
    /// specified header name.
    /// </summary>
    /// <param name="name">The header name to check.</param>
    bool Contains(string name);

    /// <summary>
    /// Attempts to retrieve the first raw value for the specified header name.
    /// </summary>
    /// <param name="name">The header name to look up.</param>
    /// <param name="value">
    /// When this method returns, contains the first value, or <see langword="null"/>
    /// if not found.
    /// </param>
    /// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
    bool TryGetValue(string name, [NotNullWhen(true)] out string? value);

    /// <summary>
    /// Attempts to retrieve all raw values for the specified header name.
    /// </summary>
    /// <param name="name">The header name to look up.</param>
    /// <param name="values">
    /// When this method returns, contains all values, or <see langword="null"/>
    /// if not found.
    /// </param>
    /// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
    bool TryGetValues(string name, [NotNullWhen(true)] out IReadOnlyList<string>? values);

    /// <summary>
    /// Gets the first raw value for the specified header name, or
    /// <see langword="null"/> if the header is not present.
    /// </summary>
    /// <param name="name">The header name to look up.</param>
    string? this[string name] { get; }

    // -------------------------------------------------------------------------
    // Enum-keyed operations
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds a value to the collection under the specified response header name.
    /// </summary>
    /// <param name="header">The response header to add.</param>
    /// <param name="value">The value to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    void Add(ResponseHeader header, string value);

    /// <summary>
    /// Sets the collection to contain exactly one value for the specified response
    /// header name, replacing any existing values.
    /// </summary>
    /// <param name="header">The response header to set.</param>
    /// <param name="value">The value to set.</param>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    void Set(ResponseHeader header, string value);

    /// <summary>
    /// Removes all values for the specified response header from the collection.
    /// </summary>
    /// <param name="header">The response header to remove.</param>
    /// <returns>
    /// <see langword="true"/> if the header was found and removed;
    /// otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    bool Remove(ResponseHeader header);

    /// <summary>
    /// Determines whether the collection contains at least one value for the
    /// specified response header.
    /// </summary>
    /// <param name="header">The response header to check.</param>
    bool Contains(ResponseHeader header);

    /// <summary>
    /// Attempts to retrieve the first raw value for the specified response header.
    /// </summary>
    /// <param name="header">The response header to look up.</param>
    /// <param name="value">
    /// When this method returns, contains the first value, or <see langword="null"/>
    /// if not found.
    /// </param>
    /// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
    bool TryGetValue(ResponseHeader header, [NotNullWhen(true)] out string? value);

    /// <summary>
    /// Attempts to retrieve all raw values for the specified response header.
    /// </summary>
    /// <param name="header">The response header to look up.</param>
    /// <param name="values">
    /// When this method returns, contains all values, or <see langword="null"/>
    /// if not found.
    /// </param>
    /// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
    bool TryGetValues(ResponseHeader header, [NotNullWhen(true)] out IReadOnlyList<string>? values);

    /// <summary>
    /// Gets the first raw value for the specified response header, or
    /// <see langword="null"/> if the header is not present.
    /// </summary>
    /// <param name="header">The response header to retrieve.</param>
    string? this[ResponseHeader header] { get; }

    // -------------------------------------------------------------------------
    // Strongly-typed parsed properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets or sets the parsed <c>Content-Type</c> header as a <see cref="ContentType"/> instance,
    /// or <see langword="null"/> if not present or unparseable.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    ContentType? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the <c>Content-Length</c> header value as a <see cref="long"/>,
    /// or <see langword="null"/> if not present or unparseable.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    long? ContentLength { get; set; }

    /// <summary>
    /// Gets the structured cookie collection for this response.
    /// </summary>
    IResponseCookieCollection Cookies { get; }

    // -------------------------------------------------------------------------
    // String convenience properties
    // -------------------------------------------------------------------------

    /// <summary>Gets or sets the raw <c>Accept-Ranges</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? AcceptRanges { get; set; }

    /// <summary>Gets or sets the raw <c>Age</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? Age { get; set; }

    /// <summary>Gets or sets the raw <c>Allow</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? Allow { get; set; }

    /// <summary>Gets or sets the raw <c>Alt-Svc</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? AltSvc { get; set; }

    /// <summary>Gets or sets the raw <c>Cache-Control</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? CacheControl { get; set; }

    /// <summary>Gets or sets the raw <c>Connection</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? Connection { get; set; }

    /// <summary>Gets or sets the raw <c>Content-Disposition</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? ContentDisposition { get; set; }

    /// <summary>Gets or sets the raw <c>Content-Encoding</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? ContentEncoding { get; set; }

    /// <summary>Gets or sets the raw <c>Content-Language</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? ContentLanguage { get; set; }

    /// <summary>Gets or sets the raw <c>Content-Location</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? ContentLocation { get; set; }

    /// <summary>Gets or sets the raw <c>Content-Range</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? ContentRange { get; set; }

    /// <summary>Gets or sets the raw <c>Date</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? Date { get; set; }

    /// <summary>Gets or sets the raw <c>ETag</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? ETag { get; set; }

    /// <summary>Gets or sets the raw <c>Expires</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? Expires { get; set; }

    /// <summary>Gets or sets the raw <c>HTTP2-Settings</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? HTTP2Settings { get; set; }

    /// <summary>Gets or sets the raw <c>Last-Modified</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? LastModified { get; set; }

    /// <summary>Gets or sets the raw <c>Link</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? Link { get; set; }

    /// <summary>Gets or sets the raw <c>Location</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? Location { get; set; }

    /// <summary>Gets or sets the raw <c>Pragma</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? Pragma { get; set; }

    /// <summary>Gets or sets the raw <c>Proxy-Authenticate</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? ProxyAuthenticate { get; set; }

    /// <summary>Gets or sets the raw <c>Retry-After</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? RetryAfter { get; set; }

    /// <summary>Gets or sets the raw <c>Server</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? Server { get; set; }

    /// <summary>
    /// Gets or sets the raw <c>Set-Cookie</c> header value.
    /// </summary>
    /// <remarks>
    /// For structured cookie access, use <see cref="Cookies"/> instead.
    /// This property provides raw string access for cases where the full
    /// <c>Set-Cookie</c> header value is already formatted.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? SetCookie { get; set; }

    /// <summary>Gets or sets the raw <c>Strict-Transport-Security</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? StrictTransportSecurity { get; set; }

    /// <summary>Gets or sets the raw <c>Transfer-Encoding</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? TransferEncoding { get; set; }

    /// <summary>Gets or sets the raw <c>Upgrade</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? Upgrade { get; set; }

    /// <summary>Gets or sets the raw <c>Vary</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? Vary { get; set; }

    /// <summary>Gets or sets the raw <c>Via</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? Via { get; set; }

    /// <summary>Gets or sets the raw <c>WWW-Authenticate</c> header value.</summary>
    /// <exception cref="InvalidOperationException">Thrown by the setter when the collection is sealed.</exception>
    string? WWWAuthenticate { get; set; }
}