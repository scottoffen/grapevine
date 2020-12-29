using System.Threading.Tasks;
using Grapevine;

namespace Samples.Resources
{
    [RestResource]
    public static class StaticResource
    {
        [RestRoute("Get", "/static")]
        public static async Task StaticRoute(IHttpContext context)
        {
            await context.Response.SendResponseAsync("Successfully executed a static route on a static class");
        }
    }
}