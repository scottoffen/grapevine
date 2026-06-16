namespace Grapevine;

/// <summary>
/// A write-only stream wrapper that ensures response headers are committed
/// exactly once before the first byte of response body data is written to the
/// underlying stream.
/// </summary>
/// <remarks>
/// <para>
/// When a handler writes to <see cref="IHttpResponse.OutputStream"/>, the
/// response headers must be sent to the client before any body data. This
/// stream invokes a provided callback on the first write operation, then
/// delegates all subsequent writes directly to the inner stream with no
/// additional overhead.
/// </para>
/// <para>
/// The callback is invoked at most once, regardless of how many write
/// operations are performed. It is invoked synchronously on the calling
/// thread before the first write is delegated to the inner stream.
/// </para>
/// <para>
/// This stream is write-only. All read and seek operations throw
/// <see cref="NotSupportedException"/>.
/// </para>
/// </remarks>
public class ResponseStream : Stream
{
    private readonly Stream _inner;
    private readonly Action _onFirstWrite;
    private bool _committed;

    /// <summary>
    /// Initializes a new instance of <see cref="ResponseStream"/> wrapping
    /// the specified stream.
    /// </summary>
    /// <param name="inner">The underlying stream to write to.</param>
    /// <param name="onFirstWrite">
    /// A callback invoked exactly once before the first write operation.
    /// Typically used to commit response headers before body data is sent.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="inner"/> or <paramref name="onFirstWrite"/>
    /// is <see langword="null"/>.
    /// </exception>
    public ResponseStream(Stream inner, Action onFirstWrite)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _onFirstWrite = onFirstWrite ?? throw new ArgumentNullException(nameof(onFirstWrite));
    }

    /// <inheritdoc/>
    public override bool CanRead => false;

    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <inheritdoc/>
    public override bool CanWrite => true;

    /// <summary>
    /// Not supported. <see cref="ResponseStream"/> is write-only.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public override long Length =>
        throw new NotSupportedException("ResponseStream is write-only and does not support Length.");

    /// <summary>
    /// Not supported. <see cref="ResponseStream"/> is write-only.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public override long Position
    {
        get => throw new NotSupportedException("ResponseStream is write-only and does not support Position.");
        set => throw new NotSupportedException("ResponseStream is write-only and does not support Position.");
    }

    /// <inheritdoc/>
    public override void Flush() => _inner.Flush();

    /// <inheritdoc/>
    public override Task FlushAsync(CancellationToken cancellationToken) =>
        _inner.FlushAsync(cancellationToken);

    /// <summary>
    /// Not supported. <see cref="ResponseStream"/> is write-only.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public override int Read(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException("ResponseStream is write-only and does not support Read.");

    /// <summary>
    /// Not supported. <see cref="ResponseStream"/> is write-only.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        throw new NotSupportedException("ResponseStream is write-only and does not support ReadAsync.");

#if NET6_0_OR_GREATER
    /// <summary>
    /// Not supported. <see cref="ResponseStream"/> is write-only.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("ResponseStream is write-only and does not support ReadAsync.");
#endif

    /// <summary>
    /// Not supported. <see cref="ResponseStream"/> is write-only.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException("ResponseStream is write-only and does not support Seek.");

    /// <summary>
    /// Not supported. <see cref="ResponseStream"/> is write-only.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public override void SetLength(long value) =>
        throw new NotSupportedException("ResponseStream is write-only and does not support SetLength.");

    /// <inheritdoc/>
    /// <remarks>
    /// Invokes the commit callback on the first call, then delegates to the
    /// inner stream. The callback is never invoked more than once.
    /// </remarks>
    public override void Write(byte[] buffer, int offset, int count)
    {
        EnsureCommitted();
        _inner.Write(buffer, offset, count);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Invokes the commit callback on the first call, then delegates to the
    /// inner stream. The callback is never invoked more than once.
    /// </remarks>
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        EnsureCommitted();
        await _inner.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
    }

#if NET6_0_OR_GREATER
    /// <inheritdoc/>
    /// <remarks>
    /// Invokes the commit callback on the first call, then delegates to the
    /// inner stream. The callback is never invoked more than once.
    /// </remarks>
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        EnsureCommitted();
        await _inner.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
    }
#endif

    /// <summary>
    /// Invokes the commit callback if it has not yet been called, then marks
    /// the stream as committed so subsequent writes skip the callback entirely.
    /// </summary>
    /// <remarks>
    /// <see cref="_committed"/> is set to <see langword="true"/> before invoking
    /// the callback so that a throwing callback does not cause the callback to
    /// be retried on subsequent writes.
    /// </remarks>
    private void EnsureCommitted()
    {
        if (_committed) return;
        _committed = true;
        _onFirstWrite();
    }
}