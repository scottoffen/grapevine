namespace Grapevine;

/// <summary>
/// Represents the route parameters extracted from a matched URL path as a
/// read-only collection of name-value pairs.
/// </summary>
/// <remarks>
/// <para>
/// Route parameter names correspond to named capture groups in the route's
/// regular expression pattern. Each name appears at most once, since capture
/// group names within a single regex are unique.
/// </para>
/// <para>
/// Key lookup is case-insensitive. Since each key appears at most once,
/// <see cref="IStringParams.GetValues"/> will always return a list of zero
/// or one elements.
/// </para>
/// <para>
/// Strongly-typed value retrieval is available via the
/// <see cref="StringParamsExtensions"/> extension methods, which delegate to
/// the <see cref="Grapevine.Abstractions.ConverterRegistry"/>.
/// </para>
/// </remarks>
public interface IRouteParams : IStringParams
{
}