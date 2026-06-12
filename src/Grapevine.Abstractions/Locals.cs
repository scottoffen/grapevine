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
    /// Retrieves the value associated with the specified key and returns it as
    /// <typeparamref name="T"/>. Returns <see langword="default"/> if the key is
    /// not present or the value is not of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to return the value as.</typeparam>
    /// <param name="key">The key to look up.</param>
    /// <returns>
    /// The value associated with <paramref name="key"/> as <typeparamref name="T"/>,
    /// or <see langword="default"/> if the key is not found or the value is not of
    /// type <typeparamref name="T"/>.
    /// </returns>
    public T? GetAs<T>(object key)
    {
        if (!TryGetValue(key, out var value)) return default;
        return value is T t ? t : default;
    }

    /// <summary>
    /// Retrieves the value associated with the specified key and returns it as
    /// <typeparamref name="T"/>, or adds and returns <paramref name="value"/> if
    /// the key is not present.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key to look up or add.</param>
    /// <param name="value">The value to add if the key is not present.</param>
    /// <returns>
    /// The existing value as <typeparamref name="T"/> if the key is present,
    /// or <paramref name="value"/> if it was just added.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Thrown when the existing value is not of type <typeparamref name="T"/>.
    /// </exception>
    public T? GetOrAddAs<T>(object key, T value)
    {
        if (TryGetValue(key, out var existing))
        {
            if (existing is null) return default;
            if (existing is T t) return t;
            throw new InvalidCastException($"Value for key '{key}' is of type '{existing.GetType()}', expected type '{typeof(T)}'.");
        }
        this[key] = value;
        return value;
    }

    /// <summary>
    /// Retrieves the value associated with the specified key and returns it as
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
    /// The existing value as <typeparamref name="T"/> if the key is present,
    /// or the value produced by <paramref name="factory"/> if it was just added.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Thrown when the existing value is not of type <typeparamref name="T"/>.
    /// </exception>
    public T? GetOrAddAs<T>(object key, Func<object, T> factory)
    {
        if (TryGetValue(key, out var existing))
        {
            if (existing is null) return default;
            if (existing is T t) return t;
            throw new InvalidCastException($"Value for key '{key}' is of type '{existing.GetType()}', expected type '{typeof(T)}'.");
        }
        var value = factory(key);
        this[key] = value;
        return value;
    }

    /// <summary>
    /// Sets the value for the specified key. If <paramref name="value"/> is
    /// <see langword="null"/>, the key is removed from the collection.
    /// </summary>
    /// <param name="key">The key to set or remove.</param>
    /// <param name="value">
    /// The value to associate with <paramref name="key"/>, or <see langword="null"/>
    /// to remove the key from the collection.
    /// </param>
    public void Set(object key, object? value)
    {
        if (value is null)
            Remove(key);
        else
            this[key] = value;
    }
}