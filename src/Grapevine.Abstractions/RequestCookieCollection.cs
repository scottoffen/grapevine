using System.Collections;
using System.Diagnostics;
using Grapevine.Abstractions;

namespace Grapevine;

/// <summary>
/// Represents the cookies received with an HTTP request, populated from the
/// raw <c>Cookie</c> request header value.
/// </summary>
/// <remarks>
/// <para>
/// Request cookies contain only name-value string pairs. Cookie attributes
/// (path, domain, expiry, flags) are response-only concepts and are not
/// present in incoming <c>Cookie</c> headers.
/// </para>
/// <para>
/// Instances are constructed via <see cref="Parse"/> and are always
/// sealed: no mutation is possible after construction. Cookie name lookup
/// is case-insensitive per RFC 6265.
/// </para>
/// <para>
/// When the same cookie name appears more than once in the raw header,
/// the last value wins.
/// </para>
/// </remarks>
[DebuggerDisplay("Count = {Count}")]
public sealed class RequestCookieCollection : IRequestCookieCollection
{
    /// <summary>
    /// An empty <see cref="RequestCookieCollection"/> instance, used when
    /// the request contains no <c>Cookie</c> header.
    /// </summary>
    public static readonly RequestCookieCollection Empty = new(
        new Dictionary<string, string>(0, StringComparer.OrdinalIgnoreCase));

    private readonly Dictionary<string, string> _cookies;

    private RequestCookieCollection(Dictionary<string, string> cookies)
    {
        _cookies = cookies;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Always returns <see langword="true"/>. Request cookie collections are
    /// populated once from the raw <c>Cookie</c> header and are never mutated.
    /// </remarks>
    public bool IsSealed => true;

    /// <summary>
    /// Parses a raw <c>Cookie</c> request header value into a
    /// <see cref="RequestCookieCollection"/>.
    /// </summary>
    /// <param name="cookieHeader">
    /// The raw value of the <c>Cookie</c> header, e.g.
    /// <c>session=abc123; theme=dark</c>. May be <see langword="null"/> or
    /// empty, in which case <see cref="Empty"/> is returned.
    /// </param>
    /// <returns>
    /// A populated <see cref="RequestCookieCollection"/>, or
    /// <see cref="Empty"/> if <paramref name="cookieHeader"/> is null,
    /// empty, or contains no valid pairs.
    /// </returns>
    /// <remarks>
    /// Cookie pairs are separated by semicolons. Each pair is split on the
    /// first <c>=</c> character. Pairs with no <c>=</c> character are skipped.
    /// Names and values are trimmed of leading and trailing whitespace. When
    /// the same name appears more than once, the last value wins.
    /// </remarks>
    public static RequestCookieCollection Parse(string? cookieHeader)
    {
        if (string.IsNullOrWhiteSpace(cookieHeader))
            return Empty;

        var cookies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var segments = cookieHeader!.Split(';');
        foreach (var segment in segments)
        {
            var trimmed = segment.Trim();
            if (trimmed.Length == 0) continue;

            // Split only on the first '=' to allow '=' in cookie values.
            var equalsIndex = trimmed.IndexOf('=');
            if (equalsIndex < 0) continue;

            var name = trimmed.Substring(0, equalsIndex).Trim();
            var value = trimmed.Substring(equalsIndex + 1).Trim();

            if (name.Length == 0) continue;

            // Last value wins when the same name appears multiple times.
            cookies[name] = value;
        }

        return cookies.Count == 0 ? Empty : new RequestCookieCollection(cookies);
    }

    // -------------------------------------------------------------------------
    // IReadOnlyDictionary<string, string> implementation
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public string this[string key] => _cookies[key];

    /// <inheritdoc/>
    public IEnumerable<string> Keys => _cookies.Keys;

    /// <inheritdoc/>
    public IEnumerable<string> Values => _cookies.Values;

    /// <inheritdoc/>
    public int Count => _cookies.Count;

    /// <inheritdoc/>
    public bool ContainsKey(string key) => _cookies.ContainsKey(key);

    /// <inheritdoc/>
    public bool TryGetValue(string key, out string value)
    {
        if (_cookies.TryGetValue(key, out var found))
        {
            value = found;
            return true;
        }

        value = string.Empty;
        return false;
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() =>
        _cookies.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}