namespace Grapevine;

/// <summary>
/// Represents the query string parameters of an HTTP request as a read-only
/// collection of name-value pairs.
/// </summary>
/// <remarks>
/// <para>
/// Query parameter names are matched case-insensitively. Multiple values for
/// the same key are supported (e.g. <c>?tag=a&amp;tag=b</c>).
/// </para>
/// <para>
/// The indexer and <see cref="IStringParams.TryGetValue(string, out string?)"/>
/// return only the first value for a given key. Use
/// <see cref="IStringParams.GetValues"/> or
/// <see cref="IStringParams.TryGetValues"/> to retrieve all values.
/// </para>
/// <para>
/// Strongly-typed value retrieval is available via the
/// <see cref="StringParamsExtensions"/> extension methods, which delegate to
/// the <see cref="Grapevine.Abstractions.ConverterRegistry"/>.
/// </para>
/// </remarks>
public interface IQueryParams : IStringParams
{
}