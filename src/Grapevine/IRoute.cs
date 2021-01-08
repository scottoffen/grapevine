using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Grapevine
{
    public interface IRoute : IRouteProperties
    {
        Regex UrlPattern { get; }

        Task InvokeAsync(IHttpContext context);

        bool IsMatch(IHttpContext context);

        IRoute WithHeader(string header, Regex pattern);
    }

    public static class IRouteExtensions
    {
        public static IRoute Disable(this IRoute route)
        {
            route.Enabled = false;
            return route;
        }

        public static IRoute Enable(this IRoute route)
        {
            route.Enabled = true;
            return route;
        }
    }
}