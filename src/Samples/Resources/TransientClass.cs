using System;
using System.Text;
using System.Threading.Tasks;
using Grapevine;
using Microsoft.Extensions.DependencyInjection;

namespace Samples.Resources
{
    [RestResource(BasePath = "api/transient")]
    [ResourceLifetime(ServiceLifetime.Transient)]
    public class TransientClass
    {
        public static int Counter = 0;

        public TransientClass()
        {
            Counter++;
        }

        [RestRoute]
        public async Task TransientCallA(IHttpContext context)
        {
            // You can store anything in locals!
            if (!context.Contains("Call-Order")) context.Set("Call-Order", new StringBuilder());
            context.GetAs<StringBuilder>("Call-Order").Append($"A-{Counter}{Environment.NewLine}");
            await Task.CompletedTask;
        }

        [RestRoute]
        public async Task TransientCallB(IHttpContext context)
        {
            // You can store anything in locals!
            if (!context.Contains("Call-Order")) context.Set("Call-Order", new StringBuilder());
            context.GetAs<StringBuilder>("Call-Order").Append($"B-{Counter}{Environment.NewLine}");
            await Task.CompletedTask;
        }

        [RestRoute]
        public async Task TransientCallC(IHttpContext context)
        {
            // You can store anything in locals!
            if (!context.Contains("Call-Order")) context.Set("Call-Order", new StringBuilder());

            var sb = context.GetAs<StringBuilder>("Call-Order");
            sb.Append($"C-{Counter}{Environment.NewLine}");

            await context.Response.SendResponseAsync(sb.ToString());
        }
    }
}