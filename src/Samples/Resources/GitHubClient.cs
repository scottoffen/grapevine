using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using Grapevine;
using Grapevine.Client;

namespace Samples.Resources
{
    [RestResource(BasePath = "github")]
    public class GitHubClient
    {
        private readonly HttpClient _client;

        public GitHubClient(HttpClient client)
        {
            _client = client;
        }

        [RestRoute("Get", "/issues/{owner}/{repo}", Description = "Returns the JSON for the list of open issues returned for the specified repository")]
        public async Task GetIssues(IHttpContext context)
        {
            var owner = context.Request.PathParameters["owner"];
            var repo  = context.Request.PathParameters["repo"];
            var query = (context.Request.QueryString.Count > 0)
                ? context.Request.QueryString
                : new NameValueCollection()
                    {
                        {"state", "open"},
                        {"sort", "created"},
                        {"direction", "desc"},
                    };

            await context.Response.SendResponseAsync(await _client.WithRoute($"/repos/{owner}/{repo}/issues")
                .WithQueryParams(query)
                .GetAsync()
                .GetResponseStreamAsync()
            );
        }
    }
}