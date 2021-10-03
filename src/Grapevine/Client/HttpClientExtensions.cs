using System.Net.Http;

namespace Grapevine.Client
{
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Allows setting a route for the HTTP request and initiates the chaining of extensions
        /// </summary>
        /// <param name="client">HttpClient instance to be used for request</param>
        /// <param name="route">Endpoint URL, can be empty if the base address already targets the endpoint</param>
        /// <returns></returns>
        public static RestRequestBuilder UsingRoute(this HttpClient client, string route = "")
        {
            return new RestRequestBuilder(client).UsingRoute(route);
        }
    }
}