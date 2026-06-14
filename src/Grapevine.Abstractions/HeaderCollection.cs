using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Grapevine.Abstractions;

/// <summary>
/// Provides the abstract base for HTTP header collections in Grapevine, backing
/// header storage with a case-insensitive dictionary and enforcing a seal mechanism
/// that prevents mutation after the collection is locked.
/// </summary>
/// <remarks>
/// <para>
/// Headers are stored as a <see cref="Dictionary{TKey,TValue}"/> mapping header
/// names (case-insensitive) to a list of raw string values, matching the HTTP
/// specification where the same header name may appear multiple times.
/// </para>
/// <para>
/// All string-keyed mutation and query operations are provided on this base class.
/// Concrete subclasses (<see cref="RequestHeaderCollection"/> and
/// <see cref="ResponseHeaderCollection"/>) add enum-keyed convenience
/// members and strongly-typed parsed properties.
/// </para>
/// <para>
/// Once <see cref="Seal"/> is called, all mutation operations throw
/// <see cref="InvalidOperationException"/>. The collection cannot be unsealed.
/// Parsed property caching on concrete subclasses is also triggered by sealing.
/// </para>
/// </remarks>
[DebuggerDisplay("Count = {Count}, IsSealed = {IsSealed}")]
public abstract class HeaderCollection : IEnumerable<KeyValuePair<string, IReadOnlyList<string>>>
{
    /// <summary>
    /// The exception message used when a mutation is attempted on a sealed collection.
    /// </summary>
    internal static readonly string SealedCollectionMessage =
        "The header collection has been sealed and cannot be modified.";

    /// <summary>
    /// The backing store for all headers. Keys are header names; values are lists
    /// of raw string header values supporting multiple occurrences of the same name.
    /// </summary>
    private readonly Dictionary<string, List<string>> _headers =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether the collection has been sealed.
    /// </summary>
    /// <remarks>
    /// Once <see langword="true"/>, all mutation operations will throw
    /// <see cref="InvalidOperationException"/>. Concrete subclasses may use
    /// this property to trigger parsed property caching on first access after sealing.
    /// </remarks>
    public bool IsSealed { get; private set; }

    /// <summary>
    /// Gets the number of distinct header names in the collection.
    /// </summary>
    public int Count => _headers.Count;

    /// <summary>
    /// Seals the collection, preventing any further modifications.
    /// </summary>
    /// <remarks>
    /// This method is called by the server infrastructure at the point of handoff:
    /// on request collections, immediately before the route handler is invoked;
    /// on response collections, immediately before the response body begins writing.
    /// Calling <see cref="Seal"/> more than once has no effect.
    /// </remarks>
    internal void Seal()
    {
        if (IsSealed) return;
        IsSealed = true;
        OnSealed();
    }

    /// <summary>
    /// Called once when the collection transitions to the sealed state.
    /// </summary>
    /// <remarks>
    /// Override in concrete subclasses to populate parsed property caches
    /// at seal time rather than on first access, if desired.
    /// </remarks>
    protected virtual void OnSealed() { }

    /// <summary>
    /// Adds a value to the collection under the specified header name.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value to add.</param>
    /// <exception cref="ThrowIfNull">
    /// Thrown when <paramref name="name"/> or <paramref name="value"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection is sealed.
    /// </exception>
    public void Add(string name, string value)
    {
        ThrowIfSealed();
        ThrowIfNull(name, nameof(name));
        ThrowIfNull(value, nameof(value));

        if (!_headers.TryGetValue(name, out var list))
        {
            list = new List<string>();
            _headers[name] = list;
        }

        list.Add(value);
    }

    /// <summary>
    /// Sets the collection to contain exactly one value for the specified header name,
    /// replacing any existing values.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value to set.</param>
    /// <exception cref="ThrowIfNull">
    /// Thrown when <paramref name="name"/> or <paramref name="value"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection is sealed.
    /// </exception>
    public void Set(string name, string value)
    {
        ThrowIfSealed();
        ThrowIfNull(name, nameof(name));
        ThrowIfNull(value, nameof(value));

        _headers[name] = new List<string>(1) { value };
    }

    /// <summary>
    /// Removes all values for the specified header name from the collection.
    /// </summary>
    /// <param name="name">The header name to remove.</param>
    /// <returns>
    /// <see langword="true"/> if the header was found and removed;
    /// <see langword="false"/> if it was not present.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection is sealed.
    /// </exception>
    public bool Remove(string name)
    {
        ThrowIfSealed();
        return _headers.Remove(name);
    }

