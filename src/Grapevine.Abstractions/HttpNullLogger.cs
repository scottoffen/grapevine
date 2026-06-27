using System.Diagnostics.CodeAnalysis;

namespace Grapevine.Abstractions;

/// <summary>
/// An <see cref="IHttpLogger"/> implementation that discards all log messages.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="HttpNullLogger"/> is the default logger used by Grapevine when
/// no logger is configured. It incurs no allocations and no I/O on any call.
/// </para>
/// <para>
/// Use <see cref="Instance"/> to obtain the shared singleton rather than
/// constructing a new instance.
/// </para>
/// </remarks>
[ExcludeFromCodeCoverage]
public sealed class HttpNullLogger : IHttpLogger
{
    /// <summary>
    /// Gets the shared singleton instance of <see cref="HttpNullLogger"/>.
    /// </summary>
    public static readonly HttpNullLogger Instance = new HttpNullLogger();

    private HttpNullLogger()
    {
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Always returns <see langword="false"/>. All log levels are disabled.
    /// </remarks>
    public bool IsEnabled(LogLevel level) => false;

    /// <inheritdoc/>
    /// <remarks>
    /// This method is a no-op. All messages are silently discarded.
    /// </remarks>
    public void Log(LogLevel level, string message, Exception? exception = null)
    {
    }
}