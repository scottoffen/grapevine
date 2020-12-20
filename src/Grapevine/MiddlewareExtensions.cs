using System;
using System.Collections.Generic;
using System.Linq;
using Grapevine.Middleware;

namespace Grapevine
{
    public static class MiddlewareExtensions
    {
        public static IRestServer UseContentFolders(this IRestServer server)
        {
            server.OnRequestAsync += ContentFolders.SendFileIfExistsAsnyc;
            return server;
        }

        public static IRestServer UseCorrelationId(this IRestServer server, string correlationIdFieldName)
        {
            return server.UseCorrelationId(correlationIdFieldName, null);
        }

        public static IRestServer UseCorrelationId(this IRestServer server, Func<string> correlactionIdGenerator)
        {
            return server.UseCorrelationId(null, correlactionIdGenerator);
        }

        public static IRestServer UseCorrelationId(this IRestServer server, string correlationIdFieldName, Func<string> correlactionIdGenerator)
        {
            var mw = new CorrelationId(correlationIdFieldName, correlactionIdGenerator);
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
            return server.UseCorsPolicy(allowOrigins.Cast<Uri>().ToList());
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
            var fieldName = context.Get("CorrelationIdFieldName").ToString();
            return (fieldName != null)
                ? context.Get(fieldName).ToString()
                : null;
        }
    }
}