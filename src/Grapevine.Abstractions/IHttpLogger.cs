namespace Grapevine;

/// <summary>
/// Defines a simple logger for use within the Grapevine request pipeline,
/// middleware, and route handlers.
/// </summary>
public interface IHttpLogger
{
    /// <summary>
    /// Determines whether the specified log level is enabled for this logger.
    /// </summary>
    /// <param name="level">The log level to check.</param>
    /// <returns>
    /// <see langword="true"/> if the log level is enabled; otherwise
    /// <see langword="false"/>.
    /// </returns>
    bool IsEnabled(LogLevel level);

    /// <summary>
    /// Writes a log entry at the specified level.
    /// </summary>
    /// <param name="level">The severity of the log entry.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">
    /// An optional exception associated with the log entry.
    /// </param>
    void Log(LogLevel level, string message, Exception? exception = null);
}