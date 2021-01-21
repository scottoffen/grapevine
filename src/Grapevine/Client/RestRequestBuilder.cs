using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Grapevine.Client
{
    public class RestRequestBuilder
    {
        private readonly HttpClient _client;

        public HttpContent Content { get; set; }

        public QueryParams QueryParams { get; set; } = new QueryParams();

        public HttpRequestMessage Request { get; } = new HttpRequestMessage();

        public string Route { get; set; } = "";

        public TimeSpan Timeout
        {
            get { return _client.Timeout; }
            set { _client.Timeout = value; }
        }

        internal RestRequestBuilder(HttpClient client)
        {
            _client = client;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method, CancellationToken token)
        {
            Request.Content = Content;
            Request.Method = method;
            Request.RequestUri = new Uri($"{_client.BaseAddress.ToString().TrimEnd('/')}/{Route.TrimStart('/')}{QueryParams.ToString()}");

            return await _client.SendAsync(Request, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false);
        }
    }
}