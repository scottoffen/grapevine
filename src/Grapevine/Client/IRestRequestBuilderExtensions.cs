using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grapevine.Client
{
    public static class IRestRequestBuilderExtensions
    {
        public static IRestRequestBuilder WithAuthentication(this IRestRequestBuilder builder, string scheme, string token)
        {
            builder.Request.Headers.Authorization = new AuthenticationHeaderValue(scheme, token);
            return builder;
        }

        public static IRestRequestBuilder WithBasicAuthentication(this IRestRequestBuilder builder, string username, string password)
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            return builder.WithBasicAuthentication(token);
        }

        public static IRestRequestBuilder WithBasicAuthentication(this IRestRequestBuilder builder, string token)
        {
            return builder.WithAuthentication("Basic", token);
        }

        public static IRestRequestBuilder WithContent(this IRestRequestBuilder builder, HttpContent content)
        {
            builder.Content = content;
            return builder;
        }

        public static IRestRequestBuilder WithContent(this IRestRequestBuilder builder, string content)
        {
            builder.Content = new StringContent(content);
            return builder;
        }

        public static IRestRequestBuilder WithHeader(this IRestRequestBuilder builder, string key, string value)
        {
            builder.Request.Headers.Add(key, value);
            return builder;
        }

        public static IRestRequestBuilder WithHeaders(this IRestRequestBuilder builder, IEnumerable<KeyValuePair<string, string>> headers)
        {
            foreach (var header in headers) builder.Request.Headers.Add(header.Key, header.Value);
            return builder;
        }

        public static IRestRequestBuilder WithOAuthBearerToken(this IRestRequestBuilder builder, string token)
        {
            return builder.WithAuthentication("Bearer", token);
        }

        public static IRestRequestBuilder WithRequestTimeout(this IRestRequestBuilder builder, int seconds)
        {
            builder.Timeout = TimeSpan.FromSeconds(seconds);
            return builder;
        }

        public static IRestRequestBuilder WithRoute(this IRestRequestBuilder builder, string route)
        {
            builder.Route = route;
            return builder;
        }

        public static IRestRequestBuilder WithQueryParam(this IRestRequestBuilder builder, string key, string value)
        {
            builder.QueryParams.Add(key, value);
            return builder;
        }

        public static IRestRequestBuilder WithQueryParams(this IRestRequestBuilder builder, IEnumerable<KeyValuePair<string, string>> queryParams)
        {
            foreach (var param in queryParams) builder.QueryParams.Add(param.Key, param.Value);
            return builder;
        }

        public static IRestRequestBuilder WithQueryParams(this IRestRequestBuilder builder, NameValueCollection queryParams)
        {
            foreach (string name in queryParams.Keys) builder.QueryParams.Add(name, queryParams.GetValue<string>(name));
            return builder;
        }

        public static async Task<HttpResponseMessage> DeleteAsync(this IRestRequestBuilder builder)
        {
            return await builder.DeleteAsync(CancellationToken.None).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> DeleteAsync(this IRestRequestBuilder builder, CancellationToken token)
        {
            return await builder.SendAsync("Delete", token).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> GetAsync(this IRestRequestBuilder builder)
        {
            return await builder.GetAsync(CancellationToken.None).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> GetAsync(this IRestRequestBuilder builder, CancellationToken token)
        {
            return await builder.SendAsync("Get", token).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PostAsync(this IRestRequestBuilder builder)
        {
            return await builder.SendAsync("Post", CancellationToken.None).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PostAsync(this IRestRequestBuilder builder, HttpContent content)
        {
            builder.Content = content;
            return await builder.SendAsync("Post", CancellationToken.None).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PostAsync(this IRestRequestBuilder builder, CancellationToken token)
        {
            return await builder.SendAsync("Post", token).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PostAsync(this IRestRequestBuilder builder, HttpContent content, CancellationToken token)
        {
            builder.Content = content;
            return await builder.SendAsync("Post", token).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PutAsync(this IRestRequestBuilder builder)
        {
            return await builder.SendAsync("Put", CancellationToken.None).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PutAsync(this IRestRequestBuilder builder, HttpContent content)
        {
            builder.Content = content;
            return await builder.SendAsync("Put", CancellationToken.None).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PutAsync(this IRestRequestBuilder builder, CancellationToken token)
        {
            return await builder.SendAsync("Put", token).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PutAsync(this IRestRequestBuilder builder, HttpContent content, CancellationToken token)
        {
            builder.Content = content;
            return await builder.SendAsync("Put", token).ConfigureAwait(false);
        }
    }
}