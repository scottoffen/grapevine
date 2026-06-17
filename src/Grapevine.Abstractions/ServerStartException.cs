using System.Diagnostics;

namespace Grapevine;

/// <summary>
/// The exception thrown when an <see cref="IHttpServer"/> fails to start,
/// typically because a configured prefix is unavailable due to a port conflict
/// or insufficient permissions.
/// </summary>
/// <remarks>
/// <para>
/// Inspect <see cref="Prefixes"/> to identify which prefixes were being
/// registered at the time of failure. The underlying system exception is
/// available via <see cref="Exception.InnerException"/>, and its error code
/// is available via <see cref="ErrorCode"/> for advanced diagnostics.
/// </para>
/// <para>
/// Common causes and their typical error codes on Windows:
/// </para>
/// <para>
/// <list type="bullet">
/// <item>Error code 5 (Access Denied): the process does not have permission
/// to bind to the configured prefix. On Windows, run the process as
/// administrator or use <c>netsh http add urlacl</c> to grant permission.
/// On Linux or macOS, binding to ports below 1024 requires elevated
/// privileges.</item>
/// <item>Error code 183 (Already Exists): another process or
/// <see cref="System.Net.HttpListener"/> instance is already listening on
/// one of the configured prefixes. Ensure no other server is bound to the
/// same port.</item>
/// <item>Error code 32 (Sharing Violation): the port is in use by another
/// application. Use a different port or stop the conflicting process.</item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay("Prefixes = {Prefixes.Count}, Message = {Message}")]
public class ServerStartException : Exception
{
    /// <summary>
    /// The error message template used when the failure is due to access
    /// being denied.
    /// </summary>
    internal static readonly string AccessDeniedMessage =
        "Access was denied when starting the server on the following prefix(es): {0}. " +
        "Ensure the process has permission to bind to these prefixes. On Windows, run as " +
        "administrator or use 'netsh http add urlacl' to grant permission. On Linux or " +
        "macOS, binding to ports below 1024 requires elevated privileges.";

    /// <summary>
    /// The error message template used when a prefix is already registered
    /// by another listener.
    /// </summary>
    internal static readonly string AlreadyExistsMessage =
        "The server could not start because one or more of the following prefix(es) are " +
        "already in use: {0}. Ensure no other server or HttpListener instance is bound to " +
        "the same port.";

    /// <summary>
    /// The error message template used when a port is already in use by
    /// another process.
    /// </summary>
    internal static readonly string SharingViolationMessage =
        "The server could not start because one or more of the following prefix(es) are " +
        "bound to a port already in use by another process: {0}. Use a different port or " +
        "stop the conflicting process.";

    /// <summary>
    /// The error message template used when the failure reason is unknown.
    /// </summary>
    internal static readonly string UnknownErrorMessage =
        "The server failed to start on the following prefix(es): {0}. " +
        "See the inner exception for details (error code: {1}).";

    /// <summary>
    /// Win32 error code indicating access was denied.
    /// </summary>
    private const int ErrorCodeAccessDenied = 5;

    /// <summary>
    /// Win32 error code indicating the prefix is already registered.
    /// </summary>
    private const int ErrorCodeAlreadyExists = 183;

    /// <summary>
    /// Win32 error code indicating a port sharing violation.
    /// </summary>
    private const int ErrorCodeSharingViolation = 32;

    /// <summary>
    /// Gets the list of URI prefixes that were being registered when the
    /// failure occurred.
    /// </summary>
    public IReadOnlyList<string> Prefixes { get; }

    /// <summary>
    /// Gets the Win32 error code from the underlying system exception, or
    /// <c>-1</c> if the inner exception does not carry an error code.
    /// </summary>
    public int ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ServerStartException"/> from
    /// the specified prefixes and underlying exception.
    /// </summary>
    /// <param name="prefixes">
    /// The URI prefixes that were being registered when the failure occurred.
    /// </param>
    /// <param name="innerException">
    /// The underlying system exception that caused the startup failure.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="prefixes"/> or <paramref name="innerException"/>
    /// is <see langword="null"/>.
    /// </exception>
    public ServerStartException(IEnumerable<string> prefixes, Exception innerException)
        : base(BuildMessage(prefixes, innerException), innerException)
    {
        Prefixes = prefixes.ToList().AsReadOnly();

        ErrorCode = innerException is System.Net.HttpListenerException hle
            ? hle.ErrorCode
            : -1;
    }

    /// <summary>
    /// Builds a diagnostic message from the prefix list and the underlying
    /// exception, distinguishing between known error codes where possible.
    /// </summary>
    /// <param name="prefixes">The prefixes that were being registered.</param>
    /// <param name="innerException">The underlying exception.</param>
    /// <returns>A human-readable message describing the failure.</returns>
    private static string BuildMessage(IEnumerable<string> prefixes, Exception innerException)
    {
        var prefixList = string.Join(", ", prefixes);

        if (innerException is System.Net.HttpListenerException hle)
        {
            return hle.ErrorCode switch
            {
                ErrorCodeAccessDenied => string.Format(AccessDeniedMessage, prefixList),
                ErrorCodeAlreadyExists => string.Format(AlreadyExistsMessage, prefixList),
                ErrorCodeSharingViolation => string.Format(SharingViolationMessage, prefixList),
                _ => string.Format(UnknownErrorMessage, prefixList, hle.ErrorCode),
            };
        }

        return string.Format(UnknownErrorMessage, prefixList, -1);
    }
}