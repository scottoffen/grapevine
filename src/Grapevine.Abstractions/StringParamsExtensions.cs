using System.Diagnostics.CodeAnalysis;
using Grapevine.Abstractions;

namespace Grapevine;

/// <summary>
/// Provides strongly-typed value retrieval extension methods for
/// <see cref="IStringParams"/> and any interface that extends it, including
/// <see cref="IQueryParams"/> and <see cref="IRouteParams"/>. Delegates to
/// the <see cref="ConverterRegistry"/>.
/// </summary>
/// <remarks>
/// <para>
/// These methods are implemented as extensions rather than interface members
/// so that the interface remains independent of the converter registry, and
/// so that test doubles implementing <see cref="IStringParams"/> do not need
/// to replicate conversion logic.
/// </para>
/// <para>
/// Converters for additional types can be registered at application startup
/// via <see cref="ConverterRegistry.Register{T}"/>.
/// </para>
/// </remarks>
public static class StringParamsExtensions
{
    /// <summary>
    /// Attempts to retrieve and convert the first value for the specified
    /// key to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="params">The string parameter collection.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="value">
    /// When this method returns <see langword="true"/>, contains the converted
    /// value. When this method returns <see langword="false"/>, contains the
    /// default value for <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the key was found and converted successfully;
    /// otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="params"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no converter is registered for <typeparamref name="T"/>.
    /// </exception>
    /// <remarks>
    /// For value types, use the nullable form of the type parameter. For example,
    /// use <c>TryGetValue&lt;int?&gt;</c> rather than <c>TryGetValue&lt;int&gt;</c>.
    /// Built-in converters are registered for <c>int?</c>, <c>long?</c>,
    /// <c>double?</c>, <c>decimal?</c>, <c>bool?</c>, <c>Guid?</c>, and
    /// <c>DateTimeOffset?</c>. See <see cref="ConverterRegistry"/> for a full
    /// explanation and for registering custom type converters.
    /// </remarks>
    public static bool TryGetValue<T>(
        this IStringParams @params,
        string key,
        [NotNullWhen(true)] out T? value)
    {
        if (@params is null)
            throw new ArgumentNullException(nameof(@params));

        if (!@params.TryGetValue(key, out var raw))
        {
            value = default;
            return false;
        }

        return ConverterRegistry.TryConvert(raw, out value);
    }

    /// <summary>
    /// Retrieves and converts the first value for the specified key to
    /// <typeparamref name="T"/>, or returns <paramref name="defaultValue"/>
    /// if the key is not present or the value cannot be converted.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="params">The string parameter collection.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">
    /// The value to return when the key is absent or conversion fails.
    /// </param>
    /// <returns>
    /// The converted value if the key exists and conversion succeeds;
    /// otherwise <paramref name="defaultValue"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="params"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no converter is registered for <typeparamref name="T"/>.
    /// </exception>
    /// <remarks>
    /// For value types, use the nullable form of the type parameter. For example,
    /// use <c>GetValue&lt;int?&gt;</c> rather than <c>GetValue&lt;int&gt;</c>.
    /// See <see cref="ConverterRegistry"/> for a full explanation and for
    /// registering custom type converters.
    /// </remarks>
    public static T? GetValue<T>(
        this IStringParams @params,
        string key,
        T? defaultValue = default)
    {
        if (@params is null)
            throw new ArgumentNullException(nameof(@params));

        return TryGetValue<T>(@params, key, out var value)
            ? value
            : defaultValue;
    }
}