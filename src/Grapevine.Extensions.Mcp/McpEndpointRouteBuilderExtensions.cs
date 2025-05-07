using System;

using Microsoft.Extensions.DependencyInjection;

namespace Grapevine.Extensions.Mcp
{
    /// <summary>
    /// Provides extension methods for <see cref="IEndpointRouteBuilder"/> to add MCP endpoints.
    /// </summary>
    public static class McpEndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Sets up endpoints for handling MCP HTTP Streaming transport.
        /// See <see href="https://modelcontextprotocol.io/specification/2025-03-26/basic/transports#streamable-http">the protocol specification</see> for details about the Streamable HTTP transport.
        /// </summary>
        /// <param name="endpoints">The web application to attach MCP HTTP endpoints.</param>
        /// <param name="pattern">The route pattern prefix to map to.</param>
        /// <returns>Returns a builder for configuring additional endpoint conventions like authorization policies.</returns>
        public static IRestServer MapMcp(this IRestServer server, string pattern = "")
        {
            var handler = server.Router.Services
                              .BuildServiceProvider()
                              .GetService<StreamableHttpHandler>() ??
                          throw new InvalidOperationException("You must call WithHttpTransport(). Unable to find required services. Call builder.Services.AddMcpServer().WithHttpTransport() in application startup code.");

            var prefix = pattern?.TrimEnd('/') ?? "";
            server.Router.Register(
                new Route(ctx => handler.HandleRequestAsync((HttpContext)ctx), "Get", prefix));
            server.Router.Register(
                new Route(ctx => handler.HandleRequestAsync((HttpContext)ctx), "Get", prefix + "/sse"));
            server.Router.Register(
                new Route(ctx => handler.HandleRequestAsync((HttpContext)ctx), "Post", prefix + "/message"));

            // Catch-all fallback for any unmatched GET routes -> 404 Not Found
            server.Router.Register(
                new Route(ctx => ctx.Response.SendResponseAsync(HttpStatusCode.NotFound), "Get", "/{*catchall}")
            );

            return server;
        }
    }
}
