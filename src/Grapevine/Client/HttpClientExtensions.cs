using System.Collections.Generic;
using System.Net.Http;

namespace Grapevine.Client
{
    public static class HttpClientExtensions
    {
        public static IRestRequestBuilder WithAuthentication(this HttpClient client, string scheme, string token)
        {
            var builder = new RestRequestBuilder(client);
            return builder.WithAuthentication(scheme, token);
        }

        public static IRestRequestBuilder WithBasicAuthentication(this HttpClient client, string username, string password)
        {
            var builder = new RestRequestBuilder(client);
            return builder.WithBasicAuthentication(username, password);
        }

        public static IRestRequestBuilder WithBasicAuthentication(this HttpClient client, string token)
        {
            var builder = new RestRequestBuilder(client);
            return builder.WithBasicAuthentication(token);
        }

        public static IRestRequestBuilder WithContent(this HttpClient client, HttpContent content)
        {
            var builder = new RestRequestBuilder(client);
            builder.Content = content;
            return builder;
        }

        public static IRestRequestBuilder WithContent(this HttpClient client, string content)
        {
            var builder = new RestRequestBuilder(client);
            builder.Content = new StringContent(content);
            return builder;
        }

        public static IRestRequestBuilder WithHeader(this HttpClient client, string key, string value)
        {
            var builder = new RestRequestBuilder(client);
            return builder.WithHeader(key, value);
        }

        public static IRestRequestBuilder WithHeaders(this HttpClient client, IEnumerable<KeyValuePair<string, string>> headers)
        {
            var builder = new RestRequestBuilder(client);
            return builder.WithHeaders(headers);
        }

        public static IRestRequestBuilder WithOAuthBearerToken(this HttpClient client, string token)
        {
            var builder = new RestRequestBuilder(client);
            return builder.WithOAuthBearerToken(token);
        }

        public static IRestRequestBuilder WithRequestTimeout(this HttpClient client, int seconds)
        {
            var builder = new RestRequestBuilder(client);
            return builder.WithRequestTimeout(seconds);
        }

        public static IRestRequestBuilder WithRoute(this HttpClient client, string route)
        {
            var builder = new RestRequestBuilder(client);
            return builder.WithRoute(route);
        }

        public static IRestRequestBuilder WithQueryParam(this HttpClient client, string key, string value)
        {
            var builder = new RestRequestBuilder(client);
            return builder.WithQueryParam(key, value);
        }

        public static IRestRequestBuilder WithQueryParams(this HttpClient client, IEnumerable<KeyValuePair<string, string>> queryParams)
        {
            var builder = new RestRequestBuilder(client);
            return builder.WithQueryParams(queryParams);
        }
    }
}