using System.Security.Claims;

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
/// requests. Pipeline components should set <see cref="WasRespondedTo"/> to
/// <see langword="true"/> once a response has been sent to signal to the remaining
/// pipeline that no further response should be written.
/// </para>
/// </remarks>
public interface IHttpContext
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
    /// Gets the authenticated user associated with this request, or
    /// <see langword="null"/> if the request is unauthenticated.
    /// </summary>
    /// <remarks>
    /// This property is populated by authentication middleware earlier in the
    /// pipeline. Route handlers and other components should treat a
    /// <see langword="null"/> value as an unauthenticated request.
    /// </remarks>
    ClaimsPrincipal? User { get; init; }

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
    /// Gets or sets a value indicating whether a response has been sent for this
    /// request.
    /// </summary>
    /// <remarks>
    /// Pipeline components should check this value before writing a response, and
    /// set it to <see langword="true"/> after doing so. This prevents multiple
    /// pipeline stages from attempting to write conflicting responses to the same
    /// request.
    /// </remarks>
    bool WasRespondedTo { get; set; }
}