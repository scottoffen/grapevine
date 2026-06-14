namespace Grapevine.Abstractions;

/// <summary>
/// Represents the cookies to be sent with an HTTP response as a mutable
/// collection of <see cref="Grapevine.Cookie"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// Each entry in the collection corresponds to a single <c>Set-Cookie</c>
/// header in the response. Cookies are stored by name (case-insensitive):
/// adding a cookie whose name already exists replaces the existing entry.
/// </para>
/// <para>
/// This interface is provided primarily to support unit testing of code that
/// writes response cookies, allowing the concrete
/// <see cref="Grapevine.ResponseCookieCollection"/> to be replaced with a
/// test double. Normal consumers should reference the concrete type directly.
/// </para>
/// </remarks>
public interface IResponseCookieCollection : IEnumerable<Grapevine.Cookie>, IReadOnlyCollection<Grapevine.Cookie>
{
    /// <summary>
    /// Gets a value indicating whether the collection has been sealed.
    /// </summary>
    /// <remarks>
    /// Once <see langword="true"/>, all mutation operations throw
    /// <see cref="InvalidOperationException"/>. The collection cannot
    /// be unsealed.
    /// </remarks>
    bool IsSealed { get; }

    /// <summary>
    /// Gets a value indicating whether the collection is read-only.
    /// </summary>
    bool IsReadOnly { get; }

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
    void Add(Grapevine.Cookie cookie);

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
    void AddRange(IEnumerable<Grapevine.Cookie> cookies);

    /// <summary>
    /// Removes all cookies from the collection.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection is sealed.
    /// </exception>
    void Clear();

    /// <summary>
    /// Determines whether the collection contains a cookie with the same name
    /// as <paramref name="cookie"/>.
    /// </summary>
    /// <param name="cookie">The cookie to check for.</param>
    bool Contains(Grapevine.Cookie cookie);

    /// <summary>
    /// Determines whether the collection contains a cookie with the specified name.
    /// </summary>
    /// <param name="name">The cookie name to check for.</param>
    bool Contains(string name);

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
    void CopyTo(Grapevine.Cookie[] array, int arrayIndex);

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
    bool Remove(Grapevine.Cookie item);

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
    bool Remove(string name);

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
    Grapevine.Cookie? this[string name] { get; set; }

    /// <summary>
    /// Returns an enumerable of fully formatted <c>Set-Cookie</c> header
    /// value strings, one per cookie in the collection.
    /// </summary>
    IEnumerable<string> ToHeaderValues();
}