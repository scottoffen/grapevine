using System.Threading.Tasks;
using Grapevine;

namespace Samples.Resources
{
    [RestResource(BasePath = "api/abstract")]
    public abstract class AbstractResource
    {
        [RestRoute("Get", "/instance", Name = "Unaccessible", Description = "This route is not accessible")]
        public async Task AbstractRoute(IHttpContext context)
        {
            await context.Response.SendResponseAsync("Instance methods on an abstract class should never execute");
        }

        [RestRoute("Get", "/static")]
        public static async Task AbstractStaticRoute(IHttpContext context)
        {
            await context.Response.SendResponseAsync("Successfully executed a static route on an abstract class");
        }

    }

    public class Resource : AbstractResource
    {
        // Even though this class is concrete, the /api/abstract/instance route will not be accessible
    }
}