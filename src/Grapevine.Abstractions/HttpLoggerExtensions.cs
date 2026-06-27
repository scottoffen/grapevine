namespace Grapevine;

/// <summary>
/// Provides convenience extension methods for <see cref="IHttpLogger"/> that
/// correspond to each <see cref="LogLevel"/> value.
/// </summary>
/// <remarks>
/// Each level has two overloads: one that accepts a message only, and one
/// that accepts a message and an associated <see cref="Exception"/>. Both
/// overloads check <see cref="IHttpLogger.IsEnabled"/> before calling
/// <see cref="IHttpLogger.Log"/> to avoid unnecessary string allocations when
/// the level is disabled.
/// </remarks>
public static class HttpLoggerExtensions
{
    /// <summary>
    /// Logs a message at the <see cref="LogLevel.Trace"/> level.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="message">The message to log.</param>
    public static void LogTrace(this IHttpLogger logger, string message)
    {
        if (logger.IsEnabled(LogLevel.Trace))
            logger.Log(LogLevel.Trace, message);
    }

    /// <summary>
    /// Logs a message and associated exception at the <see cref="LogLevel.Trace"/> level.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception associated with the log entry.</param>
    public static void LogTrace(this IHttpLogger logger, string message, Exception exception)
    {
        if (logger.IsEnabled(LogLevel.Trace))
            logger.Log(LogLevel.Trace, message, exception);
    }

    /// <summary>
    /// Logs a message at the <see cref="LogLevel.Debug"/> level.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="message">The message to log.</param>
    public static void LogDebug(this IHttpLogger logger, string message)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.Log(LogLevel.Debug, message);
    }

    /// <summary>
    /// Logs a message and associated exception at the <see cref="LogLevel.Debug"/> level.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception associated with the log entry.</param>
    public static void LogDebug(this IHttpLogger logger, string message, Exception exception)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.Log(LogLevel.Debug, message, exception);
    }

    /// <summary>
    /// Logs a message at the <see cref="LogLevel.Information"/> level.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="message">The message to log.</param>
    public static void LogInformation(this IHttpLogger logger, string message)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.Log(LogLevel.Information, message);
    }

    /// <summary>
    /// Logs a message and associated exception at the <see cref="LogLevel.Information"/> level.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception associated with the log entry.</param>
    public static void LogInformation(this IHttpLogger logger, string message, Exception exception)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.Log(LogLevel.Information, message, exception);
    }

    /// <summary>
    /// Logs a message at the <see cref="LogLevel.Warning"/> level.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="message">The message to log.</param>
    public static void LogWarning(this IHttpLogger logger, string message)
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.Log(LogLevel.Warning, message);
    }

    /// <summary>
    /// Logs a message and associated exception at the <see cref="LogLevel.Warning"/> level.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception associated with the log entry.</param>
    public static void LogWarning(this IHttpLogger logger, string message, Exception exception)
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.Log(LogLevel.Warning, message, exception);
    }

    /// <summary>
    /// Logs a message at the <see cref="LogLevel.Error"/> level.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="message">The message to log.</param>
    public static void LogError(this IHttpLogger logger, string message)
    {
        if (logger.IsEnabled(LogLevel.Error))
            logger.Log(LogLevel.Error, message);
    }

    /// <summary>
    /// Logs a message and associated exception at the <see cref="LogLevel.Error"/> level.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception associated with the log entry.</param>
    public static void LogError(this IHttpLogger logger, string message, Exception exception)
    {
        if (logger.IsEnabled(LogLevel.Error))
            logger.Log(LogLevel.Error, message, exception);
    }

    /// <summary>
    /// Logs a message at the <see cref="LogLevel.Critical"/> level.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="message">The message to log.</param>
    public static void LogCritical(this IHttpLogger logger, string message)
    {
        if (logger.IsEnabled(LogLevel.Critical))
            logger.Log(LogLevel.Critical, message);
    }

    /// <summary>
    /// Logs a message and associated exception at the <see cref="LogLevel.Critical"/> level.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception associated with the log entry.</param>
    public static void LogCritical(this IHttpLogger logger, string message, Exception exception)
    {
        if (logger.IsEnabled(LogLevel.Critical))
            logger.Log(LogLevel.Critical, message, exception);
    }
}