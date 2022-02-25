using System.Threading.Tasks;

namespace Grapevine.Middleware
{
    public static class FormUrlEncodedData
    {
        public async static Task Parse(IHttpContext context, IRestServer server)
        {
            if (context.Request.ContentType != "application/x-www-form-urlencoded") return;
            context.Locals.TryAdd("FormData", await context.Request.ParseFormUrlEncodedData());
        }
    }
}