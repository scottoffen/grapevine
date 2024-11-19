using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Grapevine.Middleware;

namespace Grapevine
{
    public static class MiddlewareExtensions
    {
        public static void Run(this IRestServer server)
        {
            ManualResetEvent manualResetEvent = new ManualResetEvent(true);

            ServerEventHandler onStart = (s) => manualResetEvent.Reset();
            ServerEventHandler onStop = (s) => manualResetEvent.Set();

            server.AfterStarting += onStart;
            server.AfterStopping += onStop;

            server.Start();
            manualResetEvent.WaitOne();

            server.AfterStarting -= onStart;
            server.AfterStopping -= onStop;
        }

        public static void Run(this IRestServer server, CancellationToken token)
        {
            ManualResetEventSlim manualResetEvent = new ManualResetEventSlim(true);

            ServerEventHandler onStop = (s) =>
            {
                if (!token.IsCancellationRequested)
                    CancellationTokenSource.CreateLinkedTokenSource(token).Cancel();
            };

            server.AfterStopping += onStop;

            server.Start();
            manualResetEvent.Wait(token);
            server.Stop();

            server.AfterStopping -= onStop;
        }

        public static IRestServer AutoParseFormUrlEncodedData(this IRestServer server)
        {
            server.OnRequestAsync -= FormUrlEncodedData.Parse;
            server.OnRequestAsync += FormUrlEncodedData.Parse;
            return server;
        }

        public static IRestServer UseContentFolders(this IRestServer server)
        {
            server.OnRequestAsync -= ContentFolders.SendFileIfExistsAsync;
            server.OnRequestAsync += ContentFolders.SendFileIfExistsAsync;
            return server;
        }

        public static IRestServer UseCorrelationId(this IRestServer server, string correlationIdFieldName)
        {
            return server.UseCorrelationId(correlationIdFieldName, null);
        }

        public static IRestServer UseCorrelationId(this IRestServer server, Func<string> correlationIdGenerator)
        {
            return server.UseCorrelationId(null, correlationIdGenerator);
        }

        public static IRestServer UseCorrelationId(this IRestServer server, string correlationIdFieldName, Func<string> correlationIdGenerator)
        {
            var mw = new CorrelationId(correlationIdFieldName, correlationIdGenerator);
            server.OnRequestAsync += mw.EnsureCorrelationIdAsync;
            return server;
        }

        public static IRestServer UseCorsPolicy(this IRestServer server)
        {
            server.OnRequestAsync += CorsPolicy.CorsWildcardAsync;
            return server;
        }

        public static IRestServer UseCorsPolicy(this IRestServer server, string allowOrigin)
        {
            return server.UseCorsPolicy(new Uri(allowOrigin));
        }

        public static IRestServer UseCorsPolicy(this IRestServer server, Uri allowOrigin)
        {
            return server.UseCorsPolicy(new CorsPolicy(allowOrigin));
        }

        public static IRestServer UseCorsPolicy(this IRestServer server, IEnumerable<string> allowOrigins)
        {
            return server.UseCorsPolicy(allowOrigins.Select(x => new Uri(x)).ToList());
        }

        public static IRestServer UseCorsPolicy(this IRestServer server, IEnumerable<Uri> allowOrigins)
        {
            return server.UseCorsPolicy(new CorsPolicy(allowOrigins));
        }

        public static IRestServer UseCorsPolicy(this IRestServer server, ICorsPolicy policy)
        {
            server.OnRequestAsync += policy.ApplyAsync;
            return server;
        }

        public static string GetCorrelationId(this IHttpContext context)
        {
            var fieldName = context.Locals.Get("CorrelationIdFieldName").ToString();
            return (fieldName != null)
                ? context.Locals.Get(fieldName).ToString()
                : null;
        }
    }
}