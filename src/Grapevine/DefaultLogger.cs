using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Grapevine
{
    public static class DefaultLogger
    {
        public static ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();

        public static ILogger<T> GetInstance<T>() => LoggerFactory.CreateLogger<T>();

        public static IRestServer SetDefaultLogger (this IRestServer server, ILoggerFactory factory)
        {
            LoggerFactory = factory;
            return server;
        }
    }
}