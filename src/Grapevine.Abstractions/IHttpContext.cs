using System.Security.Claims;
using System.Threading;

namespace Grapevine;

/// <summary>
/// Represents the context of a single HTTP request and its associated response.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IHttpContext"/> is the primary object passed through the Grapevine
/// request pipeline. It encapsulates the incoming <see cref="Request"/>, the
/// outgoing <see cref="Response"/>, the authenticated <see cref="User"/>, and any
/// per-request ambient state stored in <see cref="Locals"/>.
/// </para>
/// <para>
/// A new instance is created for each incoming request and is not shared across
/// requests. Pipeline components should check <see cref="WasRespondedTo"/> before
/// writing a response. To set this value, use <see cref="IHttpResponse.WasRespondedTo"/>
/// directly.
/// </para>
/// </remarks>
public interface IHttpContext : IDisposable
{
    /// <summary>
    /// Gets the unique identifier for this request. Defaults to a new GUID
    /// string, but may be set from an incoming correlation header such as
    /// <c>X-Request-Id</c> by the pipeline before handlers are invoked.
    /// </summary>
    string Id { get; init; }

    /// <summary>
    /// Gets the incoming HTTP request.
    /// </summary>
    IHttpRequest Request { get; init; }

    /// <summary>
    /// Gets the outgoing HTTP response.
    /// </summary>
    IHttpResponse Response { get; init; }

    /// <summary>
    /// Gets or sets the authenticated user associated with this request, or
    /// <see langword="null"/> if the request is unauthenticated.
    /// </summary>
    /// <remarks>
    /// This property is populated by authentication middleware earlier in the
    /// pipeline. Route handlers and other components should treat a
    /// <see langword="null"/> value as an unauthenticated request.
    /// </remarks>
    ClaimsPrincipal? User { get; set; }

    /// <summary>
    /// Gets the per-request ambient state bag for passing arbitrary data between
    /// pipeline components within the scope of this request.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Locals"/> to share data between middleware and route
    /// handlers without coupling them directly. Keys and values are untyped; use
    /// the typed accessor methods on <see cref="Grapevine.Locals"/> for safe
    /// retrieval.
    /// </remarks>
    Locals Locals { get; init; }

    /// <summary>
    /// Gets a <see cref="CancellationToken"/> that is triggered when the client
    /// disconnects or the request is otherwise aborted.
    /// </summary>
    /// <remarks>
    /// Pass this token to all async operations within a handler so that in-flight
    /// work can be cancelled cleanly when the client is no longer waiting for a
    /// response.
    /// </remarks>
    CancellationToken RequestAborted { get; init; }

    /// <summary>
    /// Gets a value indicating whether a response has been sent for this request.
    /// </summary>
    /// <remarks>
    /// This is a read-only view of <see cref="IHttpResponse.WasRespondedTo"/>.
    /// To set this value, use <see cref="IHttpResponse.WasRespondedTo"/> directly.
    /// </remarks>
    bool WasRespondedTo { get; }

    /// <summary>
    /// Cancels <see cref="RequestAborted"/>, signalling to any in-flight async
    /// operations that they should stop work for this request.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method only cancels the token. It does not close the connection, set
    /// <see cref="WasRespondedTo"/>, or dispose any resources. Cleanup remains
    /// the responsibility of the pipeline runner after it observes the cancellation.
    /// </para>
    /// <para>
    /// This is distinct from <see cref="IHttpResponse.Abort"/>, which closes the
    /// underlying connection at the HTTP level without sending a response.
    /// </para>
    /// </remarks>
    void Abort();
}