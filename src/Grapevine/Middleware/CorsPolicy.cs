using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grapevine.Middleware
{
    public interface ICorsPolicy
    {
        Task ApplyAsync(IHttpContext context);
    }

    public class CorsPolicy : ICorsPolicy
    {
        public static async Task CorsWildcardAsync(IHttpContext context)
        {
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            context.Response.AddHeader("Access-Control-Allow-Headers", "X-Requested-With");
            await Task.CompletedTask;
        }

        private IEnumerable<string> _allowedOrigins;

        public CorsPolicy(Uri allowOrigin)
        {
            _allowedOrigins = new List<string>() { allowOrigin.ToString() };
        }

        public CorsPolicy(IEnumerable<Uri> allowOrigins)
        {
            _allowedOrigins = allowOrigins.Cast<string>();
        }

        public async Task ApplyAsync(IHttpContext context)
        {
            if (_allowedOrigins.Count<string>() == 1)
            {
                context.Response.AddHeader("Access-Control-Allow-Origin", _allowedOrigins.First<string>());
                context.Response.AddHeader("Vary", "Origin");
            }
            else
            {
                var domain = context.Request.UrlReferrer?.ToString();

                if (!string.IsNullOrWhiteSpace(domain) && _allowedOrigins.Contains(domain))
                {
                    context.Response.AddHeader("Access-Control-Allow-Origin", domain);
                    context.Response.AddHeader("Vary", "Origin");
                }
            }

            await Task.CompletedTask;
        }
    }
}