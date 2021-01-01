using System;
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