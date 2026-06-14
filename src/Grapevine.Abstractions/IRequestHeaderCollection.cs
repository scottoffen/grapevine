using System.Diagnostics.CodeAnalysis;
using Grapevine.Abstractions;

namespace Grapevine;

/// <summary>
/// Represents the HTTP headers associated with an incoming request, providing
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
/// The collection is mutable by middleware during pipeline processing. Once
/// sealed by the server infrastructure immediately before the route handler
/// is invoked, no further modifications are permitted and strongly-typed
/// parsed properties are cached on first access.
/// </para>
/// <para>
/// String convenience properties (e.g. <see cref="Host"/>, <see cref="UserAgent"/>)
/// always return the first raw value for that header, or <see langword="null"/>
/// if the header is not present.
/// </para>
/// </remarks>
public interface IRequestHeaderCollection : IEnumerable<KeyValuePair<string, IReadOnlyList<string>>>
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
    /// Adds a value to the collection under the specified request header name.
    /// </summary>
    /// <param name="header">The request header to add.</param>
    /// <param name="value">The value to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    void Add(RequestHeader header, string value);

    /// <summary>
    /// Sets the collection to contain exactly one value for the specified request
    /// header name, replacing any existing values.
    /// </summary>
    /// <param name="header">The request header to set.</param>
    /// <param name="value">The value to set.</param>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    void Set(RequestHeader header, string value);

    /// <summary>
    /// Removes all values for the specified request header from the collection.
    /// </summary>
    /// <param name="header">The request header to remove.</param>
    /// <returns>
    /// <see langword="true"/> if the header was found and removed;
    /// otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the collection is sealed.</exception>
    bool Remove(RequestHeader header);

    /// <summary>
    /// Determines whether the collection contains at least one value for the
    /// specified request header.
    /// </summary>
    /// <param name="header">The request header to check.</param>
    bool Contains(RequestHeader header);

    /// <summary>
    /// Attempts to retrieve the first raw value for the specified request header.
    /// </summary>
    /// <param name="header">The request header to look up.</param>
    /// <param name="value">
    /// When this method returns, contains the first value, or <see langword="null"/>
    /// if not found.
    /// </param>
    /// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
    bool TryGetValue(RequestHeader header, [NotNullWhen(true)] out string? value);

    /// <summary>
    /// Attempts to retrieve all raw values for the specified request header.
    /// </summary>
    /// <param name="header">The request header to look up.</param>
    /// <param name="values">
    /// When this method returns, contains all values, or <see langword="null"/>
    /// if not found.
    /// </param>
    /// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
    bool TryGetValues(RequestHeader header, [NotNullWhen(true)] out IReadOnlyList<string>? values);

    /// <summary>
    /// Gets the first raw value for the specified request header, or
    /// <see langword="null"/> if the header is not present.
    /// </summary>
    /// <param name="header">The request header to retrieve.</param>
    string? this[RequestHeader header] { get; }

    // -------------------------------------------------------------------------
    // Strongly-typed parsed properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the parsed <c>Accept</c> header, or <see langword="null"/> if not present.
    /// </summary>
    Header? Accept { get; }

    /// <summary>
    /// Gets the parsed <c>Accept-Encoding</c> header, or <see langword="null"/> if not present.
    /// </summary>
    Header? AcceptEncoding { get; }

    /// <summary>
    /// Gets the parsed <c>Accept-Language</c> header, or <see langword="null"/> if not present.
    /// </summary>
    Header? AcceptLanguage { get; }

    /// <summary>
    /// Gets the parsed <c>Content-Type</c> header as a <see cref="ContentType"/> instance,
    /// or <see langword="null"/> if not present or unparseable.
    /// </summary>
    ContentType? ContentType { get; }

    /// <summary>
    /// Gets the <c>Content-Length</c> header value as a <see cref="long"/>,
    /// or <see langword="null"/> if not present or unparseable.
    /// </summary>
    long? ContentLength { get; }

    /// <summary>
    /// Gets the parsed cookies from the <c>Cookie</c> request header.
    /// </summary>
    IRequestCookieCollection Cookies { get; }

    // -------------------------------------------------------------------------
    // String convenience properties
    // -------------------------------------------------------------------------

    /// <summary>Gets the raw <c>Accept-Charset</c> header value, or <see langword="null"/> if not present.</summary>
    string? AcceptCharset { get; }

    /// <summary>Gets the raw <c>Authorization</c> header value, or <see langword="null"/> if not present.</summary>
    string? Authorization { get; }

    /// <summary>Gets the raw <c>Cache-Control</c> header value, or <see langword="null"/> if not present.</summary>
    string? CacheControl { get; }

    /// <summary>Gets the raw <c>Connection</c> header value, or <see langword="null"/> if not present.</summary>
    string? Connection { get; }

    /// <summary>Gets the raw <c>Content-Encoding</c> header value, or <see langword="null"/> if not present.</summary>
    string? ContentEncoding { get; }

    /// <summary>
    /// Gets the raw <c>Cookie</c> header value, or <see langword="null"/> if not present.
    /// </summary>
    /// <remarks>
    /// For structured cookie access, use <see cref="Cookies"/> instead.
    /// </remarks>
    string? Cookie { get; }

    /// <summary>Gets the raw <c>Date</c> header value, or <see langword="null"/> if not present.</summary>
    string? Date { get; }

    /// <summary>Gets the raw <c>Expect</c> header value, or <see langword="null"/> if not present.</summary>
    string? Expect { get; }

    /// <summary>Gets the raw <c>Forwarded</c> header value, or <see langword="null"/> if not present.</summary>
    string? Forwarded { get; }

    /// <summary>Gets the raw <c>From</c> header value, or <see langword="null"/> if not present.</summary>
    string? From { get; }

    /// <summary>Gets the raw <c>Host</c> header value, or <see langword="null"/> if not present.</summary>
    string? Host { get; }

    /// <summary>Gets the raw <c>If-Match</c> header value, or <see langword="null"/> if not present.</summary>
    string? IfMatch { get; }

    /// <summary>Gets the raw <c>If-Modified-Since</c> header value, or <see langword="null"/> if not present.</summary>
    string? IfModifiedSince { get; }

    /// <summary>Gets the raw <c>If-None-Match</c> header value, or <see langword="null"/> if not present.</summary>
    string? IfNoneMatch { get; }

    /// <summary>Gets the raw <c>If-Range</c> header value, or <see langword="null"/> if not present.</summary>
    string? IfRange { get; }

    /// <summary>Gets the raw <c>If-Unmodified-Since</c> header value, or <see langword="null"/> if not present.</summary>
    string? IfUnmodifiedSince { get; }

    /// <summary>Gets the raw <c>Origin</c> header value, or <see langword="null"/> if not present.</summary>
    string? Origin { get; }

    /// <summary>Gets the raw <c>Pragma</c> header value, or <see langword="null"/> if not present.</summary>
    string? Pragma { get; }

    /// <summary>Gets the raw <c>Proxy-Authorization</c> header value, or <see langword="null"/> if not present.</summary>
    string? ProxyAuthorization { get; }

    /// <summary>Gets the raw <c>Range</c> header value, or <see langword="null"/> if not present.</summary>
    string? Range { get; }

    /// <summary>Gets the raw <c>Referer</c> header value, or <see langword="null"/> if not present.</summary>
    string? Referer { get; }

    /// <summary>Gets the raw <c>TE</c> header value, or <see langword="null"/> if not present.</summary>
    string? TE { get; }

    /// <summary>Gets the raw <c>Trailer</c> header value, or <see langword="null"/> if not present.</summary>
    string? Trailer { get; }

    /// <summary>Gets the raw <c>Transfer-Encoding</c> header value, or <see langword="null"/> if not present.</summary>
    string? TransferEncoding { get; }

    /// <summary>Gets the raw <c>Upgrade</c> header value, or <see langword="null"/> if not present.</summary>
    string? Upgrade { get; }

    /// <summary>Gets the raw <c>User-Agent</c> header value, or <see langword="null"/> if not present.</summary>
    string? UserAgent { get; }

    /// <summary>Gets the raw <c>Via</c> header value, or <see langword="null"/> if not present.</summary>
    string? Via { get; }

    /// <summary>Gets the raw <c>Warning</c> header value, or <see langword="null"/> if not present.</summary>
    string? Warning { get; }
}