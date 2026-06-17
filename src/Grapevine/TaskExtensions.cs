#if !NET6_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;

namespace Grapevine;

/// <summary>
/// Provides extension methods for <see cref="Task"/> to support cancellable
/// awaiting on target frameworks that do not natively support
/// <c>Task.WaitAsync(CancellationToken)</c>.
/// </summary>
internal static class TaskExtensions
{
    /// <summary>
    /// Waits for the task to complete, throwing <see cref="OperationCanceledException"/>
    /// if the <paramref name="cancellationToken"/> is cancelled before the task finishes.
    /// </summary>
    /// <param name="task">The task to wait for.</param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation. If cancelled before the task
    /// completes, <see cref="OperationCanceledException"/> is thrown.
    /// </param>
    /// <returns>A task that represents the asynchronous wait operation.</returns>
    internal static async Task WaitAsync(this Task task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();

        using var registration = cancellationToken.Register(
            state => ((TaskCompletionSource<bool>)state!).TrySetCanceled(),
            tcs
        );

        // Race the original task against the cancellation signal.
        await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        // Await the original task to propagate any exceptions it may have thrown.
        await task.ConfigureAwait(false);
    }

    /// <summary>
    /// Waits for the task to complete, throwing <see cref="OperationCanceledException"/>
    /// if the <paramref name="cancellationToken"/> is cancelled before the task finishes.
    /// </summary>
    /// <typeparam name="T">The result type of the task.</typeparam>
    /// <param name="task">The task to wait for.</param>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation. If cancelled before the task
    /// completes, <see cref="OperationCanceledException"/> is thrown.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous wait operation, yielding the
    /// result of the original task on completion.
    /// </returns>
    internal static async Task<T> WaitAsync<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<T>();

        using var registration = cancellationToken.Register(
            state => ((TaskCompletionSource<T>)state!).TrySetCanceled(),
            tcs
        );

        // Race the original task against the cancellation signal.
        await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        // Await the original task to propagate any exceptions it may have thrown.
        return await task.ConfigureAwait(false);
    }
}
#endif