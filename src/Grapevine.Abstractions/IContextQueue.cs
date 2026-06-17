namespace Grapevine.Abstractions;

/// <summary>
/// Represents a queue of incoming HTTP request contexts to be processed by
/// the middleware pipeline.
/// </summary>
/// <remarks>
/// <para>
/// The server writes <see cref="IHttpContext"/> instances to the queue as
/// requests arrive. The middleware pipeline reads from the queue and processes
/// each context in turn.
/// </para>
/// <para>
/// Consumers of the pipeline should call <see cref="DequeueAsync"/> in a loop,
/// exiting when <see cref="OperationCanceledException"/> or
/// <see cref="System.Threading.Channels.ChannelClosedException"/> is thrown,
/// which signals that the server has stopped and no further contexts will be
/// written.
/// </para>
/// </remarks>
public interface IContextQueue
{
    /// <summary>
    /// Asynchronously dequeues the next <see cref="IHttpContext"/> from the
    /// queue, waiting until one is available or the operation is cancelled.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the wait. Typically the server's
    /// lifetime token, so the pipeline exits cleanly when the server stops.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> that completes with the next
    /// <see cref="IHttpContext"/> when one is available.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is cancelled.
    /// </exception>
    /// <exception cref="System.Threading.Channels.ChannelClosedException">
    /// Thrown when the queue has been completed and no further contexts will
    /// be written.
    /// </exception>
    ValueTask<IHttpContext> DequeueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to dequeue the next <see cref="IHttpContext"/> from the queue
    /// without waiting.
    /// </summary>
    /// <param name="context">
    /// When this method returns <see langword="true"/>, contains the dequeued
    /// <see cref="IHttpContext"/>; otherwise <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a context was available and dequeued;
    /// otherwise <see langword="false"/>.
    /// </returns>
    bool TryDequeue(out IHttpContext? context);
}