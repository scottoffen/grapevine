namespace Grapevine;

/// <summary>
/// Defines the severity levels used by <see cref="IHttpLogger"/>.
/// </summary>
/// <remarks>
/// <para>
/// Integer values match those defined by
/// <c>Microsoft.Extensions.Logging.LogLevel</c>, so a cast is sufficient
/// to bridge between the two enumerations without a lookup table.
/// </para>
/// </remarks>
public enum LogLevel
{
    /// <summary>
    /// Logs that contain the most detailed messages. May contain sensitive
    /// application data. Disabled by default and should not be enabled in
    /// production.
    /// </summary>
    Trace = 0,

    /// <summary>
    /// Logs that are used for interactive investigation during development.
    /// These logs should primarily contain information useful for debugging
    /// and have no long-term value.
    /// </summary>
    Debug = 1,

    /// <summary>
    /// Logs that track the general flow of the application. These logs
    /// should have long-term value.
    /// </summary>
    Information = 2,

    /// <summary>
    /// Logs that highlight an abnormal or unexpected event in the application
    /// flow, but do not otherwise cause the application execution to stop.
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Logs that highlight when the current flow of execution is stopped due
    /// to a failure. These should indicate a failure in the current activity,
    /// not an application-wide failure.
    /// </summary>
    Error = 4,

    /// <summary>
    /// Logs that describe an unrecoverable application or system crash, or a
    /// catastrophic failure that requires immediate attention.
    /// </summary>
    Critical = 5,

    /// <summary>
    /// Not used for writing log messages. Specifies that a logging category
    /// should write no messages.
    /// </summary>
    None = 6,
}