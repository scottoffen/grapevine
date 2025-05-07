using System;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol.Messages;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Server;
using ModelContextProtocol.Utils.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Grapevine.Extensions.Mcp
{
    internal sealed class StreamableHttpHandler
    {

        private readonly ConcurrentDictionary<string, HttpMcpSession> _sessions = new ConcurrentDictionary<string, HttpMcpSession>(StringComparer.Ordinal);
        private readonly ILogger _logger;
        private readonly IOptions<McpServerOptions> _mcpServerOptionsSnapshot;
        private readonly IOptionsFactory<McpServerOptions> _mcpServerOptionsFactory;
        private readonly IOptions<HttpServerTransportOptions> _httpMcpServerOptions;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILoggerFactory _loggerFactory;

        public StreamableHttpHandler(IOptions<McpServerOptions> mcpServerOptionsSnapshot,
            IOptionsFactory<McpServerOptions> mcpServerOptionsFactory,
            IOptions<HttpServerTransportOptions> httpMcpServerOptions,
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory loggerFactory)
        {
            _mcpServerOptionsSnapshot = mcpServerOptionsSnapshot;
            _mcpServerOptionsFactory = mcpServerOptionsFactory;
            _httpMcpServerOptions = httpMcpServerOptions;
            _hostApplicationLifetime = hostApplicationLifetime;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<StreamableHttpHandler>();
        }

        public async Task HandleRequestAsync(HttpContext context)
        {
            if (context.Request.HttpMethod == System.Net.Http.HttpMethod.Get)
            {
                await HandleSseRequestAsync(context);
            }
            else if (context.Request.HttpMethod == System.Net.Http.HttpMethod.Post)
            {
                await HandleMessageRequestAsync(context);
            }
            else
            {
                context.Response.StatusCode = HttpStatusCode.MethodNotAllowed;
                await context.Response.SendResponseAsync(HttpStatusCode.MethodNotAllowed);
            }
        }

        public async Task HandleSseRequestAsync(HttpContext context)
        {
            // If the server is shutting down, we need to cancel all SSE connections immediately without waiting for HostOptions.ShutdownTimeout
            // which defaults to 30 seconds.
            using var sseCts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, _hostApplicationLifetime.ApplicationStopping);
            var cancellationToken = sseCts.Token;

            var response = context.Response;
            response.ContentType = "text/event-stream";
            response.Headers["Cache-Control"] = "no-cache,no-store";

            // Make sure we disable all response buffering for SSE
            response.ContentEncoding = Encoding.UTF8;
            // ASP.NET Core buffering disable removed for netstandard2.0 compatibility

            var sessionId = MakeNewSessionId();
            await using var transport = new SseResponseStreamTransport(context.Advanced.Response.OutputStream, $"message?sessionId={sessionId}");
            var httpMcpSession = new HttpMcpSession(transport, context.Advanced.User);
            if (!_sessions.TryAdd(sessionId, httpMcpSession))
            {
                Debug.Fail("Unreachable given good entropy!");
                throw new InvalidOperationException($"Session with ID '{sessionId}' has already been created.");
            }

            try
            {
                var mcpServerOptions = _mcpServerOptionsSnapshot.Value;
                if (_httpMcpServerOptions.Value.ConfigureSessionOptions is { } configureSessionOptions)
                {
                    mcpServerOptions = _mcpServerOptionsFactory.Create(Options.DefaultName);
                    await configureSessionOptions(context, mcpServerOptions, cancellationToken);
                }

                var transportTask = transport.RunAsync(cancellationToken);

                try
                {
                    await using var mcpServer = McpServerFactory.Create(transport, mcpServerOptions, _loggerFactory, context.Services);

                    var runSessionAsync = _httpMcpServerOptions.Value.RunSessionHandler ?? RunSessionAsync;
                    await runSessionAsync(context, mcpServer, cancellationToken);
                }
                finally
                {
                    await transport.DisposeAsync();
                    await transportTask;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // RequestAborted always triggers when the client disconnects before a complete response body is written,
                // but this is how SSE connections are typically closed.
            }
            finally
            {
                _sessions.TryRemove(sessionId, out _);
            }
        }

        public async Task HandleMessageRequestAsync(HttpContext context)
        {
            if (string.IsNullOrEmpty(context.Request.QueryString.Get("sessionId")))
            {
                await context.Response.SendResponseAsync(HttpStatusCode.BadRequest, "Missing sessionId query parameter.");
                return;
            }
            var sessionId = context.Request.QueryString.Get("sessionId");
            if (!_sessions.TryGetValue(sessionId, out var httpMcpSession))
            {
                await context.Response.SendResponseAsync(HttpStatusCode.BadRequest, "Invalid sessionId");
                return;
            }

            if (!httpMcpSession.HasSameUserId(context.Advanced.User))
            {
                await context.Response.SendResponseAsync(HttpStatusCode.Forbidden);
                return;
            }

            string json = new StreamReader(context.Advanced.Request.InputStream).ReadToEnd();
            var message = JsonSerializer.Deserialize<IJsonRpcMessage>(json, McpJsonUtilities.DefaultOptions);
            if (message is null)
            {
                await context.Response.SendResponseAsync(HttpStatusCode.BadRequest, "No message in request body.");
                return;
            }

            await httpMcpSession.Transport.OnMessageReceivedAsync(message, context.CancellationToken);
            context.Response.StatusCode = HttpStatusCode.Accepted;
            await context.Response.SendResponseAsync(HttpStatusCode.Accepted);
        }

        private static Task RunSessionAsync(HttpContext httpContext, IMcpServer session, CancellationToken requestAborted)
            => session.RunAsync(requestAborted);

        private static string MakeNewSessionId()
        {
            {
                var buffer = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(buffer);
                }
                string base64 = Convert.ToBase64String(buffer);
                return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
            }
        }
    }
}
