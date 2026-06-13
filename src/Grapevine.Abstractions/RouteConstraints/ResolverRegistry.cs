using System.Collections.Concurrent;

namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// A registry of <see cref="RouteConstraintResolver"/> delegates used to convert route
/// template constraint segments into <see cref="SegmentDescriptor"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// The following built-in constraints are registered by default:
/// <c>alpha</c>, <c>bool</c>, <c>date</c>, <c>datetime</c>, <c>decimal</c>,
/// <c>double</c>, <c>float</c>, <c>guid</c>, <c>int</c>, <c>len</c>, <c>long</c>,
/// <c>numeric</c>, <c>regex</c>, <c>text</c>.
/// </para>
/// <para>
/// The following aliases are registered and behave identically to their primary constraint:
/// <list type="bullet">
///   <item><term><c>float</c></term><description>alias for <c>double</c></description></item>
///   <item><term><c>long</c></term><description>alias for <c>int</c> (supports negative values)</description></item>
///   <item><term><c>len</c></term><description>alias for <c>text</c></description></item>
/// </list>
/// </para>
/// <para>
/// Use <see cref="RegisterResolver"/> to add new constraints and
/// <see cref="OverrideResolver"/> to replace existing ones.
/// </para>
/// </remarks>
public static class ResolverRegistry
{
    private static readonly ConcurrentDictionary<string, RouteConstraintResolver> _resolvers
        = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes the registry with the built-in route constraint resolvers.
    /// </summary>
    static ResolverRegistry()
    {
        RegisterResolver("alpha",    AlphaResolver.Resolve);
        RegisterResolver("bool",     BoolResolver.Resolve);
        RegisterResolver("date",     DateResolver.Resolve);
        RegisterResolver("datetime", DateTimeResolver.Resolve);
        RegisterResolver("decimal",  DecimalResolver.Resolve);
        RegisterResolver("double",   DoubleResolver.Resolve);
        RegisterResolver("float",    DoubleResolver.Resolve);   // alias for double
        RegisterResolver("guid",     GuidResolver.Resolve);
        RegisterResolver("int",      IntResolver.Resolve);
        RegisterResolver("len",      DefaultResolver.Resolve);  // alias for text
        RegisterResolver("long",     IntResolver.Resolve);      // alias for int
        RegisterResolver("numeric",  NumericResolver.Resolve);
        RegisterResolver("regex",    RegexResolver.Resolve);
        RegisterResolver("text",     DefaultResolver.Resolve);
    }

    /// <summary>
    /// Attempts to resolve a route template segment in the format <c>{name}</c>,
    /// <c>{name:constraint}</c>, or <c>{name:constraint(args)}</c> into a
    /// <see cref="SegmentDescriptor"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If no constraint is specified, the <c>text</c> resolver is used as the default.
    /// Returns <see langword="false"/> if the segment is not in a recognized brace format.
    /// </para>
    /// <para>
    /// Throws if the segment is valid but references an unregistered constraint key.
    /// </para>
    /// </remarks>
    /// <param name="segment">The route template segment to resolve.</param>
    /// <param name="descriptor">
    /// When this method returns <see langword="true"/>, contains the fully populated
    /// <see cref="SegmentDescriptor"/> for the segment; otherwise the default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the segment was successfully resolved; otherwise
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the segment references a constraint key that is not registered.
    /// </exception>
    public static bool TryResolveSegment(string segment, out SegmentDescriptor descriptor)
    {
        descriptor = default;

        if (!TryParseSegment(segment, out var name, out var key, out var args))
            return false;

        if (_resolvers.TryGetValue(key, out var resolver))
        {
            var (pattern, strictness, group) = resolver(name, args);
            descriptor = SegmentDescriptor.Parameter(segment, pattern, name, key, args, strictness, group);
            return true;
        }

        var knownKeys = string.Join(", ", _resolvers.Keys.OrderBy(k => k));
        throw new ArgumentException(
            $"No resolver registered for constraint '{key}'. Known constraints: {knownKeys}.");
    }

    /// <summary>
    /// Registers a new route constraint resolver under the specified key. Throws if the
    /// key is already registered.
    /// </summary>
    /// <param name="key">The constraint name, e.g. <c>"alpha"</c> or <c>"myconstraint"</c>.</param>
    /// <param name="resolver">The resolver delegate to register.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="key"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="resolver"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a resolver is already registered for <paramref name="key"/>.
    /// </exception>
    public static void RegisterResolver(string key, RouteConstraintResolver resolver)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        if (resolver == null)
            throw new ArgumentNullException(nameof(resolver));

        if (!_resolvers.TryAdd(key, resolver))
            throw new InvalidOperationException(
                $"A resolver is already registered for the key '{key}'.");
    }

    /// <summary>
    /// Registers or replaces a route constraint resolver under the specified key.
    /// Silently adds the resolver if the key does not yet exist.
    /// </summary>
    /// <param name="key">The constraint name to register or replace.</param>
    /// <param name="resolver">The resolver delegate to register.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="key"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="resolver"/> is null.
    /// </exception>
    public static void OverrideResolver(string key, RouteConstraintResolver resolver)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        if (resolver == null)
            throw new ArgumentNullException(nameof(resolver));

        _resolvers[key] = resolver;
    }

    /// <summary>
    /// Parses a route template segment into its name, constraint key, and optional
    /// argument components. Recognizes the formats <c>{name}</c>,
    /// <c>{name:constraint}</c>, and <c>{name:constraint(args)}</c>.
    /// </summary>
    /// <param name="input">The segment string to parse, including braces.</param>
    /// <param name="name">The route parameter name extracted from the segment.</param>
    /// <param name="key">
    /// The constraint key extracted from the segment, or <c>"text"</c> if no
    /// constraint was specified.
    /// </param>
    /// <param name="args">
    /// The constraint arguments extracted from the segment, or <see langword="null"/>
    /// if none were specified.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the input was successfully parsed as a parameter
    /// segment; otherwise <see langword="false"/>.
    /// </returns>
    private static bool TryParseSegment(string input, out string name, out string key, out string? args)
    {
        name = string.Empty;
        key  = "text";
        args = null;

        if (string.IsNullOrEmpty(input) || input.Length < 3
            || input[0] != '{' || input[input.Length - 1] != '}')
            return false;

        var inner = input.Substring(1, input.Length - 2);

        var colonIndex = inner.IndexOf(':');
        if (colonIndex < 0)
        {
            name = inner;
            return !string.IsNullOrWhiteSpace(name);
        }

        name = inner.Substring(0, colonIndex);
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var parenIndex = inner.IndexOf('(', colonIndex + 1);
        if (parenIndex >= 0)
        {
            var closingParenIndex = inner.LastIndexOf(')');
            if (closingParenIndex < parenIndex)
                return false;

            key  = inner.Substring(colonIndex + 1, parenIndex - colonIndex - 1);
            args = inner.Substring(parenIndex + 1, closingParenIndex - parenIndex - 1);
        }
        else
        {
            key = inner.Substring(colonIndex + 1);
        }

        return !string.IsNullOrWhiteSpace(key);
    }
}