using System.Diagnostics.CodeAnalysis;

namespace Grapevine.Abstractions;

/// <summary>
/// Provides base configuration options for an <see cref="IHttpServer"/>
/// implementation.
/// </summary>
/// <remarks>
/// <para>
/// This class contains configuration that is common to all
/// <see cref="IHttpServer"/> implementations regardless of the
/// underlying transport. Implementation-specific options should be defined
/// in a derived class.
/// </para>
/// </remarks>
[ExcludeFromCodeCoverage]
public class HttpServerOptions
{
    /// <summary>
    /// The default channel capacity used when <see cref="ChannelCapacity"/> is
    /// not explicitly set.
    /// </summary>
    public static readonly int DefaultChannelCapacity = 512;

    /// <summary>
    /// Gets or sets the URI prefixes to pre-populate on the server at
    /// construction time.
    /// </summary>
    /// <remarks>
    /// Each prefix must be a valid prefix beginning with <c>http://</c> or
    /// <c>https://</c> and ending with a forward slash. Invalid prefixes are
    /// silently skipped during construction; use
    /// <see cref="PrefixCollection.IsValidPrefix"/> to pre-validate
    /// if needed.
    /// </remarks>
    public IEnumerable<string> Prefixes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the maximum number of <see cref="IHttpContext"/>
    /// instances that can be queued in the request channel before the server
    /// applies backpressure to incoming requests.
    /// </summary>
    /// <remarks>
    /// A lower value reduces memory usage under load at the cost of increased
    /// request rejection. A higher value allows more requests to queue but
    /// increases memory pressure. Defaults to <see cref="DefaultChannelCapacity"/>.
    /// </remarks>
    public int ChannelCapacity { get; set; } = DefaultChannelCapacity;

    /// <summary>
    /// Gets or sets a value indicating whether write exceptions caused by
    /// client disconnects are suppressed by the underlying transport.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// When <see langword="true"/>, client disconnect exceptions during response
    /// writes are suppressed rather than thrown. The Grapevine pipeline detects
    /// disconnects via <see cref="IHttpContext.Abort"/> rather than
    /// relying on these exceptions, so this should remain <see langword="true"/>
    /// in most cases.
    /// </remarks>
    public bool IgnoreWriteExceptions { get; set; } = true;
}