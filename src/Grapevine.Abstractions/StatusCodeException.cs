using System.Diagnostics.CodeAnalysis;

namespace Grapevine;

/// <summary>
/// Represents an exception that carries an HTTP status code, used to signal
/// that a request should be terminated with a specific HTTP response status.
/// </summary>
[ExcludeFromCodeCoverage]
public class StatusCodeException : Exception
{
    /// <summary>
    /// Gets the HTTP status code associated with this exception.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="StatusCodeException"/> with
    /// the specified HTTP status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to associate with this exception.</param>
    public StatusCodeException(HttpStatusCode statusCode) : base()
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="StatusCodeException"/> with
    /// the specified HTTP status code and error message.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to associate with this exception.</param>
    /// <param name="message">A message that describes the error.</param>
    public StatusCodeException(HttpStatusCode statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="StatusCodeException"/> with
    /// the specified HTTP status code, error message, and a reference to the
    /// inner exception that caused this exception.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to associate with this exception.</param>
    /// <param name="message">A message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of this exception.</param>
    public StatusCodeException(HttpStatusCode statusCode, string message, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
