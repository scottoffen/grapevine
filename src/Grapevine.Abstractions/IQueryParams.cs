using System.Diagnostics.CodeAnalysis;

namespace Grapevine;

/// <summary>
/// Represents the query string parameters of an HTTP request as a read-only
/// collection of name-value pairs.
/// </summary>
/// <remarks>
/// <para>
/// Query parameter names are matched case-insensitively. Multiple values for
/// the same key are supported (e.g. <c>?tag=a&amp;tag=b</c>).
/// </para>
/// <para>
/// The indexer and <see cref="TryGetValue(string, out string?)"/> return only
/// the first value for a given key. Use <see cref="GetValues"/> or
/// <see cref="TryGetValues"/> to retrieve all values.
/// </para>
/// <para>
/// Strongly-typed value retrieval is available via the
/// <see cref="QueryParamsExtensions"/> extension methods, which delegate to
/// the <see cref="Grapevine.Abstractions.ConverterRegistry"/>.
/// </para>
/// </remarks>
public interface IQueryParams : IEnumerable<KeyValuePair<string, IReadOnlyList<string>>>
{
    /// <summary>
    /// Gets a value indicating whether the collection has been sealed.
    /// </summary>
    /// <remarks>
    /// Query parameter collections are always sealed after construction from
    /// a URL. This property will always return <see langword="true"/> on a
    /// fully constructed instance.
    /// </remarks>
    bool IsSealed { get; }

    /// <summary>
    /// Gets the number of distinct parameter names in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the first value associated with the specified parameter name, or
    /// <see langword="null"/> if the parameter is not present.
    /// </summary>
    /// <param name="key">The parameter name to look up.</param>
    string? this[string key] { get; }

    /// <summary>
    /// Determines whether the collection contains at least one value for the
    /// specified parameter name.
    /// </summary>
    /// <param name="key">The parameter name to check.</param>
    bool ContainsKey(string key);

    /// <summary>
    /// Attempts to retrieve the first value for the specified parameter name.
    /// </summary>
    /// <param name="key">The parameter name to look up.</param>
    /// <param name="value">
    /// When this method returns, contains the first value associated with
    /// <paramref name="key"/>, or <see langword="null"/> if not found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the parameter was found; otherwise
    /// <see langword="false"/>.
    /// </returns>
    bool TryGetValue(string key, [NotNullWhen(true)] out string? value);

    /// <summary>
    /// Returns all values for the specified parameter name, or an empty list
    /// if the parameter is not present.
    /// </summary>
    /// <param name="key">The parameter name to look up.</param>
    IReadOnlyList<string> GetValues(string key);

    /// <summary>
    /// Attempts to retrieve all values for the specified parameter name.
    /// </summary>
    /// <param name="key">The parameter name to look up.</param>
    /// <param name="values">
    /// When this method returns, contains all values associated with
    /// <paramref name="key"/>, or <see langword="null"/> if not found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the parameter was found; otherwise
    /// <see langword="false"/>.
    /// </returns>
    bool TryGetValues(string key, [NotNullWhen(true)] out IReadOnlyList<string>? values);
}