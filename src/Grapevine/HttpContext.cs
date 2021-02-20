using System;
using System.Net;
using System.Threading;

namespace Grapevine
{
    public class HttpContext : Locals, IHttpContext
    {
        public HttpListenerContext Advanced { get; }

        public CancellationToken CancellationToken { get; }

        public string Id { get; } = Guid.NewGuid().ToString();

        public bool WasRespondedTo => Response.ResponseSent;

        public IHttpRequest Request { get; }

        public IHttpResponse Response { get; }

        public IServiceProvider Services { get; set; }

        internal HttpContext(HttpListenerContext context, CancellationToken token)
        {
            Advanced = context;
            CancellationToken = token;

            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Encoding
            var acceptEncoding = context.Request.Headers.GetValue<string>("Accept-Encoding", string.Empty);
            var identityForbidden = (acceptEncoding.Contains("identity;q=0") || acceptEncoding.Contains("*;q=0"));

            Request = new HttpRequest(context.Request);
            Response = new HttpResponse(context.Response)
            {
                CompressionProvider = new CompressionProvider(QualityValues.Parse(acceptEncoding), identityForbidden),
            };
        }
    }
}