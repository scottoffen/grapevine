using System.Threading.Tasks;
using Grapevine;

namespace Samples.Resources
{
    [RestResource]
    public abstract class AbstractResource
    {
        [RestRoute("Get", "/abstract")]
        public async Task AbstractRoute(IHttpContext context)
        {
            await context.Response.SendResponseAsync("Instance methods on an abstract class should never execute");
        }

        [RestRoute("Get", "/static-abstract")]
        public static async Task AbstractStaticRoute(IHttpContext context)
        {
            await context.Response.SendResponseAsync("Successfully executed a static route on an abstract class");
        }

    }

    public class Resource : AbstractResource
    {
        // Even though this class is concrete, the /abstract route should not be executed
    }
}