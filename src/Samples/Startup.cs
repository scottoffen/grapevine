using System;
using System.IO;
using Grapevine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Samples.Clients;

namespace Samples
{
    public class Startup
    {
        private IConfiguration _configuration;

        private string _serverPort = PortFinder.FindNextLocalOpenPort(1234);

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddNLog(_configuration);
            });

            services.AddHttpClient<GitHubClient>(c =>
            {
                c.BaseAddress = new Uri("https://api.github.com/");
                c.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                c.DefaultRequestHeaders.Add("User-Agent", "Grapevine-Client");
            });
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

            /* Configure Router Options (if supported by your router implementation) */
            server.Router.Options.SendExceptionMessages = true;
        }
    }
}