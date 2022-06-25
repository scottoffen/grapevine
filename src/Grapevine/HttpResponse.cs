using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Grapevine
{
    public abstract class HttpResponseBase : IHttpResponse
    {
        public HttpListenerResponse Advanced { get; }

        public Encoding ContentEncoding
        {
            get { return Advanced.ContentEncoding; }
            set { Advanced.ContentEncoding = value; }
        }

        public TimeSpan ContentExpiresDuration { get; set; } = TimeSpan.FromDays(1);

        public long ContentLength64
        {
            get { return Advanced.ContentLength64; }
            set { Advanced.ContentLength64 = value; }
        }

        public string ContentType
        {
            get { return Advanced.ContentType; }
            set { Advanced.ContentType = value; }
        }

        public CookieCollection Cookies
        {
            get { return Advanced.Cookies; }
            set { Advanced.Cookies = value; }
        }

        public WebHeaderCollection Headers
        {
            get { return Advanced.Headers; }
            set { Advanced.Headers = value; }
        }

        public string RedirectLocation
        {
            get { return Advanced.RedirectLocation; }
            set { Advanced.RedirectLocation = value; }
        }

        public bool ResponseSent { get; protected internal set; }

        public int StatusCode
        {
            get { return Advanced.StatusCode; }
            set
            {
                Advanced.StatusDescription = (HttpStatusCode)value;
                Advanced.StatusCode = value;
            }
        }

        public string StatusDescription
        {
            get { return Advanced.StatusDescription; }
            set { Advanced.StatusDescription = value; }
        }

        public bool SendChunked
        {
            get { return Advanced.SendChunked; }
            set { Advanced.SendChunked = value; }
        }

        public void Abort()
        {
            ResponseSent = true;
            Advanced.Abort();
        }

        public void AddHeader(string name, string value) => Advanced.AddHeader(name, value);

        public void AppendCookie(Cookie cookie) => Advanced.AppendCookie(cookie);

        public void AppendHeader(string name, string value) => Advanced.AppendHeader(name, value);

        public void Redirect(string url)
        {
            ResponseSent = true;
            Advanced.Redirect(url);
        }

        public abstract Task SendResponseAsync(byte[] contents);

        public void SetCookie(Cookie cookie) => Advanced.SetCookie(cookie);

        protected HttpResponseBase(HttpListenerResponse response)
        {
            Advanced = response;
            response.ContentEncoding = Encoding.UTF8;
        }

        /// <summary>
        /// Use this method to manually set the ResponseSent property to true. Generally, the value of this property is set by the SendResponseAsync method. If, however, this method was bypassed and the Advanced property was used to directly access the output stream, then the ResponseSent property will need to be set here to avoid an exception being thrown later in the routing process.
        /// </summary>
        public virtual void MarkAsResponseSent()
        {
            ResponseSent = true;
        }
    }

    public class HttpResponse : HttpResponseBase, IHttpResponse
    {
        public CompressionProvider CompressionProvider { get; set; }

        public HttpResponse(HttpListenerResponse response) : base(response) { }

        public virtual async Task<byte[]> CompressContentsAsync(byte[] contents)
        {
            if (ContentType != null && ((ContentType)ContentType).IsBinary) return contents;
            if (contents.Length <= CompressionProvider.CompressIfContentLengthGreaterThan) return contents;

            Headers["Content-Encoding"] = CompressionProvider.ContentEncoding;
            return await CompressionProvider.CompressAsync(contents);
        }

        public async override Task SendResponseAsync(byte[] contents)
        {
            if (!Advanced.OutputStream.CanWrite) throw new NotSupportedException("The response output stream can not be written to. It may have previously been written to, or the connection may no longer be available.");

            try
            {
                contents = await CompressContentsAsync(contents);
                ContentLength64 = contents.Length;

                await Advanced.OutputStream.WriteAsync(contents, 0, (int)ContentLength64);
                Advanced.OutputStream.Close();
            }
            catch (StatusCodeException sce)
            {
                StatusCode = sce.StatusCode;
            }
            catch
            {
                if (Advanced.OutputStream.CanWrite) Advanced.OutputStream.Close();
                throw;
            }
            finally
            {
                ResponseSent = true;
                Advanced.Close();
            }
        }
    }
}