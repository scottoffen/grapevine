using System.Threading.Tasks;
using Grapevine;
using Microsoft.Extensions.Logging;

namespace Samples.Resources
{
    [RestResource]
    public class MyFirstResource
    {
        private readonly ILogger<MyFirstResource> _logger;

        public MyFirstResource(ILogger<MyFirstResource> logger)
        {
            _logger = logger;
        }

        [RestRoute("Get", "/hello", Name = "Test Route", Description = "Just a route to test stuff with.", Enabled = true)]
        public async Task GetTest(IHttpContext context)
        {
            await context.Response.SendResponseAsync("Hello, world!");
        }

        [RestRoute("Get", "/stop", Name = "Server Stop")]
        public async Task StopServer(IHttpContext context)
        {
            await context.Response.SendResponseAsync("Stopping Server");
            context.Server.Stop();
        }


        [RestRoute]
        public async Task GetResource(IHttpContext context)
        {
            _logger.LogTrace($"{context.Request.HttpMethod} {context.Request.PathInfo} : Catch All Method");
            await context.Response.SendResponseAsync(HttpStatusCode.Ok);
        }
    }
}