    /// <summary>
    /// Removes the collection entirely.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection is sealed.
    /// </exception>
    public void Clear()
    {
        ThrowIfSealed();
        _headers.Clear();
    }

    /// <summary>
    /// Determines whether the collection contains at least one value for the specified
    /// header name.
    /// </summary>
    /// <param name="name">The header name to check.</param>
    /// <returns>
    /// <see langword="true"/> if the header is present; otherwise <see langword="false"/>.
    /// </returns>
    public bool Contains(string name) => _headers.ContainsKey(name);

    /// <summary>
    /// Attempts to retrieve the first raw value for the specified header name.
    /// </summary>
    /// <param name="name">The header name to look up.</param>
    /// <param name="value">
    /// When this method returns, contains the first value associated with the
    /// specified name, or <see langword="null"/> if not found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a value was found; otherwise <see langword="false"/>.
    /// </returns>
    public bool TryGetValue(string name, [NotNullWhen(true)] out string? value)
    {
        if (_headers.TryGetValue(name, out var list) && list.Count > 0)
        {
            value = list[0];
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Attempts to retrieve all raw values for the specified header name.
    /// </summary>
    /// <param name="name">The header name to look up.</param>
    /// <param name="values">
    /// When this method returns, contains a read-only list of all values associated
    /// with the specified name, or <see langword="null"/> if not found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if any values were found; otherwise <see langword="false"/>.
    /// </returns>
    public bool TryGetValues(string name, [NotNullWhen(true)] out IReadOnlyList<string>? values)
    {
        if (_headers.TryGetValue(name, out var list))
        {
            values = list;
            return true;
        }

        values = null;
        return false;
    }

    /// <summary>
    /// Gets the first raw value for the specified header name, or
    /// <see langword="null"/> if the header is not present.
    /// </summary>
    /// <param name="name">The header name to look up.</param>
    public string? this[string name]
    {
        get
        {
            if (_headers.TryGetValue(name, out var list) && list.Count > 0)
                return list[0];
            return null;
        }
    }

    /// <summary>
    /// Returns an enumerator over all header name and value-list pairs in the collection.
    /// </summary>
    public IEnumerator<KeyValuePair<string, IReadOnlyList<string>>> GetEnumerator()
    {
        // Wrap each list as IReadOnlyList to avoid exposing the mutable backing list.
        foreach (var pair in _headers)
            yield return new KeyValuePair<string, IReadOnlyList<string>>(pair.Key, pair.Value);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> if the collection is sealed.
    /// </summary>
    protected void ThrowIfSealed()
    {
        if (IsSealed)
            throw new InvalidOperationException(SealedCollectionMessage);
    }

    /// <summary>
    /// Gets the raw backing list for the specified header name, for use by
    /// concrete subclasses during sealed-state property caching.
    /// </summary>
    /// <param name="name">The header name to retrieve.</param>
    /// <returns>
    /// The list of raw values, or <see langword="null"/> if the header is not present.
    /// </returns>
    protected List<string>? GetRawList(string name)
    {
        _headers.TryGetValue(name, out var list);
        return list;
    }

    /// <summary>
    /// Gets the first raw value string for the specified header name, for use by
    /// concrete subclasses implementing string convenience properties.
    /// </summary>
    /// <param name="name">The header name to retrieve.</param>
    /// <returns>
    /// The first raw value string, or <see langword="null"/> if the header
    /// is not present or has no values.
    /// </returns>
    protected string? GetRaw(string name)
    {
        if (_headers.TryGetValue(name, out var list) && list.Count > 0)
            return list[0];
        return null;
    }

    /// <summary>
    /// Sets a single raw value for the specified header name, for use by concrete
    /// subclasses implementing string convenience property setters.
    /// </summary>
    /// <param name="name">The header name to set.</param>
    /// <param name="value">The value to set.</param>
    protected void SetRaw(string name, string value)
    {
        ThrowIfSealed();
        _headers[name] = new List<string>(1) { value };
    }

    /// <summary>
    /// Throws <see cref="ArgumentNullException"/> if <paramref name="value"/> is
    /// <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// Extracted to a helper to avoid capturing the parameter name as a string
    /// literal at every call site.
    /// </remarks>
    private static void ThrowIfNull(string? value, string paramName)
    {
        if (value is null)
            throw new System.ArgumentNullException(paramName);
    }
}