using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Grapevine
{
    public class RestServerBuilder
    {
        public IConfiguration Configuration { get; set; }
        public IServiceCollection Services { get; set; }
        public Action<IServiceCollection> ConfigureServices { get; set; }
        public Action<IRestServer> ConfigureServer { get; set; }

        public RestServerBuilder() : this(null, null, null, null) { }

        public RestServerBuilder(IServiceCollection services) : this(services, null, null, null) { }

        public RestServerBuilder(IServiceCollection services, IConfiguration configuration) : this(services, configuration, null, null) { }

        public RestServerBuilder(IServiceCollection services, IConfiguration configuration, Action<IServiceCollection> configureServices) : this(services, configuration, configureServices, null) { }

        public RestServerBuilder(IServiceCollection services, IConfiguration configuration, Action<IRestServer> configureServer) : this(services, configuration, null, configureServer) { }

        public RestServerBuilder(IServiceCollection services, Action<IServiceCollection> configureServices) : this(services, null, configureServices, null) { }

        public RestServerBuilder(IServiceCollection services, Action<IRestServer> configureServer) : this(services, null, null, configureServer) { }

        public RestServerBuilder(IServiceCollection services, Action<IServiceCollection> configureServices, Action<IRestServer> configureServer) : this(services, null, configureServices, configureServer) { }

        public RestServerBuilder(IServiceCollection services, IConfiguration configuration, Action<IServiceCollection> configureServices, Action<IRestServer> configureServer)
        {
            Services = services ?? new ServiceCollection();
            Configuration = configuration ?? GetDefaultConfiguration();
            ConfigureServices = configureServices;
            ConfigureServer = configureServer;
        }

        public IRestServer Build()
        {
            if (Configuration == null) Configuration = GetDefaultConfiguration();

            Services.AddSingleton(typeof(IConfiguration), Configuration);
            Services.AddSingleton<IRestServer, RestServer>();
            Services.AddSingleton<IRouter, Router>();
            Services.AddSingleton<IRouteScanner, RouteScanner>();
            Services.AddTransient<IContentFolder, ContentFolder>();

            ConfigureServices?.Invoke(Services);

            var provider = Services.BuildServiceProvider();

            var server = provider.GetRequiredService<IRestServer>();
            server.Router.Services = Services;
            server.RouteScanner.Services = Services;

            var factory = provider.GetService<ILoggerFactory>();
            if (factory != null) server.SetDefaultLogger(factory);

            var assembly = GetType().Assembly.GetName();
            server.GlobalResponseHeaders.Add("Server", $"{assembly.Name}/{assembly.Version} ({RuntimeInformation.OSDescription})");

            // Override with instances
            Services.AddSingleton<IRestServer>(server);
            Services.AddSingleton<IRouter>(server.Router);
            Services.AddSingleton<IRouteScanner>(server.RouteScanner);

            ConfigureServer?.Invoke(server);

            return server;
        }

        public static RestServerBuilder UseDefaults()
        {
            var config = GetDefaultConfiguration();

            Action<IServiceCollection> configServices = (services) =>
            {
                services.AddLogging(configure => configure.AddConsole());
                services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace);
            };

            Action<IRestServer> configServer = (server) =>
            {
                server.Prefixes.Add("http://localhost:1234/");
            };

            return new RestServerBuilder(new ServiceCollection(), config, configServices, configServer);
        }

        public static RestServerBuilder From<T>()
        {
            var type = typeof(T);

            // Get the constructor
            var constructor = type.GetConstructors().Where(c =>
            {
                var args = c.GetParameters();
                return args.Count() == 1 && args.First().ParameterType == typeof(IConfiguration);
            }).FirstOrDefault();

            // Get the configuration
            var config = GetDefaultConfiguration();

            // Instanciate startup
            var obj = (constructor != null)
                ? Activator.CreateInstance(type, new object[] { config })
                : Activator.CreateInstance(type, new object[] { });

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            // Initialize Configuration: ReturnType(IConfiguration), Args(null)
            var mci = methods.Where(m => m.ReturnParameter.ParameterType == typeof(IConfiguration) && m.GetParameters().Length == 0).FirstOrDefault();
            Func<IConfiguration> configInitializer = () =>
            {
                if (mci == null) return config;
                return (IConfiguration)mci.Invoke(obj, null);
            };

            // Initialize Services: ReturnType(IServiceCollection), Args(null)
            var msi = methods.Where(m => m.ReturnParameter.ParameterType == typeof(IServiceCollection) && m.GetParameters().Length == 0).FirstOrDefault();
            Func<IServiceCollection> serviceInitializer = () =>
            {
                if (msi == null) return new ServiceCollection();
                return (IServiceCollection)msi.Invoke(obj, null);
            };

            // Configure Services: ReturnType(void), Arg[0](IServiceCollection)
            var mcs = methods.Where(m => m.ReturnParameter.ParameterType == typeof(void) && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(IServiceCollection));
            Action<IServiceCollection> configureServices = (s) =>
            {
                if (!mcs.Any()) return;
                foreach (var method in mcs) method.Invoke(obj, new object[] { s });
            };

            // Configure Server: ReturnType(void), Arg[0](IRestServer)
            var mcr = methods.Where(m => m.ReturnParameter.ParameterType == typeof(void) && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(IRestServer));
            Action<IRestServer> configureServer = (s) =>
            {
                if (!mcr.Any()) return;
                foreach (var method in mcr) method.Invoke(obj, new object[] { s });
            };

            return new RestServerBuilder(serviceInitializer.Invoke(), configInitializer.Invoke(), configureServices, configureServer);
        }

        private static IConfiguration GetDefaultConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            return config;
        }
    }

    public static class RestServerBuilderExtensions
    {
        public static RestServerBuilder UseConfiguration(this RestServerBuilder builder, IConfiguration configuration)
        {
            builder.Configuration = configuration;
            return builder;
        }

        public static RestServerBuilder BuildConfiguration(this RestServerBuilder builder, Func<IConfiguration> configurationBuilder)
        {
            builder.Configuration = configurationBuilder.Invoke();
            return builder;
        }

        public static RestServerBuilder UseContainer(this RestServerBuilder builder, IServiceCollection services)
        {
            builder.Services = services;
            return builder;
        }

        public static RestServerBuilder BuildContainer(this RestServerBuilder builder, Func<IServiceCollection> servicesBuilder)
        {
            builder.Services = servicesBuilder.Invoke();
            return builder;
        }
    }
}
