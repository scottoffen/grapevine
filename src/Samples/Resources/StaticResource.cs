using System.Threading.Tasks;
using Grapevine;

namespace Samples.Resources
{
    [RestResource(BasePath = "api/static")]
    public static class StaticResource
    {
        [RestRoute("Get", "/method")]
        public static async Task StaticRoute(IHttpContext context)
        {
            await context.Response.SendResponseAsync("Successfully executed a static route on a static class");
        }
    }
}