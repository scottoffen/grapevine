using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Grapevine.Client
{
    public interface IRestRequestBuilder
    {
        HttpContent Content { get; set; }

        QueryParams QueryParams { get; set; }

        HttpRequestMessage Request { get; }

        string Route { get; set; }

        TimeSpan Timeout { get; set; }

        Task<HttpResponseMessage> SendAsync(HttpMethod method, CancellationToken token);
    }
}