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

        public IRestServer Server { get; }

        public IServiceProvider Services { get; set; }

        internal HttpContext(HttpListenerContext context, IRestServer server, CancellationToken token)
        {
            Advanced = context;
            Server = server;
            CancellationToken = token;

            var encoding = context.Request.Headers.Get("Accept-Encoding") ?? string.Empty;

            Request = new HttpRequest(context.Request);
            Response = new HttpResponse(context.Response)
            {
                IsCompressible = (encoding.Contains("gzip")),
                CompressIfBytesGreaterThan = server.Router.Options.CompressIfBytesGreaterThan,
                ContentExpiresDuration = server.Router.Options.ContentExpiresDuration
            };

            server.ApplyGlobalResponseHeaders(Response.Headers);
        }
    }
}