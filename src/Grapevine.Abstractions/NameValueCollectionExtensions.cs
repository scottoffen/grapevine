using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Grapevine;

/// <summary>
/// Provides extension methods for <see cref="NameValueCollection"/> that support
/// strongly-typed value retrieval and quality value header parsing.
/// </summary>
public static class NameValueCollectionExtensions
{
    private static readonly ConcurrentDictionary<Type, TypeConverter> _converters = new();

    /// <summary>
    /// Retrieves the value associated with the specified key and converts it to
    /// <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="collection">The collection to retrieve the value from.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The value associated with <paramref name="key"/> converted to <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="collection"/> or <paramref name="key"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="key"/> is not found in the collection.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the value cannot be converted to <typeparamref name="T"/>.
    /// </exception>
    public static T GetValue<T>(this NameValueCollection collection, string key)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection), "Missing collection");
        if (key == null) throw new ArgumentNullException(nameof(key), "Missing key");

        var value = collection[key];
        if (value == null) throw new ArgumentOutOfRangeException(nameof(key), $"Key {key} not found in collection");

        var converter = _converters.GetOrAdd(typeof(T), t => TypeDescriptor.GetConverter(t));

        if (!converter.CanConvertFrom(typeof(string)))
            throw new ArgumentException($"Cannot convert '{value}' to {typeof(T)}");

        var result = converter.ConvertFrom(value);
        if (result == null)
            throw new ArgumentException($"Conversion of '{value}' to {typeof(T)} returned null");

        return (T)result;
    }

    /// <summary>
    /// Retrieves the value associated with the specified key and converts it to
    /// <typeparamref name="T"/>, or returns <paramref name="defaultValue"/> if the key
    /// is not present or the value cannot be converted.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="collection">The collection to retrieve the value from.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">The value to return if the key is not found or conversion fails.</param>
    /// <returns>
    /// The converted value if the key exists and conversion succeeds, otherwise
    /// <paramref name="defaultValue"/>.
    /// </returns>
    public static T GetValue<T>(this NameValueCollection collection, string key, T defaultValue)
    {
        if (collection == null || key == null || collection[key] == null)
            return defaultValue;

        try
        {
            return collection.GetValue<T>(key);
        }
        catch (ArgumentException)
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Retrieves the value associated with the specified key and parses it as a
    /// quality value header, returning an ordered list of values from most preferred
    /// to least preferred.
    /// </summary>
    /// <remarks>
    /// If the key is not present or the value is empty, an empty list is returned.
    /// Single values with no quality factor are returned as a single-element list
    /// without parsing overhead.
    /// </remarks>
    /// <param name="collection">The collection to retrieve the value from.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>
    /// An ordered list of values from most preferred to least preferred, or an empty
    /// list if the key is not present or the value is empty.
    /// </returns>
    public static IList<string> SortQualityValues(this NameValueCollection collection, string key)
    {
        var unparsed = collection.GetValue<string>(key, string.Empty);

        if (string.IsNullOrWhiteSpace(unparsed)) return Array.Empty<string>();

        return QualityValues.Parse(unparsed);
    }
}