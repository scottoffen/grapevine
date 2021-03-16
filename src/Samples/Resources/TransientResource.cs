using System;
using System.Text;
using System.Threading.Tasks;
using Grapevine;
using Microsoft.Extensions.DependencyInjection;

namespace Samples.Resources
{
    [RestResource(BasePath = "api/transient")]
    [ResourceLifetime(ServiceLifetime.Transient)]
    public class TransientResource
    {
        public static int Counter = 0;

        public TransientResource()
        {
            Counter++;
        }

        [RestRoute("Get", "{key:maxlength(20)}/{value}")]
        public async Task TransientCallA(IHttpContext context)
        {
            if (!context.Locals.ContainsKey("PathParameters")) context.Locals.TryAdd("PathParameters", new StringBuilder());
            var sb = context.Locals.GetAs<StringBuilder>("PathParameters");

            sb.Append($"A{Counter}: Count: {context.Request.PathParameters.Count}{Environment.NewLine}");
            sb.Append($"\tkey: {context.Request.PathParameters["key"]}{Environment.NewLine}");
            sb.Append($"\tvalue: {context.Request.PathParameters["value"]}{Environment.NewLine}");

            await Task.CompletedTask;
        }

        [RestRoute("Get", @"^/([a-zA-Z]+)/(\d{1,})")]
        public async Task TransientCallB(IHttpContext context)
        {
            if (!context.Locals.ContainsKey("PathParameters")) context.Locals.TryAdd("PathParameters", new StringBuilder());
            var sb = context.Locals.GetAs<StringBuilder>("PathParameters");

            sb.Append($"B{Counter}: Count: {context.Request.PathParameters.Count}{Environment.NewLine}");
            sb.Append($"\tp0: {context.Request.PathParameters["p0"]}{Environment.NewLine}");
            sb.Append($"\tp1: {context.Request.PathParameters["p1"]}{Environment.NewLine}");

            await Task.CompletedTask;
        }

        [RestRoute("Get", "{thing1}/{thing2:num}")]
        public async Task TransientCallC(IHttpContext context)
        {
            if (!context.Locals.ContainsKey("PathParameters")) context.Locals.TryAdd("PathParameters", new StringBuilder());
            var sb = context.Locals.GetAs<StringBuilder>("PathParameters");

            sb.Append($"C{Counter}: Count: {context.Request.PathParameters.Count}{Environment.NewLine}");
            sb.Append($"\tthing1: {context.Request.PathParameters["thing1"]}{Environment.NewLine}");
            sb.Append($"\tthing2: {context.Request.PathParameters["thing2"]}{Environment.NewLine}");

            await context.Response.SendResponseAsync($"{sb.ToString()}{Environment.NewLine}");
        }
    }
}