using System.Threading.Channels;
using Grapevine.Abstractions;

namespace Grapevine;

/// <summary>
/// A minimal implementation of <see cref="IHttpPipeline"/> that processes
/// incoming <see cref="IHttpContext"/> instances using a single worker and a
/// terminal handler delegate.
/// </summary>
/// <remarks>
/// <para>
/// This implementation is intended as a foundation for the full middleware
/// pipeline. Currently it supports a single terminal handler with no
/// middleware chain. Middleware support will be added in a future iteration
/// via a <c>Use</c> method.
/// </para>
/// <para>
/// The pipeline processes one context at a time on a single worker task.
/// Concurrency support will be added when the full pipeline is implemented.
/// </para>
/// </remarks>
public class HttpPipeline : IHttpPipeline
{
    internal static readonly string LogHandlerError = "An unhandled exception occurred while processing a request.";

    private readonly Func<IHttpContext, Task> _handler;
    private readonly IHttpLogger _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="HttpPipeline"/> with the
    /// specified terminal handler and no logging.
    /// </summary>
    /// <param name="handler">
    /// The terminal handler invoked for each incoming <see cref="IHttpContext"/>.
    /// The handler is responsible for sending a response before returning.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public HttpPipeline(Func<IHttpContext, Task> handler) : this(handler, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="HttpPipeline"/> with the
    /// specified terminal handler and logger.
    /// </summary>
    /// <param name="handler">
    /// The terminal handler invoked for each incoming <see cref="IHttpContext"/>.
    /// The handler is responsible for sending a response before returning.
    /// </param>
    /// <param name="logger">
    /// The logger to use for unhandled handler exceptions. Pass
    /// <see langword="null"/> to disable logging.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public HttpPipeline(Func<IHttpContext, Task> handler, IHttpLogger? logger)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _logger  = logger ?? HttpNullLogger.Instance;
    }

    /// <inheritdoc/>
    public async Task RunAsync(IHttpContext context)
    {
        try
        {
            await _handler(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // If the handler throws and has not yet responded, abort the
            // context so the client receives a clean connection close rather
            // than hanging indefinitely.
            if (!context.WasRespondedTo)
                context.Response.Abort();

            _logger.LogError(LogHandlerError, ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task ListenAsync(IContextQueue queue, CancellationToken cancellationToken = default)
    {
        if (queue == null) throw new ArgumentNullException(nameof(queue));

        while (true)
        {
            IHttpContext context;

            try
            {
                context = await queue.DequeueAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Cancellation token was cancelled, server is stopping.
                break;
            }
            catch (ChannelClosedException)
            {
                // Queue has been completed, server has stopped writing.
                break;
            }

            try
            {
                await RunAsync(context).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // RunAsync has already logged the exception. Swallow here so
                // that a single failing request does not stop the pipeline loop.
            }
            finally
            {
                // Always dispose the context to release the underlying
                // HttpListener resources regardless of handler outcome.
                context.Dispose();
            }
        }
    }
}