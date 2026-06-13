namespace Grapevine;

/// <summary>
/// Represents an incoming HTTP request.
/// </summary>
/// <remarks>
/// This interface is a stub. Additional members will be added as the request
/// handling infrastructure is built out.
/// </remarks>
public interface IHttpRequest
{
    /// <summary>
    /// Gets the HTTP method of the request.
    /// </summary>
    HttpMethod HttpMethod { get; }

    /// <summary>
    /// Gets the path component of the request URL, used for route matching.
    /// </summary>
    string Endpoint { get; }
}