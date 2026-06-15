using System.Diagnostics.CodeAnalysis;
using Grapevine.Abstractions;

namespace Grapevine;

/// <summary>
/// Provides strongly-typed value retrieval extension methods for
/// <see cref="IQueryParams"/>, delegating to the
/// <see cref="ConverterRegistry"/>.
/// </summary>
/// <remarks>
/// <para>
/// These methods are implemented as extensions rather than interface members
/// so that the interface remains independent of the converter registry, and
/// so that test doubles implementing <see cref="IQueryParams"/> do not need
/// to replicate conversion logic.
/// </para>
/// <para>
/// Converters for additional types can be registered at application startup
/// via <see cref="ConverterRegistry.Register{T}"/>.
/// </para>
/// </remarks>
public static class QueryParamsExtensions
{
    /// <summary>
    /// Attempts to retrieve and convert the first value for the specified
    /// parameter name to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="queryParams">The query parameter collection.</param>
    /// <param name="key">The parameter name to look up.</param>
    /// <param name="value">
    /// When this method returns <see langword="true"/>, contains the converted
    /// value. When this method returns <see langword="false"/>, contains the
    /// default value for <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the parameter was found and converted
    /// successfully; otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="queryParams"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no converter is registered for <typeparamref name="T"/>.
    /// </exception>
    /// <remarks>
    /// For value types, use the nullable form of the type parameter. For example,
    /// use <c>TryGetValue&lt;int?&gt;</c> rather than <c>TryGetValue&lt;int&gt;</c>.
    /// Built-in converters are registered for <c>int?</c>, <c>long?</c>,
    /// <c>double?</c>, <c>decimal?</c>, <c>bool?</c>, <c>Guid?</c>, and
    /// <c>DateTimeOffset?</c>. See <see cref="Grapevine.Abstractions.ConverterRegistry"/>
    /// for a full explanation and for registering custom type converters.
    /// </remarks>
    public static bool TryGetValue<T>(
        this IQueryParams queryParams,
        string key,
        [NotNullWhen(true)] out T? value)
    {
        if (queryParams is null)
            throw new ArgumentNullException(nameof(queryParams));

        if (!queryParams.TryGetValue(key, out var raw))
        {
            value = default;
            return false;
        }

        return ConverterRegistry.TryConvert(raw, out value);
    }

    /// <summary>
    /// Retrieves and converts the first value for the specified parameter name
    /// to <typeparamref name="T"/>, or returns <paramref name="defaultValue"/>
    /// if the parameter is not present or the value cannot be converted.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="queryParams">The query parameter collection.</param>
    /// <param name="key">The parameter name to look up.</param>
    /// <param name="defaultValue">
    /// The value to return when the parameter is absent or conversion fails.
    /// </param>
    /// <returns>
    /// The converted value if the parameter exists and conversion succeeds;
    /// otherwise <paramref name="defaultValue"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="queryParams"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no converter is registered for <typeparamref name="T"/>.
    /// </exception>
    /// <remarks>
    /// For value types, use the nullable form of the type parameter. For example,
    /// use <c>GetValue&lt;int?&gt;</c> rather than <c>GetValue&lt;int&gt;</c>.
    /// See <see cref="Grapevine.Abstractions.ConverterRegistry"/> for a full
    /// explanation and for registering custom type converters.
    /// </remarks>
    public static T? GetValue<T>(
        this IQueryParams queryParams,
        string key,
        T? defaultValue = default)
    {
        if (queryParams is null)
            throw new ArgumentNullException(nameof(queryParams));

        return TryGetValue<T>(queryParams, key, out var value)
            ? value
            : defaultValue;
    }
}