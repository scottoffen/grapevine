using System.Collections.Concurrent;
using System.Reflection;

namespace Grapevine;

/// <summary>
/// Represents an HTTP method for use in request routing, providing a shared registry
/// for case-insensitive lookup by name and support for a wildcard <see cref="Any"/> value.
/// </summary>
/// <remarks>
/// <para>
/// The well-known HTTP methods defined by <see cref="System.Net.Http.HttpMethod"/> (e.g.
/// <c>GET</c>, <c>POST</c>, <c>PUT</c>, <c>DELETE</c>) are automatically registered at
/// startup alongside <see cref="Any"/> and any other public static fields declared on
/// this class.
/// </para>
/// <para>
/// Custom HTTP methods can be registered via <see cref="Register"/> and looked up via
/// <see cref="FromMethod"/>. Lookups are case-insensitive.
/// </para>
/// <para>
/// Equality is based solely on <see cref="Method"/> using a case-insensitive ordinal
/// comparison. Use <see cref="Equivalent"/> rather than equality when wildcard matching
/// against <see cref="Any"/> is desired.
/// </para>
/// </remarks>
public class HttpMethod
{
    /// <summary>
    /// A wildcard HTTP method that matches any other method when used with
    /// <see cref="Equivalent"/>. Use when a handler should respond regardless
    /// of the HTTP method used in the request.
    /// </summary>
    public static readonly HttpMethod Any = new HttpMethod("Any");

    private static readonly ConcurrentDictionary<string, HttpMethod> _httpMethods
        = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes the static registry by reflecting over all public static fields of
    /// type <see cref="HttpMethod"/> on this class, then reflecting over all public
    /// static properties of <see cref="System.Net.Http.HttpMethod"/> to register the
    /// well-known base class methods. Lookups are case-insensitive.
    /// </summary>
    static HttpMethod()
    {
        var ct = typeof(HttpMethod);
        var fields = ct.GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var field in fields)
        {
            if (field.GetValue(null) is HttpMethod method)
                _httpMethods.TryAdd(method.Method, method);
        }

