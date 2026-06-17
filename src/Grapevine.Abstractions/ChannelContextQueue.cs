using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace Grapevine.Abstractions;

/// <summary>
/// An <see cref="IContextQueue"/> implementation backed by a
/// <see cref="ChannelReader{T}"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChannelContextQueue : IContextQueue
{
    private readonly ChannelReader<IHttpContext> _reader;

    /// <summary>
    /// Initializes a new instance of <see cref="ChannelContextQueue"/> wrapping
    /// the specified <see cref="ChannelReader{T}"/>.
    /// </summary>
    /// <param name="reader">The channel reader to read contexts from.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="reader"/> is <see langword="null"/>.
    /// </exception>
    public ChannelContextQueue(ChannelReader<IHttpContext> reader)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    /// <inheritdoc/>
    public ValueTask<IHttpContext> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return _reader.ReadAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public bool TryDequeue([NotNullWhen(true)] out IHttpContext? context)
    {
        return _reader.TryRead(out context);
    }
}