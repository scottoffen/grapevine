using System.Globalization;

namespace Grapevine.Abstractions;

/// <summary>
/// An <see cref="IHttpLogger"/> implementation that writes formatted log
/// entries to <see cref="Console.Out"/>.
/// </summary>
/// <remarks>
/// <para>
/// Each log entry is written in the format
/// <c>[timestamp] [Level] Message</c>, or
/// <c>[timestamp] [Level] Category: Message</c> when a category name is
/// provided. For <see cref="LogLevel.Error"/> and
/// <see cref="LogLevel.Critical"/> entries that include an exception, the
/// full exception detail is written on a separate line. For all other levels
/// the exception type and message are appended inline.
/// </para>
/// <para>
/// Override <see cref="FormatEntry"/> to customise the output format, or
/// <see cref="WriteEntry"/> to redirect output (e.g. to
/// <see cref="Console.Error"/> for warnings and above).
/// </para>
/// </remarks>
public class HttpConsoleLogger : IHttpLogger
{
    internal static readonly string TimestampFormat = "yyyy-MM-dd HH:mm:ss";

    private readonly string? _category;

    /// <summary>
    /// Gets the minimum <see cref="LogLevel"/> this logger will write.
    /// Entries below this level are silently discarded.
    /// </summary>
    public LogLevel MinimumLevel { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="HttpConsoleLogger"/> with
    /// an optional category name and minimum log level.
    /// </summary>
    /// <param name="category">
    /// An optional category name prefixed to each log entry, e.g. the name
    /// of the route handler class. Pass <see langword="null"/> or omit to
    /// write entries without a category prefix.
    /// </param>
    /// <param name="minimumLevel">
    /// The minimum <see cref="LogLevel"/> to write. Defaults to
    /// <see cref="LogLevel.Information"/>.
    /// </param>
    public HttpConsoleLogger(string? category = null, LogLevel minimumLevel = LogLevel.Information)
    {
        _category = string.IsNullOrWhiteSpace(category) ? null : category!.Trim();
        MinimumLevel = minimumLevel;
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel level) => level != LogLevel.None && level >= MinimumLevel;

    /// <inheritdoc/>
    /// <remarks>
    /// Calls <see cref="FormatEntry"/> to build the output string, then
    /// passes it to <see cref="WriteEntry"/>. Entries below
    /// <see cref="MinimumLevel"/> are silently discarded.
    /// </remarks>
    public void Log(LogLevel level, string message, Exception? exception = null)
    {
        if (!IsEnabled(level)) return;
        WriteEntry(FormatEntry(level, message, exception));
    }

    /// <summary>
    /// Builds the formatted string for a log entry.
    /// </summary>
    /// <remarks>
    /// Override to customise the output format. The return value is passed
    /// directly to <see cref="WriteEntry"/>. The default format is
    /// <c>[timestamp] [Level] Message</c>, or
    /// <c>[timestamp] [Level] Category: Message</c> when a category name was
    /// provided at construction. For <see cref="LogLevel.Error"/> and
    /// <see cref="LogLevel.Critical"/> entries the full exception detail is
    /// appended on a second line; for all other levels only the exception
    /// type and message are appended inline.
    /// </remarks>
    /// <param name="level">The severity of the log entry.</param>
    /// <param name="message">The log message.</param>
    /// <param name="exception">The associated exception, or <see langword="null"/>.</param>
    /// <returns>The fully formatted string to be written.</returns>
    protected virtual string FormatEntry(LogLevel level, string message, Exception? exception)
    {
        var timestamp  = DateTime.UtcNow.ToString(TimestampFormat, CultureInfo.InvariantCulture);
        var levelLabel = LevelLabel(level);
        var prefix     = _category != null ? $"{_category}: " : string.Empty;

        // For Trace/Debug/Information/Warning, append the exception inline
        // to keep the output on a single line where possible.
        // For Error/Critical, the stack trace warrants a dedicated line.
        if (exception != null && level < LogLevel.Error)
            return $"[{timestamp}] [{levelLabel}] {prefix}{message} -- {exception.GetType().Name}: {exception.Message}";

        var primaryLine = $"[{timestamp}] [{levelLabel}] {prefix}{message}";

        if (exception != null)
            return $"{primaryLine}{Environment.NewLine}{exception}";

        return primaryLine;
    }

    /// <summary>
    /// Writes a fully formatted log entry string to the output destination.
    /// </summary>
    /// <remarks>
    /// Override to redirect output, for example writing warnings and errors
    /// to <see cref="Console.Error"/> instead of <see cref="Console.Out"/>.
    /// The default implementation always writes to <see cref="Console.Out"/>.
    /// </remarks>
    /// <param name="formattedEntry">The fully formatted string produced by <see cref="FormatEntry"/>.</param>
    protected virtual void WriteEntry(string formattedEntry)
    {
        Console.Out.WriteLine(formattedEntry);
    }

    /// <summary>
    /// Returns the fixed-width label string for the specified log level.
    /// </summary>
    private static string LevelLabel(LogLevel level) => level switch
    {
        LogLevel.Trace       => "Trace   ",
        LogLevel.Debug       => "Debug   ",
        LogLevel.Information => "Info    ",
        LogLevel.Warning     => "Warning ",
        LogLevel.Error       => "Error   ",
        LogLevel.Critical    => "Critical",
        _                    => "Unknown ",
    };
}