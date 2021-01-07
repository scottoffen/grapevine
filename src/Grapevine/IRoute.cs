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
}