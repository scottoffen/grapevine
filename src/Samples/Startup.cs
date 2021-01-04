using System;
using System.IO;
using Grapevine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Samples
{
    public class Startup
    {
        public IServiceCollection GetServices()
        {
            return new ServiceCollection();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace);
        }

        public void ConfigureServer(IRestServer server)
        {
            // Use this if you want to live-edit the website
            server.ContentFolders.Add(new ContentFolder(Path.Combine(Directory.GetCurrentDirectory(), "website")));

            // Use this if you want to use the published website (for production)
            // server.ContentFolders.Add(new ContentFolder(Path.Combine(AppContext.BaseDirectory, "website")));

            server.UseContentFolders();

            server.Prefixes.Add("http://localhost:1234/");

            /* Configure Router Options (if supported by your router implementation) */
            server.Router.ConfigureOptions((options) =>
            {
                options.ContentExpiresDuration = TimeSpan.FromSeconds(1);
                options.SendExceptionMessages = true;
            });
        }
    }
}