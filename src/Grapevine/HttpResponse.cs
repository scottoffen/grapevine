using System.Diagnostics.CodeAnalysis;
using System.Net;
using Grapevine.Abstractions;

namespace Grapevine;

/// <summary>
/// Wraps an <see cref="HttpListenerResponse"/> and exposes it as an
/// <see cref="IHttpResponse"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class HttpResponse : IHttpResponse, IDisposable
{
    private readonly HttpListenerResponse _response;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="HttpResponse"/> wrapping the
    /// specified <see cref="HttpListenerResponse"/>.
    /// </summary>
    /// <param name="response">The underlying <see cref="HttpListenerResponse"/> to wrap.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="response"/> is <see langword="null"/>.
    /// </exception>
    public HttpResponse(HttpListenerResponse response)
    {
        _response = response ?? throw new ArgumentNullException(nameof(response));

        Headers = new ResponseHeaderCollection();
        Cookies = new ResponseCookieCollection();
        StatusCode = HttpStatusCode.Ok;
        ContentType = ContentType.Html;
        OutputStream = new ResponseStream(_response.OutputStream, CommitResponse);
    }

    /// <inheritdoc/>
    public HttpStatusCode StatusCode { get; set; }

    /// <inheritdoc/>
    public ContentType ContentType { get; set; }

    /// <inheritdoc/>
    public IResponseHeaderCollection Headers { get; }

    /// <inheritdoc/>
    public IResponseCookieCollection Cookies { get; }

    /// <inheritdoc/>
    public Stream OutputStream { get; }

    /// <inheritdoc/>
    public bool SendChunked
    {
        get => _response.SendChunked;
        set => _response.SendChunked = value;
    }

    /// <inheritdoc/>
    public long ContentLength64
    {
        get => _response.ContentLength64;
        set => _response.ContentLength64 = value;
    }

    /// <inheritdoc/>
    public bool WasRespondedTo { get; set; }

    /// <inheritdoc/>
    public void Abort()
    {
        // Abort terminates the connection without sending a response, so there
        // is nothing to apply. Mark as responded and abort immediately.
        WasRespondedTo = true;
        _response.Abort();
    }

    /// <inheritdoc/>
    public void Close()
    {
        CommitResponse();
        _response.Close();
        WasRespondedTo = true;
    }

    /// <inheritdoc/>
    public async Task CloseAsync()
    {
        CommitResponse();

        // HttpListenerResponse does not have a native async Close. Flush the
        // output stream asynchronously to avoid blocking the calling thread on
        // any buffered I/O, then close synchronously.
        await _response.OutputStream.FlushAsync().ConfigureAwait(false);
        _response.Close();
        WasRespondedTo = true;
    }

    /// <inheritdoc/>
    public void Redirect(string url)
    {
        // Redirect sets Location and status code internally; no need to apply
        // headers or content type since no body is sent.
        WasRespondedTo = true;
        _response.Redirect(url);
        _response.Close();
    }

    /// <summary>
    /// Seals the response headers and cookies, then commits all values from this
    /// wrapper to the underlying <see cref="HttpListenerResponse"/> before it is sent.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="HttpListenerResponse"/> restricts direct modification of certain
    /// headers (such as <c>Content-Type</c> and <c>Content-Length</c>) via the
    /// headers collection and requires them to be set via dedicated properties.
    /// Those are handled explicitly here; all remaining headers are written via
    /// <c>Headers.Add</c>.
    /// </para>
    /// <para>
    /// Cookies are serialized as <c>Set-Cookie</c> header values via
    /// <see cref="ResponseCookieCollection.ToHeaderValues"/> since
    /// <see cref="HttpListenerResponse"/> does not provide a safe structured
    /// cookie API that works across all target TFMs.
    /// </para>
    /// </remarks>
    private void CommitResponse()
    {
        if (Headers.IsSealed) return;

        // Seal headers and cookies together before writing anything to the
        // underlying response. ResponseHeaderCollection.OnSealed cascades the
        // seal to the ResponseCookieCollection, so a single call covers both.
        if (Headers is ResponseHeaderCollection responseHeaders)
            responseHeaders.Seal();

        // Status code: implicit conversion from custom HttpStatusCode to int.
        _response.StatusCode = StatusCode;

        // Content type: implicit conversion from ContentType to string.
        _response.ContentType = ContentType.ToString();

        // Write all headers except those that HttpListenerResponse manages via
        // dedicated properties, which throw if set through the headers collection.
        foreach (var header in Headers)
        {
            var name = header.Key;

            // Skip headers managed by HttpListenerResponse dedicated properties
            // to avoid a ProtocolViolationException or ArgumentException.
            if (string.Equals(name, HeaderNames.ContentType, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, HeaderNames.ContentLength, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, HeaderNames.TransferEncoding, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, HeaderNames.KeepAlive, StringComparison.OrdinalIgnoreCase))
                continue;

            var values = header.Value;
            for (var i = 0; i < values.Count; i++)
                _response.Headers.Add(name, values[i]);
        }

        // Serialize cookies as Set-Cookie header values. HttpListenerResponse.Cookies
        // has cross-TFM inconsistencies, so we write directly to the headers collection.
        foreach (var setCookieValue in Cookies.ToHeaderValues())
            _response.Headers.Add(HeaderNames.SetCookie, setCookieValue);
    }

    /// <summary>
    /// Releases the resources used by this <see cref="HttpResponse"/>, including
    /// the underlying <see cref="HttpListenerResponse.OutputStream"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the managed resources used by this <see cref="HttpResponse"/>.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release managed resources; <see langword="false"/>
    /// if called from a finalizer.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _response.OutputStream.Dispose();
        }

        _disposed = true;
    }
}