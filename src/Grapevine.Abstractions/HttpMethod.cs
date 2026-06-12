using System.Collections.Concurrent;
using System.Diagnostics;

namespace Grapevine;

/// <summary>
/// Represents an HTTP method, providing a shared registry for case-insensitive lookup
/// by name and support for a wildcard <see cref="Any"/> value.
/// </summary>
/// <remarks>
/// <para>
/// The well-known HTTP methods (e.g. <c>GET</c>, <c>POST</c>, <c>PUT</c>,
/// <c>DELETE</c>) are provided as static readonly fields and automatically registered
/// at startup. Custom HTTP methods can be registered via <see cref="Register"/> and
/// looked up via <see cref="Parse"/>. Lookups are case-insensitive.
/// </para>
/// <para>
/// Use <see cref="Matches"/> rather than equality when wildcard matching against
/// <see cref="Any"/> is desired.
/// </para>
/// </remarks>
[DebuggerDisplay("{Name}")]
public partial class HttpMethod : IEquatable<HttpMethod>
{
    /// <summary>
    /// Gets the uppercase name of the HTTP method, e.g. <c>"GET"</c> or <c>"POST"</c>.
    /// </summary>
    public string Name { get; }

    private HttpMethod(string name)
    {
        Name = name.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Determines whether this instance is equal to another <see cref="HttpMethod"/>.
    /// Equality is based on <see cref="Name"/> using a case-insensitive ordinal comparison.
    /// </summary>
    /// <param name="other">The <see cref="HttpMethod"/> to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if both instances have the same <see cref="Name"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool Equals(HttpMethod? other)
        => other is not null && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this instance is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    public override bool Equals(object? obj)
        => obj is HttpMethod other && Equals(other);

    /// <summary>
    /// Returns a hash code based on <see cref="Name"/>, consistent with the equality
    /// contract defined by <see cref="Equals(HttpMethod?)"/>.
    /// </summary>
    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);

    /// <summary>
    /// Determines whether this instance matches the specified <see cref="HttpMethod"/>,
    /// treating <see cref="Any"/> as a wildcard that matches all methods.
    /// </summary>
    /// <param name="method">The <see cref="HttpMethod"/> to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if either instance is <see cref="Any"/>, or if both
    /// instances are equal; otherwise <see langword="false"/>.
    /// </returns>
    public bool Matches(HttpMethod? method)
        => Equals(method) || method == Any || this == Any;

    /// <summary>
    /// Returns the HTTP method name.
    /// </summary>
    public override string ToString() => Name;
}

/// <summary>
/// Provides equality and conversion operators for <see cref="HttpMethod"/>.
/// </summary>
public partial class HttpMethod
{
    /// <summary>Returns <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> have the same method name.</summary>
    public static bool operator ==(HttpMethod? left, HttpMethod? right)
        => left?.Equals(right) ?? right is null;

    /// <summary>Returns <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> have different method names.</summary>
    public static bool operator !=(HttpMethod? left, HttpMethod? right)
        => !(left == right);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="HttpMethod"/> by
    /// looking up or registering the value in the shared dictionary.
    /// </summary>
    /// <param name="name">The HTTP method name to convert.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="name"/> is null or whitespace.
    /// </exception>
    public static implicit operator HttpMethod(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name), "HTTP method name cannot be null or empty.");

        return Parse(name);
    }

    /// <summary>
    /// Implicitly converts an <see cref="HttpMethod"/> to its <see cref="string"/>
    /// method name, e.g. <c>"GET"</c>.
    /// </summary>
    /// <param name="method">The <see cref="HttpMethod"/> instance to convert.</param>
    public static implicit operator string(HttpMethod method) => method.Name;
}

/// <summary>
/// Provides the static registry, parsing, and well-known method fields for
/// <see cref="HttpMethod"/>.
/// </summary>
public partial class HttpMethod
{
    private static readonly ConcurrentDictionary<string, HttpMethod> _methods
        = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a read-only collection of all known HTTP methods.
    /// </summary>
    public static IReadOnlyCollection<HttpMethod> Known => _methods.Values.ToArray();

    /// <summary>
    /// Parses a string representation of an HTTP method into an <see cref="HttpMethod"/>
    /// instance, registering it in the shared dictionary if not already present.
    /// Lookups are case-insensitive.
    /// </summary>
    /// <param name="name">The HTTP method name to parse, e.g. <c>"GET"</c>.</param>
    /// <returns>The corresponding <see cref="HttpMethod"/> instance.</returns>
    public static HttpMethod Parse(string name)
    {
        var method = new HttpMethod(name);
        return _methods.GetOrAdd(method.Name, method);
    }

    /// <summary>
    /// Registers a new <see cref="HttpMethod"/> in the shared dictionary if the specified
    /// method name is not already present. Has no effect if the method is already registered.
    /// </summary>
    /// <param name="name">The HTTP method name to register, e.g. <c>"PROPFIND"</c>.</param>
    public static void Register(string name)
        => _methods.TryAdd(name.Trim().ToUpperInvariant(), new HttpMethod(name));

    /// <summary>
    /// A wildcard HTTP method that matches any other method when used with
    /// <see cref="Matches"/>. Use when a handler should respond regardless of the
    /// HTTP method used in the request.
    /// </summary>
    public static readonly HttpMethod Any = Parse("*");

    /// <summary>Represents an HTTP CONNECT method.</summary>
    public static readonly HttpMethod Connect = Parse("Connect");

    /// <summary>Represents an HTTP DELETE method.</summary>
    public static readonly HttpMethod Delete = Parse("Delete");

    /// <summary>Represents an HTTP GET method.</summary>
    public static readonly HttpMethod Get = Parse("Get");

    /// <summary>Represents an HTTP HEAD method.</summary>
    public static readonly HttpMethod Head = Parse("Head");

    /// <summary>Represents an HTTP OPTIONS method.</summary>
    public static readonly HttpMethod Options = Parse("Options");

    /// <summary>Represents an HTTP PATCH method.</summary>
    public static readonly HttpMethod Patch = Parse("Patch");

    /// <summary>Represents an HTTP POST method.</summary>
    public static readonly HttpMethod Post = Parse("Post");

    /// <summary>Represents an HTTP PUT method.</summary>
    public static readonly HttpMethod Put = Parse("Put");

    /// <summary>Represents an HTTP TRACE method.</summary>
    public static readonly HttpMethod Trace = Parse("Trace");
}