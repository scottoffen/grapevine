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
    /// Thrown when <paramref name="collection"/> is null or <paramref name="key"/> is
    /// null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="key"/> is not found in the collection.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no type converter exists for <typeparamref name="T"/>.
    /// </exception>
    public static T? GetValue<T>(this NameValueCollection collection, string key)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        if (!collection.TryGetValue(key, out var value))
            throw new ArgumentOutOfRangeException(nameof(key), $"Key '{key}' not found in collection.");

        try
        {
            var converter = GetTypeConverter(typeof(T));
            return (T?)converter.ConvertFrom(value!);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception)
        {
            return default;
        }
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
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="collection"/> is null or <paramref name="key"/> is
    /// null or whitespace.
    /// </exception>
    public static T? GetValue<T>(this NameValueCollection collection, string key, T defaultValue)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        if (collection.TryGetValue(key, out var value))
        {
            try
            {
                var converter = GetTypeConverter(typeof(T));
                return (T?)converter.ConvertFrom(value!);
            }
            catch (Exception)
            {
                // Silently return defaultValue on any conversion failure.
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Attempts to retrieve the value associated with the specified key.
    /// </summary>
    /// <param name="collection">The collection to retrieve the value from.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with <paramref name="key"/>,
    /// or <see langword="null"/> if the key was not found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the key exists and has a non-null value; otherwise
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="collection"/> is null or <paramref name="key"/> is
    /// null or whitespace.
    /// </exception>
    public static bool TryGetValue(this NameValueCollection collection, string key, out string? value)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        value = collection[key];
        return value != null;
    }

    /// <summary>
    /// Retrieves the cached <see cref="TypeConverter"/> for the specified type, or
    /// creates and caches a new one. Throws if no converter capable of converting from
    /// <see cref="string"/> exists for the type.
    /// </summary>
    /// <param name="type">The type to retrieve a converter for.</param>
    /// <returns>A <see cref="TypeConverter"/> that can convert from <see cref="string"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no type converter capable of converting from <see cref="string"/>
    /// exists for <paramref name="type"/>.
    /// </exception>
    private static TypeConverter GetTypeConverter(Type type)
    {
        return _converters.GetOrAdd(type, t =>
        {
            var converter = TypeDescriptor.GetConverter(t);
            if (converter.CanConvertFrom(typeof(string)))
                return converter;

            throw new InvalidOperationException($"No type converter found for type '{t.FullName}'.");
        });
    }
}