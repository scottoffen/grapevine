using System.Diagnostics.CodeAnalysis;
using System.Net;
using Grapevine.Abstractions;

namespace Grapevine;

/// <summary>
/// Wraps an <see cref="HttpListenerRequest"/> and exposes it as an
/// <see cref="IHttpRequest"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class HttpRequest : IHttpRequest
{
    private readonly HttpListenerRequest _request;
    /// <summary>
    /// Initializes a new instance of <see cref="HttpRequest"/> wrapping the
    /// specified <see cref="HttpListenerRequest"/>.
    /// </summary>
    /// <param name="request">The underlying <see cref="HttpListenerRequest"/> to wrap.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <see langword="null"/>.
    /// </exception>
    public HttpRequest(HttpListenerRequest request)
    {
        _request = request ?? throw new ArgumentNullException(nameof(request));

        // Extract endpoint information eagerly so that ConnectionInfo is ready
        // before the first property access and the underlying IPEndPoint does
        // not need to be accessed on the hot path.
        LocalEndpoint = new ConnectionInfo(
            _request.LocalEndPoint.Address.ToString(),
            _request.LocalEndPoint.Port
        );

        RemoteEndpoint = new ConnectionInfo(
            _request.RemoteEndPoint.Address.ToString(),
            _request.RemoteEndPoint.Port
        );

        Method = _request.HttpMethod;
        Headers = BuildHeaders(_request);
        QueryString = QueryParams.Parse(_request.Url?.Query);
        Cookies = Headers.Cookies;
    }

    /// <summary>
    /// Populates a <see cref="RequestHeaderCollection"/> from the raw headers on
    /// the underlying <see cref="HttpListenerRequest"/>.
    /// </summary>
    /// <param name="request">The source request whose headers are copied.</param>
    /// <returns>A sealed <see cref="RequestHeaderCollection"/> populated with all request headers.</returns>
    private static RequestHeaderCollection BuildHeaders(HttpListenerRequest request)
    {
        var headers = new RequestHeaderCollection();
        var keys = request.Headers.AllKeys;

        for (var i = 0; i < keys.Length; i++)
        {
            var key = keys[i];
            if (key == null) continue;

            // GetValues returns string[]
            var values = request.Headers.GetValues(key);
            if (values == null || values.Length == 0) continue;

            headers.Add(key, values);
        }

        return headers;
    }

    /// <inheritdoc/>
    public HttpMethod Method { get; }

    /// <inheritdoc/>
    public Uri? Url => _request.Url;

    /// <inheritdoc/>
    public string? RawUrl => _request.RawUrl;

    /// <inheritdoc/>
    public string Path => _request.Url?.AbsolutePath ?? _request.RawUrl ?? string.Empty;

    /// <inheritdoc/>
    public IRequestHeaderCollection Headers { get; }

    /// <inheritdoc/>
    public IQueryParams QueryString { get; }

    /// <inheritdoc/>
    public IRequestCookieCollection Cookies { get; }

    /// <inheritdoc/>
    public Stream InputStream => _request.InputStream;

    /// <inheritdoc/>
    public bool IsAuthenticated => _request.IsAuthenticated;

    /// <inheritdoc/>
    public bool IsLocal => _request.IsLocal;

    /// <inheritdoc/>
    public bool IsSecureConnection => _request.IsSecureConnection;

    /// <inheritdoc/>
    public bool IsWebSocketRequest => _request.IsWebSocketRequest;

    /// <inheritdoc/>
    public bool KeepAlive => _request.KeepAlive;

    /// <inheritdoc/>
    public ConnectionInfo LocalEndpoint { get; }

    /// <inheritdoc/>
    public ConnectionInfo RemoteEndpoint { get; }

}