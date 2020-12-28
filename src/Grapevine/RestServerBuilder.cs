using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Grapevine
{
    public class RestServerBuilder
    {
        private IServiceCollection _services;
        private Action<IServiceCollection> _configureServices;
        private Action<IRestServer> _configureServer;

        public RestServerBuilder() : this(new ServiceCollection(), null, null) {}

        public RestServerBuilder(IServiceCollection services) : this(services, null, null) { }

        public RestServerBuilder(Func<IServiceCollection> serviceInitializer) : this(serviceInitializer, null, null) { }

        public RestServerBuilder(IServiceCollection services, Action<IServiceCollection> configureServices) : this(services, configureServices, null) { }

        public RestServerBuilder(Func<IServiceCollection> serviceInitializer, Action<IServiceCollection> configureServices) : this(serviceInitializer, configureServices, null) { }

        public RestServerBuilder(IServiceCollection services, Action<IRestServer> configureServer) : this(services, null, configureServer) { }

        public RestServerBuilder(Func<IServiceCollection> serviceInitializer, Action<IRestServer> configureServer) : this(serviceInitializer, null, configureServer) { }

        public RestServerBuilder(Func<IServiceCollection> serviceInitializer, Action<IServiceCollection> configureServices, Action<IRestServer> configureServer) : this(serviceInitializer.Invoke(), configureServices, configureServer) { }

        public RestServerBuilder(IServiceCollection services, Action<IServiceCollection> configureServices, Action<IRestServer> configureServer)
        {
            _services = services;
            _configureServices = configureServices;
            _configureServer = configureServer;
        }

        public IRestServer Build()
        {
            _services.AddSingleton<IRestServer, RestServer>();
            _services.AddSingleton<IRouter, Router>();
            _services.AddSingleton<IRouteScanner, RouteScanner>();
            _services.AddLogging(configure => configure.AddConsole());

            _configureServices?.Invoke(_services);

            var server = _services.BuildServiceProvider().GetRequiredService<IRestServer>();
            server.Router.Services = _services;

            _configureServer?.Invoke(server);

            return server;
        }

        public static RestServerBuilder UseDefaults()
        {
            Action<IServiceCollection> configServices = (services) => {
                services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace);
            };

            Action<IRestServer> configServer = (server) => {
                server.Prefixes.Add("http://localhost:1234/");
            };

            return new RestServerBuilder(new ServiceCollection(), configServices, configServer);
        }

        public static RestServerBuilder From<T>()
        {
            var type = typeof(T);
            var obj = Activator.CreateInstance(type, new object[] {});
            var methods = type.GetMethods();

            // Initialize Services: ReturnType(IServiceCollection), Args(null)
            var msi = methods.Where(m => m.ReturnParameter.ParameterType == typeof(IServiceCollection) && m.GetParameters().Length == 0).FirstOrDefault();
            Func<IServiceCollection> serviceInitializer = () =>
            {
                if (msi == null) return new ServiceCollection();
                return (IServiceCollection) msi.Invoke(obj, null);
            };

            // Configure Services: ReturnType(void), Arg[0](IServiceCollection)
            var mcs = methods.Where(m => m.ReturnParameter.ParameterType == typeof(void) && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(IServiceCollection)).FirstOrDefault();
            Action<IServiceCollection> configureServices = (s) =>
            {
                if (mcs == null) return;
                mcs.Invoke(obj, new object[]{s});
            };

            // Configure Server: ReturnType(void), Arg[0](IRestServer)
            var mcv = methods.Where(m => m.ReturnParameter.ParameterType == typeof(void) && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(IRestServer)).FirstOrDefault();
            Action<IRestServer> configureServer = (s) =>
            {
                if (mcs == null) return;
                mcv.Invoke(obj, new object[]{s});
            };

            return new RestServerBuilder(serviceInitializer, configureServices, configureServer);
        }
    }
}