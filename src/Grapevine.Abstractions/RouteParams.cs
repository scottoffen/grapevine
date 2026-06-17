using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Grapevine.Abstractions;

/// <summary>
/// Default implementation of <see cref="Grapevine.IRouteParams"/>, representing
/// the route parameters extracted from a matched URL path as a read-only,
/// case-insensitive collection of name-value pairs.
/// </summary>
/// <remarks>
/// <para>
/// Consumers should type against <see cref="Grapevine.IRouteParams"/> rather
/// than this concrete type. This class is public to support alternative server
/// implementations and testing.
/// </para>
/// <para>
/// Instances are populated by the routing infrastructure via the internal
/// <see cref="Add"/> method and are sealed immediately after population.
/// No mutation is possible after sealing.
/// </para>
/// <para>
/// Each route parameter name corresponds to a named capture group in the route
/// pattern and appears at most once. Key lookup is case-insensitive.
/// </para>
/// </remarks>
[DebuggerDisplay("Count = {Count}, IsSealed = {IsSealed}")]
public sealed class RouteParams : Grapevine.IRouteParams
{
    /// <summary>
    /// An empty <see cref="RouteParams"/> instance, used when a route has no
    /// named capture groups.
    /// </summary>
    public static readonly RouteParams Empty = new RouteParams().SealAndReturn();

    /// <summary>
    /// The exception message used when a mutation is attempted on a sealed collection.
    /// </summary>
    internal static readonly string SealedCollectionMessage =
        "The route parameter collection has been sealed and cannot be modified.";

    private readonly Dictionary<string, string> _params =
        new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public bool IsSealed { get; private set; }

    /// <inheritdoc/>
    public int Count => _params.Count;

    /// <inheritdoc/>
    public string? this[string key] =>
        _params.TryGetValue(key, out var value) ? value : null;

    /// <inheritdoc/>
    public bool ContainsKey(string key) => _params.ContainsKey(key);

    /// <inheritdoc/>
    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value) =>
        _params.TryGetValue(key, out value);

    /// <inheritdoc/>
    public IReadOnlyList<string> GetValues(string key)
    {
        if (_params.TryGetValue(key, out var value))
            return [value];

        return Array.Empty<string>();
    }

    /// <inheritdoc/>
    public bool TryGetValues(string key, [NotNullWhen(true)] out IReadOnlyList<string>? values)
    {
        if (_params.TryGetValue(key, out var value))
        {
            values = [value];
            return true;
        }

        values = null;
        return false;
    }

    /// <summary>
    /// Returns an enumerator over all route parameter name and value-list pairs.
    /// </summary>
    public IEnumerator<KeyValuePair<string, IReadOnlyList<string>>> GetEnumerator()
    {
        foreach (var pair in _params)
            yield return new KeyValuePair<string, IReadOnlyList<string>>(
                pair.Key, [pair.Value]);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Adds a route parameter name-value pair to the collection.
    /// </summary>
    /// <param name="key">The route parameter name.</param>
    /// <param name="value">The captured value.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection is sealed.
    /// </exception>
    /// <remarks>
    /// This method is intended for use by the routing infrastructure only.
    /// Call <see cref="Seal"/> after all parameters have been added.
    /// </remarks>
    internal void Add(string key, string value)
    {
        if (IsSealed)
            throw new InvalidOperationException(SealedCollectionMessage);

        _params[key] = value;
    }

    /// <summary>
    /// Seals the collection, preventing any further modifications.
    /// </summary>
    /// <remarks>
    /// Called by the routing infrastructure after all parameters have been
    /// populated from the regex match. Calling <see cref="Seal"/> more than
    /// once has no effect.
    /// </remarks>
    internal void Seal()
    {
        IsSealed = true;
    }

    /// <summary>
    /// Seals the collection and returns the instance, enabling use in field
    /// initialisers.
    /// </summary>
    private RouteParams SealAndReturn()
    {
        Seal();
        return this;
    }
}