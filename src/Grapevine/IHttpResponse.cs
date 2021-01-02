using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Grapevine
{
    public interface IHttpResponse
    {
        /// <summary>
        /// Gets or sets the Encoding for this response's OutputStream
        /// </summary>
        Encoding ContentEncoding { get; set; }

        /// <summary>
        /// Gets or sets an integer to indicate the minimum number of bytes after which content will potentially be compressed before being returned to the client
        /// </summary>
        TimeSpan ContentExpiresDuration { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes in the body data included in the response
        /// </summary>
        long ContentLength64 { get; set; }

        /// <summary>
        /// Gets or sets the MIME type of the content returned
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the collection of cookies returned with the response
        /// </summary>
        CookieCollection Cookies { get; set; }

        /// <summary>
        /// Gets or sets the collection of header name/value pairs returned by the server
        /// </summary>
        WebHeaderCollection Headers { get; set; }

        string RedirectLocation { get; set; }

        /// <summary>
        /// Gets a value indicating whether a response has been sent to this request
        /// </summary>
        bool ResponseSent { get; }

        /// <summary>
        /// Gets or sets the HTTP status code to be returned to the client
        /// </summary>
        int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets a text description of the HTTP status code returned to the client
        /// </summary>
        string StatusDescription { get; set; }

        /// <summary>
        /// Gets or sets whether the response uses chunked transfer encoding
        /// </summary>
        bool SendChunked { get; set; }

        /// <summary>
        /// Closes the connection to the client without sending a response.
        /// </summary>
        void Abort();

        /// <summary>
        /// Adds the specified header and value to the HTTP headers for this response
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void AddHeader(string name, string value);

        /// <summary>
        /// Adds the specified Cookie to the collection of cookies for this response
        /// </summary>
        /// <param name="cookie"></param>
        void AppendCookie(Cookie cookie);

        /// <summary>
        /// Appends a value to the specified HTTP header to be sent with this response
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void AppendHeader(string name, string value);

        /// <summary>
        /// Configures the response to redirect the client to the specified URL
        /// </summary>
        /// <param name="url"></param>
        void Redirect(string url);

        /// <summary>
        /// Write the contents of the buffer to and then closes the OutputStream, followed by closing the Response
        /// </summary>
        /// <param name="contents"></param>
        Task SendResponseAsync(byte[] contents);

        /// <summary>
        /// Adds or updates a Cookie in the collection of cookies sent with this response
        /// </summary>
        /// <param name="cookie"></param>
        void SetCookie(Cookie cookie);
    }

    public static class IHttpResponseExtensions
    {
        public static async Task SendResponseAsync(this IHttpResponse response, Stream content, string filename)
        {
            if (!string.IsNullOrWhiteSpace(filename))
                response.AddHeader("Content-Disposition", $"attachment; filename=\"{filename}\"");

            await response.SendResponseAsync(content);
        }

        public static async Task SendResponseAsync(this IHttpResponse response, Stream content)
        {
            if (!response.Headers.AllKeys.ToArray().Contains("Expires"))
                response.AddHeader("Expires",
                    DateTime.Now.Add(response.ContentExpiresDuration).ToString("R"));

            using (var buffer = new MemoryStream())
            {
                await content.CopyToAsync(buffer);
                await response.SendResponseAsync(buffer.ToArray());
            }
        }

        public static async Task SendResponseAsync(this IHttpResponse response, HttpStatusCode statusCode)
        {
            response.StatusCode = statusCode;
            await response.SendResponseAsync(statusCode.ToString());
        }

        public static async Task SendResponseAsync(this IHttpResponse response, string content)
        {
            await response.SendResponseAsync(response.ContentEncoding.GetBytes(content));
        }
    }
}