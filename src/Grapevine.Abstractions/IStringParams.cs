using System.Diagnostics.CodeAnalysis;

namespace Grapevine;

/// <summary>
/// Represents a read-only collection of string key-value pairs, providing
/// the shared access surface for <see cref="IQueryParams"/> and
/// <see cref="IRouteParams"/>.
/// </summary>
/// <remarks>
/// <para>
/// Key lookup is case-insensitive. The indexer and
/// <see cref="TryGetValue(string, out string?)"/> return only the first value
/// for a given key. Use <see cref="GetValues"/> or <see cref="TryGetValues"/>
/// to retrieve all values when multiple values per key are possible.
/// </para>
/// <para>
/// Strongly-typed value retrieval is available via the
/// <see cref="StringParamsExtensions"/> extension methods, which delegate to
/// the <see cref="Grapevine.Abstractions.ConverterRegistry"/>.
/// </para>
/// </remarks>
public interface IStringParams : IEnumerable<KeyValuePair<string, IReadOnlyList<string>>>
{
    /// <summary>
    /// Gets a value indicating whether the collection has been sealed.
    /// </summary>
    bool IsSealed { get; }

    /// <summary>
    /// Gets the number of distinct keys in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the first value associated with the specified key, or
    /// <see langword="null"/> if the key is not present.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    string? this[string key] { get; }

    /// <summary>
    /// Determines whether the collection contains at least one value for the
    /// specified key.
    /// </summary>
    /// <param name="key">The key to check.</param>
    bool ContainsKey(string key);

    /// <summary>
    /// Attempts to retrieve the first value for the specified key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="value">
    /// When this method returns, contains the first value associated with
    /// <paramref name="key"/>, or <see langword="null"/> if not found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the key was found; otherwise
    /// <see langword="false"/>.
    /// </returns>
    bool TryGetValue(string key, [NotNullWhen(true)] out string? value);

    /// <summary>
    /// Returns all values for the specified key, or an empty list if the key
    /// is not present.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    IReadOnlyList<string> GetValues(string key);

    /// <summary>
    /// Attempts to retrieve all values for the specified key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="values">
    /// When this method returns, contains all values associated with
    /// <paramref name="key"/>, or <see langword="null"/> if not found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the key was found; otherwise
    /// <see langword="false"/>.
    /// </returns>
    bool TryGetValues(string key, [NotNullWhen(true)] out IReadOnlyList<string>? values);
}