using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Grapevine
{
    public interface IRoute : IRouteProperties
    {
        Regex UrlPattern { get; }

        Task InvokeAsync(IHttpContext context);

        bool Matches(IHttpContext context);

        IRoute MatchOn(string header, Regex pattern);
    }
}