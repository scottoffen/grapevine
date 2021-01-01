using System;
using System.IO;
using System.IO.Compression;
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

        public HttpResponseBase(HttpListenerResponse response)
        {
            Advanced = response;
        }
    }

    public class HttpResponse : HttpResponseBase, IHttpResponse
    {
        public bool IsCompressible { get; protected internal set; }

        public int CompressIfBytesGreaterThan { get; set; } = 1024;

        public HttpResponse(HttpListenerResponse response) : base(response)
        {
            response.ContentEncoding = Encoding.ASCII;
        }

        public virtual async Task<byte[]> CompressContentsAsync(byte[] contents)
        {
            if (!IsCompressible || contents.Length < CompressIfBytesGreaterThan) return contents;

            Headers["Content-Encoding"] = "gzip";
            using (var ms = new MemoryStream())
            using (var gzip = new GZipStream(ms, CompressionMode.Compress))
            {
                await gzip.WriteAsync(contents, 0, contents.Length);
                return ms.ToArray();
            }
        }

        public async override Task SendResponseAsync(byte[] contents)
        {
            try
            {
                contents = await CompressContentsAsync(contents);
                ContentLength64 = contents.Length;

                // if (((ContentType)ContentType).IsBinary) SendChunked = true;
                await Advanced.OutputStream.WriteAsync(contents, 0, (int)ContentLength64);
                Advanced.OutputStream.Close();
            }
            catch
            {
                Advanced.OutputStream.Close();
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