        var baseType = typeof(System.Net.Http.HttpMethod);
        var baseProperties = baseType.GetProperties(BindingFlags.Public | BindingFlags.Static);
        foreach (var prop in baseProperties)
        {
            if (prop.GetValue(null) is System.Net.Http.HttpMethod baseMethod)
                _httpMethods.TryAdd(baseMethod.Method, new HttpMethod(baseMethod.Method));
        }
    }

    /// <summary>
    /// Gets the HTTP method name, e.g. <c>"GET"</c> or <c>"POST"</c>.
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="HttpMethod"/> with the specified method name.
    /// </summary>
    /// <param name="method">The HTTP method name, e.g. <c>"GET"</c> or <c>"PATCH"</c>.</param>
    public HttpMethod(string method)
    {
        Method = method;
    }

    /// <summary>
    /// Determines whether this method is equivalent to another, treating <see cref="Any"/>
    /// as a wildcard that matches all methods. Two non-wildcard methods are equivalent only
    /// if they are equal.
    /// </summary>
    /// <param name="other">The <see cref="HttpMethod"/> to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if either method is <see cref="Any"/>, or if the two methods
    /// are equal; otherwise <see langword="false"/>.
    /// </returns>
    public bool Equivalent(HttpMethod other)
    {
        if (this.Equals(Any)) return true;
        if (other.Equals(Any)) return true;
        return this.Equals(other);
    }

    /// <summary>
    /// Returns the HTTP method name.
    /// </summary>
    public override string ToString() => Method;

    /// <summary>
    /// Determines whether this instance is equal to another object. Equality is based
    /// solely on <see cref="Method"/> using a case-insensitive ordinal comparison.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is an <see cref="HttpMethod"/>
    /// or <see cref="string"/> whose method name matches <see cref="Method"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            HttpMethod m => string.Equals(Method, m.Method, StringComparison.OrdinalIgnoreCase),
            string s => string.Equals(Method, s, StringComparison.OrdinalIgnoreCase),
            System.Net.Http.HttpMethod m => string.Equals(Method, m.Method, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    /// <summary>
    /// Returns a hash code based solely on <see cref="Method"/>, consistent with the
    /// equality contract defined by <see cref="Equals"/>.
    /// </summary>
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Method);

    /// <summary>Returns <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> have the same method name.</summary>
    public static bool operator ==(HttpMethod left, HttpMethod right) => left.Equals(right);

    /// <summary>Returns <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> have different method names.</summary>
    public static bool operator !=(HttpMethod left, HttpMethod right) => !left.Equals(right);

    /// <summary>Returns <see langword="true"/> if <paramref name="left"/> has the same method name as <paramref name="right"/>.</summary>
    public static bool operator ==(HttpMethod left, string right) => left.Equals(right);

    /// <summary>Returns <see langword="true"/> if <paramref name="left"/> has a different method name than <paramref name="right"/>.</summary>
    public static bool operator !=(HttpMethod left, string right) => !left.Equals(right);

    /// <summary>Returns <see langword="true"/> if <paramref name="left"/> matches the method name of <paramref name="right"/>.</summary>
    public static bool operator ==(string left, HttpMethod right) => right.Equals(left);

    /// <summary>Returns <see langword="true"/> if <paramref name="left"/> does not match the method name of <paramref name="right"/>.</summary>
    public static bool operator !=(string left, HttpMethod right) => !right.Equals(left);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="HttpMethod"/> by
    /// looking up or registering the value in the shared dictionary.
    /// </summary>
    /// <param name="value">The HTTP method name to convert.</param>
    public static implicit operator HttpMethod(string value) => FromMethod(value);

    /// <summary>
    /// Implicitly converts an <see cref="HttpMethod"/> to its <see cref="string"/>
    /// method name, e.g. <c>"GET"</c>.
    /// </summary>
    /// <param name="value">The <see cref="HttpMethod"/> instance to convert.</param>
    public static implicit operator string(HttpMethod value) => value.Method;

    /// <summary>
    /// Implicitly converts a <see cref="System.Net.Http.HttpMethod"/> to a
    /// <see cref="HttpMethod"/> by looking up or registering its method name
    /// in the shared dictionary.
    /// </summary>
    /// <param name="value">The <see cref="System.Net.Http.HttpMethod"/> instance to convert.</param>
    public static implicit operator HttpMethod(System.Net.Http.HttpMethod value)
        => FromMethod(value.Method);

    /// <summary>
    /// Implicitly converts a <see cref="HttpMethod"/> to a
    /// <see cref="System.Net.Http.HttpMethod"/> using its method name.
    /// </summary>
    /// <param name="value">The <see cref="HttpMethod"/> instance to convert.</param>
    public static implicit operator System.Net.Http.HttpMethod(HttpMethod value)
        => new System.Net.Http.HttpMethod(value.Method);

    /// <summary>
    /// Retrieves the <see cref="HttpMethod"/> for the specified method name, registering
    /// a new instance if one is not already present. Lookups are case-insensitive.
    /// </summary>
    /// <param name="value">The HTTP method name to look up, e.g. <c>"GET"</c>.</param>
    /// <returns>The corresponding <see cref="HttpMethod"/> instance.</returns>
    public static HttpMethod FromMethod(string value)
        => _httpMethods.GetOrAdd(value, v => new HttpMethod(v));

    /// <summary>
    /// Registers a new <see cref="HttpMethod"/> in the shared dictionary if the specified
    /// method name is not already present. Has no effect if the method is already registered.
    /// </summary>
    /// <param name="value">The HTTP method name to register, e.g. <c>"PROPFIND"</c>.</param>
    public static void Register(string value)
        => _httpMethods.TryAdd(value, new HttpMethod(value));
}