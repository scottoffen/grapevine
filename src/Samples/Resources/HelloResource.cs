using System.Threading.Tasks;
using Grapevine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Samples.Resources
{
    [RestResource]
    [ResourceLifetime(ServiceLifetime.Scoped)]

    public class HelloResource
    {
        private static int Count = 0;

        private readonly ILogger<HelloResource> _logger;

        public HelloResource(ILogger<HelloResource> logger)
        {
            _logger = logger;
            Count++;
        }

        [RestRoute("Get", "/hello", Name = "Hello, world!", Description = "The obligatory Hello, world! route")]
        public async Task HelloWorld(IHttpContext context)
        {
            await context.Response.SendResponseAsync($"Hello, world! ({Count})");
        }

        [RestRoute("Get", "/stop", Name = "Server Stop")]
        public async Task StopServer(IHttpContext context)
        {
            await context.Response.SendResponseAsync("Stopping Server");
            context.Server.Stop();
        }

        [RestRoute(Name = "Default Route", Description = "The default route is diabled by default", Enabled = false)]
        public async Task DefaultRoute(IHttpContext context)
        {
            _logger.LogTrace($"{context.Request.HttpMethod} {context.Request.PathInfo} : Catch All Method");
            await context.Response.SendResponseAsync(HttpStatusCode.Ok);
        }
    }
}