using Grapevine.Abstractions;

namespace Grapevine;

/// <summary>
/// Represents a single middleware component in the request processing pipeline.
/// </summary>
/// <param name="context">The current HTTP context.</param>
/// <param name="next">
/// A delegate that invokes the next middleware component in the pipeline.
/// Middleware must call <paramref name="next"/> to pass control forward, or
/// short-circuit by not calling it, for example after sending a response.
/// </param>
/// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
public delegate Task MiddlewareDelegate(IHttpContext context, Func<Task> next);

/// <summary>
/// Represents a request processing pipeline that reads incoming
/// <see cref="IHttpContext"/> instances from an <see cref="IContextQueue"/> and
/// processes each one through a chain of middleware components.
/// </summary>
/// <remarks>
/// <para>
/// The pipeline drives the request processing loop. Call
/// <see cref="ListenAsync"/> to begin pulling contexts from the queue and
/// processing them. The loop runs until the queue is completed or the
/// <see cref="CancellationToken"/> is cancelled, which typically happens when
/// the server stops.
/// </para>
/// <para>
/// Middleware components are added via <c>Use</c> and are invoked in
/// registration order for each request. Each component may short-circuit the
/// pipeline by not calling <c>next</c>, or pass control forward by calling it.
/// </para>
/// </remarks>
public interface IHttpPipeline
{
    /// <summary>
    /// Processes a single <see cref="IHttpContext"/> through the pipeline.
    /// </summary>
    /// <param name="context">The HTTP context to process.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task RunAsync(IHttpContext context);

    /// <summary>
    /// Starts the pipeline loop, continuously dequeuing and processing
    /// <see cref="IHttpContext"/> instances from the specified
    /// <see cref="IContextQueue"/> until the queue is completed or the
    /// <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    /// <param name="queue">The queue to read incoming contexts from.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to stop the pipeline loop. Typically the
    /// server's lifetime token.
    /// </param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous loop operation.</returns>
    Task ListenAsync(IContextQueue queue, CancellationToken cancellationToken = default);
}