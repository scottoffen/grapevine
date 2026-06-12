namespace Grapevine;

/// <summary>
/// A per-request ambient state bag for passing arbitrary data between middleware,
/// route handlers, and other request pipeline components within the scope of a
/// single request.
/// </summary>
/// <remarks>
/// Keys and values are untyped to allow maximum flexibility. Use
/// <see cref="GetAs{T}"/> and <see cref="GetOrAddAs{T}(object, T)"/> for typed access.
/// </remarks>
public class Locals : Dictionary<object, object?>
{
    /// <summary>
    /// Retrieves the value associated with the specified key, or <see langword="null"/>
    /// if the key is not present. This method never throws for a missing key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>
    /// The value associated with <paramref name="key"/>, or <see langword="null"/> if
    /// the key is not found.
    /// </returns>
    public object? Get(object key)
    {
        return TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Retrieves the value associated with the specified key and casts it to
    /// <typeparamref name="T"/>. Returns <see langword="default"/> if the key is
    /// not present.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <param name="key">The key to look up.</param>
    /// <returns>
    /// The value associated with <paramref name="key"/> cast to <typeparamref name="T"/>,
    /// or <see langword="default"/> if the key is not found.
    /// </returns>
    public T? GetAs<T>(object key)
    {
        var value = Get(key);
        return value is null ? default : (T)value;
    }

    /// <summary>
    /// Retrieves the value associated with the specified key and casts it to
    /// <typeparamref name="T"/>, or adds and returns <paramref name="value"/> if
    /// the key is not present.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key to look up or add.</param>
    /// <param name="value">The value to add if the key is not present.</param>
    /// <returns>
    /// The existing value cast to <typeparamref name="T"/> if the key is present,
    /// or <paramref name="value"/> if it was just added.
    /// </returns>
    public T GetOrAddAs<T>(object key, T value)
    {
        if (TryGetValue(key, out var existing))
            return (T)existing!;
        this[key] = value;
        return value;
    }

    /// <summary>
    /// Retrieves the value associated with the specified key and casts it to
    /// <typeparamref name="T"/>, or adds and returns the value produced by
    /// <paramref name="factory"/> if the key is not present.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key to look up or add.</param>
    /// <param name="factory">
    /// A factory function that produces the value to add if the key is not present.
    /// The function receives the key as its argument.
    /// </param>
    /// <returns>
    /// The existing value cast to <typeparamref name="T"/> if the key is present,
    /// or the value produced by <paramref name="factory"/> if it was just added.
    /// </returns>
    public T GetOrAddAs<T>(object key, Func<object, T> factory)
    {
        if (TryGetValue(key, out var existing))
            return (T)existing!;
        var value = factory(key);
        this[key] = value;
        return value;
    }
}