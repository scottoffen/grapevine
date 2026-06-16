using System.Collections;
using System.Diagnostics;
using Grapevine.Abstractions;

namespace Grapevine;

/// <summary>
/// Represents the cookies to be sent with an HTTP response, serialized as
/// <c>Set-Cookie</c> response headers.
/// </summary>
/// <remarks>
/// <para>
/// Each entry in the collection corresponds to a single <c>Set-Cookie</c>
/// header in the response. Cookies are stored by name (case-insensitive):
/// adding a cookie whose name already exists replaces the existing entry.
/// </para>
/// <para>
/// The collection is mutable until <see cref="Seal"/> is called by the server
/// infrastructure immediately before the response body begins writing. After
/// sealing, all mutation operations throw <see cref="InvalidOperationException"/>.
/// </para>
/// </remarks>
[DebuggerDisplay("Count = {Count}, IsSealed = {IsSealed}")]
public sealed class ResponseCookieCollection : IResponseCookieCollection
{
    /// <summary>
    /// The exception message used when a mutation is attempted on a sealed collection.
    /// </summary>
    internal static readonly string SealedCollectionMessage =
        "The response cookie collection has been sealed and cannot be modified.";

    private readonly Dictionary<string, Cookie> _cookies =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether the collection has been sealed.
    /// </summary>
    /// <remarks>
    /// Once <see langword="true"/>, all mutation operations throw
    /// <see cref="InvalidOperationException"/>. The collection cannot
    /// be unsealed.
    /// </remarks>
    public bool IsSealed { get; private set; }

    /// <summary>
    /// Gets the number of cookies in the collection.
    /// </summary>
    public int Count => _cookies.Count;

    /// <summary>
    /// Gets a value indicating whether the collection is read-only.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="true"/> when the collection is sealed,
    /// satisfying the <see cref="ICollection{T}"/> contract.
    /// </remarks>
    public bool IsReadOnly => IsSealed;

    /// <summary>
    /// Seals the collection, preventing any further modifications.
    /// </summary>
    /// <remarks>
    /// Called by the server infrastructure immediately before the response
    /// body begins writing. Calling <see cref="Seal"/> more than once has
    /// no effect.
    /// </remarks>
    internal void Seal()
    {
        IsSealed = true;
    }

    /// <summary>
    /// Adds a cookie to the collection. If a cookie with the same name already
    /// exists, it is replaced.
    /// </summary>
    /// <param name="cookie">The cookie to add.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="cookie"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection is sealed.
    /// </exception>
    public void Add(Cookie cookie)
    {
        ThrowIfSealed();

        if (cookie is null)
            throw new ArgumentNullException(nameof(cookie));

        _cookies[cookie.Name] = cookie;
    }

    /// <summary>
    /// Adds a range of cookies to the collection. Cookies with duplicate names
    /// replace existing entries.
    /// </summary>
    /// <param name="cookies">The cookies to add.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="cookies"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection is sealed.
    /// </exception>
    public void AddRange(IEnumerable<Cookie> cookies)
    {
        ThrowIfSealed();

        if (cookies is null)
            throw new ArgumentNullException(nameof(cookies));

        foreach (var cookie in cookies)
            Add(cookie);
    }

    /// <summary>
    /// Removes all cookies from the collection.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection is sealed.
    /// </exception>
    public void Clear()
    {
        ThrowIfSealed();
        _cookies.Clear();
    }

    /// <summary>
    /// Determines whether the collection contains a cookie with the same name
    /// as <paramref name="cookie"/>.
    /// </summary>
    /// <param name="cookie">The cookie to check for.</param>
    public bool Contains(Cookie cookie) => _cookies.ContainsKey(cookie.Name);

    /// <summary>
    /// Determines whether the collection contains a cookie with the specified name.
    /// </summary>
    /// <param name="name">The cookie name to check for.</param>
    public bool Contains(string name) => _cookies.ContainsKey(name);

    /// <summary>
    /// Copies the cookies to the specified array, starting at
    /// <paramref name="arrayIndex"/>.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index at which copying begins.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="arrayIndex"/> is negative or greater than
    /// the length of <paramref name="array"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the destination array has insufficient space.
    /// </exception>
    public void CopyTo(Cookie[] array, int arrayIndex)
    {
        if (array is null)
            throw new ArgumentNullException(nameof(array));

        if (arrayIndex < 0 || arrayIndex > array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        if (array.Length - arrayIndex < _cookies.Count)
            throw new ArgumentException("The destination array has insufficient space.", nameof(array));

        var i = arrayIndex;
        foreach (var cookie in _cookies.Values)
            array[i++] = cookie;
    }

    /// <summary>
    /// Removes the cookie with the same name as <paramref name="item"/>
    /// from the collection.
    /// </summary>
    /// <param name="item">The cookie whose name identifies the entry to remove.</param>
    /// <returns>
    /// <see langword="true"/> if a matching cookie was found and removed;
    /// otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection is sealed.
    /// </exception>
    public bool Remove(Cookie item) => Remove(item.Name);

    /// <summary>
    /// Removes the cookie with the specified name from the collection.
    /// </summary>
    /// <param name="name">The name of the cookie to remove.</param>
    /// <returns>
    /// <see langword="true"/> if the cookie was found and removed;
    /// otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection is sealed.
    /// </exception>
    public bool Remove(string name)
    {
        ThrowIfSealed();
        return _cookies.Remove(name);
    }

    /// <summary>
    /// Gets or sets the cookie with the specified name.
    /// </summary>
    /// <param name="name">The cookie name.</param>
    /// <remarks>
    /// The name of the cookie being set must match the indexer key using
    /// ordinal case-insensitive comparison.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown by the setter when <paramref name="value"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown by the setter when the cookie name does not match the indexer key.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown by the setter when the collection is sealed.
    /// </exception>
    public Cookie? this[string name]
    {
        get => _cookies.TryGetValue(name, out var cookie) ? cookie : null;
        set
        {
            ThrowIfSealed();

            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (!string.Equals(value.Name, name, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(
                    "Cookie name must match the indexer key.", nameof(value));

            _cookies[name] = value;
        }
    }

    /// <summary>
    /// Returns an enumerable of fully formatted <c>Set-Cookie</c> header
    /// value strings, one per cookie in the collection.
    /// </summary>
    public IEnumerable<string> ToHeaderValues()
    {
        foreach (var cookie in _cookies.Values)
            yield return cookie.ToString();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the cookies in the collection.
    /// </summary>
    public IEnumerator<Cookie> GetEnumerator() => _cookies.Values.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> if the collection is sealed.
    /// </summary>
    private void ThrowIfSealed()
    {
        if (IsSealed)
            throw new InvalidOperationException(SealedCollectionMessage);
    }
}