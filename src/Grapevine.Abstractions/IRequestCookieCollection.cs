namespace Grapevine.Abstractions;

/// <summary>
/// Represents the cookies received with an HTTP request as a read-only
/// collection of name-value string pairs.
/// </summary>
/// <remarks>
/// <para>
/// Request cookies carry only their name and value. All cookie attributes
/// (path, domain, expiry, flags) are response-only concepts defined in
/// <c>Set-Cookie</c> headers and are not present in incoming <c>Cookie</c>
/// request headers.
/// </para>
/// <para>
/// Implementations are populated once from the raw <c>Cookie</c> header value
/// and sealed before the route handler is invoked. Cookie name lookup is
/// case-insensitive.
/// </para>
/// </remarks>
public interface IRequestCookieCollection : IReadOnlyDictionary<string, string>
{
    /// <summary>
    /// Gets a value indicating whether the collection has been sealed.
    /// </summary>
    /// <remarks>
    /// Request cookie collections are always sealed after construction since
    /// they are populated once from the raw <c>Cookie</c> header and never
    /// mutated. This property will always return <see langword="true"/> on
    /// a fully constructed instance.
    /// </remarks>
    bool IsSealed { get; }
}