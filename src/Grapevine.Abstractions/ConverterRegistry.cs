namespace Grapevine.Abstractions;

/// <summary>
/// Represents a method that converts a raw string value into an instance of
/// <typeparamref name="T"/>, or returns <see langword="null"/> if the value
/// cannot be converted.
/// </summary>
/// <typeparam name="T">The target type to convert to.</typeparam>
/// <param name="value">The raw string value to convert.</param>
/// <returns>
/// The converted value, or <see langword="null"/> if conversion fails.
/// </returns>
public delegate T? ValueConverter<T>(string value);

/// <summary>
/// Provides a general-purpose registry of delegate-based converters for parsing
/// raw string values into strongly-typed instances.
/// </summary>
/// <remarks>
/// <para>
/// Converters are registered as <see cref="ValueConverter{T}"/> delegates.
/// Built-in converters are registered automatically for the most common types.
/// Custom converters for application-specific types can be registered at startup
/// via <see cref="Register{T}"/>.
/// </para>
/// <para>
/// The registry is global mutable state intended to be configured once at
/// application startup, before any requests are processed. Registrations are
/// not thread-safe during the configuration phase, but reads during request
/// handling are safe since the underlying dictionary is not modified after
/// startup.
/// </para>
/// <para>
/// This approach is fully AOT-compatible. Delegate invocation does not require
/// runtime reflection, and the trimmer can statically analyse all registered
/// converter implementations.
/// </para>
/// <para>
/// <strong>Value type registration:</strong> value types must be registered and
/// looked up as their nullable counterparts. For example, the built-in converter
/// for integers is registered as <c>int?</c>, not <c>int</c>. When registering
/// a custom value type converter, use <c>Register&lt;MyStruct?&gt;</c> and call
/// <c>TryConvert&lt;MyStruct?&gt;</c> accordingly. This is required because
/// <see cref="ValueConverter{T}"/> returns <c>T?</c>, and for a non-nullable
/// value type <c>T</c>, the return type <c>T?</c> resolves to <c>T</c> rather
/// than <c>Nullable&lt;T&gt;</c>, making it impossible to return
/// <see langword="null"/> on parse failure. Using the nullable form as <c>T</c>
/// avoids this ambiguity entirely.
/// </para>
/// </remarks>
public static class ConverterRegistry
{
    /// <summary>
    /// The error message used when no converter is registered for a requested type.
    /// </summary>
    internal static readonly string NoConverterMessage =
        "No converter is registered for type '{0}'. " +
        "Call ConverterRegistry.Register<T> at startup to add one.";

    private static readonly Dictionary<Type, Delegate> _converters = new();

    static ConverterRegistry()
    {
        // Register built-in converters for common types. Each converter returns
        // null when the input string cannot be parsed, rather than throwing.
        // Value types are registered as their nullable counterparts so that
        // ValueConverter<T?> has a return type of T? with no implicit conversion
        // ambiguity between T and Nullable<T>.
        Register<string>(new ValueConverter<string>(s => s));

        Register<int?>(new ValueConverter<int?>(
            s => int.TryParse(s, out var v) ? v : null));

        Register<long?>(new ValueConverter<long?>(
            s => long.TryParse(s, out var v) ? v : null));

        Register<double?>(new ValueConverter<double?>(s => double.TryParse(
            s,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var v) ? v : null));

        Register<decimal?>(new ValueConverter<decimal?>(s => decimal.TryParse(
            s,
            System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture,
            out var v) ? v : null));

        Register<bool?>(new ValueConverter<bool?>(
            s => bool.TryParse(s, out var v) ? v : null));

        Register<Guid?>(new ValueConverter<Guid?>(
            s => Guid.TryParse(s, out var v) ? v : null));

        Register<DateTimeOffset?>(new ValueConverter<DateTimeOffset?>(s => DateTimeOffset.TryParse(
            s,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.RoundtripKind,
            out var v) ? v : null));
    }

    /// <summary>
    /// Registers a converter for the specified type, replacing any existing
    /// registration for that type.
    /// </summary>
    /// <typeparam name="T">The type to register a converter for.</typeparam>
    /// <param name="converter">
    /// A <see cref="ValueConverter{T}"/> delegate that converts a raw string
    /// value to an instance of <typeparamref name="T"/>, or
    /// <see langword="null"/> if the value cannot be converted.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="converter"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Call this method at application startup before any requests are processed.
    /// Registrations made after startup are not guaranteed to be visible to
    /// concurrent request handlers.
    /// </para>
    /// <para>
    /// For value types, register and call using the nullable form of the type.
    /// For example, use <c>Register&lt;int?&gt;</c> rather than
    /// <c>Register&lt;int&gt;</c>. See the <see cref="ConverterRegistry"/> class
    /// remarks for a full explanation.
    /// </para>
    /// </remarks>
    public static void Register<T>(ValueConverter<T> converter)
    {
        if (converter is null)
            throw new ArgumentNullException(nameof(converter));

        _converters[typeof(T)] = converter;
    }

    /// <summary>
    /// Attempts to convert a raw string value to the specified type using the
    /// registered converter.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="raw">The raw string value to convert.</param>
    /// <param name="value">
    /// When this method returns <see langword="true"/>, contains the converted
    /// value. When this method returns <see langword="false"/>, contains the
    /// default value for <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a converter is registered and returns a
    /// non-null result; otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no converter is registered for <typeparamref name="T"/>.
    /// </exception>
    /// <remarks>
    /// For value types, call using the nullable form of the type. For example,
    /// use <c>TryConvert&lt;int?&gt;</c> rather than <c>TryConvert&lt;int&gt;</c>.
    /// The built-in converters for <see cref="int"/>, <see cref="long"/>,
    /// <see cref="double"/>, <see cref="decimal"/>, <see cref="bool"/>,
    /// <see cref="Guid"/>, and <see cref="DateTimeOffset"/> are all registered
    /// under their nullable forms. See the <see cref="ConverterRegistry"/> class
    /// remarks for a full explanation.
    /// </remarks>
    public static bool TryConvert<T>(string raw, out T? value)
    {
        if (!_converters.TryGetValue(typeof(T), out var del))
            throw new InvalidOperationException(
                string.Format(NoConverterMessage, typeof(T).FullName));

        var result = ((ValueConverter<T>)del)(raw);

        if (result is null)
        {
            value = default;
            return false;
        }

        value = result;
        return true;
    }

    /// <summary>
    /// Determines whether a converter is registered for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns>
    /// <see langword="true"/> if a converter is registered; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public static bool IsRegistered<T>() => _converters.ContainsKey(typeof(T));
}