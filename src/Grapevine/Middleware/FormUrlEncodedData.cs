using System.Threading.Tasks;

namespace Grapevine.Middleware
{
    public static class FormUrlEncodedData
    {
        public async static Task Parse(IHttpContext context, IRestServer server)
        {
            if ((context.Request.ContentType != ContentType.FormUrlEncoded) || (!context.Request.HasEntityBody)) return;
            context.Locals.TryAdd("FormData", await context.Request.ParseFormUrlEncodedData());
        }
    }
}