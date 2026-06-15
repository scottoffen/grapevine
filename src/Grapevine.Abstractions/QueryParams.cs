using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Grapevine.Abstractions;

/// <summary>
/// Default implementation of <see cref="Grapevine.IQueryParams"/>, representing
/// the query string parameters of an HTTP request as a read-only, case-insensitive
/// collection of name-value pairs.
/// </summary>
/// <remarks>
/// <para>
/// Consumers should type against <see cref="Grapevine.IQueryParams"/> rather than
/// this concrete type. This class is public to support alternative server
/// implementations and testing.
/// </para>
/// <para>
/// Instances are constructed via <see cref="Parse"/> and are always sealed:
/// no mutation is possible after construction. The original raw query string
/// is preserved and returned by <see cref="ToString"/>.
/// </para>
/// <para>
/// Parameter names are matched case-insensitively. Multiple values for the
/// same key are supported (e.g. <c>?tag=a&amp;tag=b</c>). When the same key
/// appears with different casing, all values are collected under the
/// first-seen casing of the key.
/// </para>
/// </remarks>
[DebuggerDisplay("{_raw}")]
public sealed class QueryParams : Grapevine.IQueryParams
{
    /// <summary>
    /// An empty <see cref="QueryParams"/> instance, used when the request URL
    /// contains no query string.
    /// </summary>
    public static readonly QueryParams Empty = new(
        string.Empty,
        new Dictionary<string, List<string>>(0, StringComparer.OrdinalIgnoreCase));

    private readonly string _raw;
    private readonly Dictionary<string, List<string>> _params;

    private QueryParams(string raw, Dictionary<string, List<string>> @params)
    {
        _raw = raw;
        _params = @params;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Always returns <see langword="true"/>. Query parameter collections are
    /// immutable after construction via <see cref="Parse"/>.
    /// </remarks>
    public bool IsSealed => true;

    /// <inheritdoc/>
    public int Count => _params.Count;

    /// <inheritdoc/>
    public string? this[string key] =>
        _params.TryGetValue(key, out var list) && list.Count > 0
            ? list[0]
            : null;

    /// <inheritdoc/>
    public bool ContainsKey(string key) => _params.ContainsKey(key);

    /// <inheritdoc/>
    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        if (_params.TryGetValue(key, out var list) && list.Count > 0)
        {
            value = list[0];
            return true;
        }

        value = null;
        return false;
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetValues(string key) =>
        _params.TryGetValue(key, out var list)
            ? list
            : Array.Empty<string>();

    /// <inheritdoc/>
    public bool TryGetValues(string key, [NotNullWhen(true)] out IReadOnlyList<string>? values)
    {
        if (_params.TryGetValue(key, out var list))
        {
            values = list;
            return true;
        }

        values = null;
        return false;
    }

    /// <summary>
    /// Returns the original raw query string as received from the URL,
    /// without a leading <c>?</c> character.
    /// </summary>
    public override string ToString() => _raw;

    /// <summary>
    /// Returns an enumerator over all parameter name and value-list pairs.
    /// </summary>
    public IEnumerator<KeyValuePair<string, IReadOnlyList<string>>> GetEnumerator()
    {
        foreach (var pair in _params)
            yield return new KeyValuePair<string, IReadOnlyList<string>>(pair.Key, pair.Value);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Parses a raw query string into a <see cref="QueryParams"/> instance.
    /// </summary>
    /// <param name="queryString">
    /// The raw query string to parse, with or without a leading <c>?</c>
    /// character. May be <see langword="null"/> or empty, in which case
    /// <see cref="Empty"/> is returned.
    /// </param>
    /// <returns>
    /// A populated <see cref="QueryParams"/> instance, or <see cref="Empty"/>
    /// if <paramref name="queryString"/> is null, empty, or contains no valid
    /// pairs.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Parameter names and values are percent-decoded via
    /// <see cref="Uri.UnescapeDataString(string)"/>. Names are matched
    /// case-insensitively. Multiple values for the same name are collected
    /// in encounter order. Parameters without a value (no <c>=</c> sign) are
    /// stored with a <see langword="null"/> value. Parameters with an empty
    /// name after trimming are skipped.
    /// </para>
    /// </remarks>
    public static QueryParams Parse(string? queryString)
    {
        if (string.IsNullOrEmpty(queryString))
            return Empty;

        // Strip the leading '?' if present and store the raw string without it.
        var raw = queryString![0] == '?' ? queryString.Substring(1) : queryString;

        if (raw.Length == 0)
            return Empty;

        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        var length = raw.Length;
        var i = 0;

        while (i < length)
        {
            var keyStart = i;
            var keyEnd = -1;
            var valueStart = -1;

            while (i < length)
            {
                var c = raw[i];
                if (c == '=' && keyEnd < 0)
                {
                    keyEnd = i;
                    valueStart = ++i;
                }
                else if (c == '&')
                {
                    break;
                }
                else
                {
                    i++;
                }
            }

            var key = Uri.UnescapeDataString(
                raw.Substring(keyStart, (keyEnd >= 0 ? keyEnd : i) - keyStart));

            var value = keyEnd >= 0
                ? Uri.UnescapeDataString(raw.Substring(valueStart, i - valueStart))
                : null;

            if (!string.IsNullOrWhiteSpace(key))
            {
                if (!result.TryGetValue(key, out var list))
                {
                    list = new List<string>();
                    result[key] = list;
                }

                if (value is not null)
                    list.Add(value);
            }

            if (i < length && raw[i] == '&') i++;
        }

        return result.Count == 0 ? Empty : new QueryParams(raw, result);
    }
}