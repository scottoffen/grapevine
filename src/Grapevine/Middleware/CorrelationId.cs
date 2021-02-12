using System;
using System.Threading.Tasks;

namespace Grapevine.Middleware
{
    public class CorrelationId
    {
        public static string DefaultCorrelationIdFieldName { get; set; } = "X-Correlation-Id";

        public static Func<string> DefaultCorrelationIdFactory { get; set; } = () => { return Guid.NewGuid().ToString(); };

        public string CorrelationIdFieldName { get; }

        public Func<string> CorrelationIdFactory { get; }

        public CorrelationId(string fieldName, Func<string> factory)
        {
            CorrelationIdFieldName = fieldName ?? DefaultCorrelationIdFieldName;
            CorrelationIdFactory = factory ?? DefaultCorrelationIdFactory;
        }

        public async Task EnsureCorrelationIdAsync(IHttpContext context, IRestServer server)
        {
            string value = context.Request.Headers[CorrelationIdFieldName] ?? CorrelationIdFactory();
            context.Response.AddHeader(CorrelationIdFieldName, value);
            context.Set("CorrelationIdFieldName", CorrelationIdFieldName);
            context.Set(CorrelationIdFieldName, value);
            await Task.CompletedTask;
        }
    }
}