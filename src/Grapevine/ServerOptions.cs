using System.Net;
using System.Threading;

namespace Grapevine
{
    /// <summary>
    /// Delegate for the <see cref="ServerOptions.HttpContextFactory"/> factory
    /// </summary>
    /// <param name="context"></param>
    /// <param name="server"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public delegate IHttpContext HttpContextFactory(object state, IRestServer server, CancellationToken token);

    public class ServerOptions
    {
        /// <summary>
        /// Gets or sets a value that indicates whether autoscan is enabled on this router.
        /// </summary>
        /// <value>true</value>
        public bool EnableAutoScan { get; set; } = true;

        /// <summary>
        /// Gets or sets the delegate that creates IHttpContext objects.
        /// </summary>
        /// <value></value>
        public HttpContextFactory HttpContextFactory { get; set; } = (state, server, token) => new HttpContext(state as HttpListenerContext, server, token);
    }
}