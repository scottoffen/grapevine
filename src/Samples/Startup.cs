using System;
using System.IO;
using Grapevine;
// Uncomment the following line to enable MCP extensions via Grapevine.Extensions.Mcp
//     using Grapevine.Extensions.Mcp;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Samples.Clients;

namespace Samples
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }

        private string _serverPort = PortFinder.FindNextLocalOpenPort(1234);

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddNLog(new NLogLoggingConfiguration(Configuration.GetSection("NLog")));
            });

            services.AddHttpClient<GitHubClient>(c =>
            {
                c.BaseAddress = new Uri("https://api.github.com/");
                c.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                c.DefaultRequestHeaders.Add("User-Agent", "Grapevine-Client");
            });

            // Uncomment the following lines to register and configure the MCP server
            //     services.AddMcpServer()
            //         .WithHttpTransport()
            //         .WithPromptsFromAssembly()
            //         .WithToolsFromAssembly();
        }

        public void ConfigureServer(IRestServer server)
        {
            // The path to your static content
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "website");

            // The following line is shorthand for:
            //     server.ContentFolders.Add(new ContentFolder(folderPath));
            server.ContentFolders.Add(folderPath);
            server.UseContentFolders();

            server.Prefixes.Add($"http://localhost:{_serverPort}/");

            /* Configure server to auto parse application/x-www-for-urlencoded data*/
            server.AutoParseFormUrlEncodedData();

            /* Configure Router Options (if supported by your router implementation) */
            server.Router.Options.SendExceptionMessages = true;

            // Uncomment the following line to map MCP endpoints on the server
            //     server.MapMcp();
        }
    }
}