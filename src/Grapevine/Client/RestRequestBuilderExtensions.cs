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
    public static class RestRequestBuilderExtensions
    {
        public static RestRequestBuilder WithAuthentication(this RestRequestBuilder builder, string scheme, string token)
        {
            builder.Request.Headers.Authorization = new AuthenticationHeaderValue(scheme, token);
            return builder;
        }

        public static RestRequestBuilder WithBasicAuthentication(this RestRequestBuilder builder, string username, string password)
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            return builder.WithBasicAuthentication(token);
        }

        public static RestRequestBuilder WithBasicAuthentication(this RestRequestBuilder builder, string token)
        {
            return builder.WithAuthentication("Basic", token);
        }

        public static RestRequestBuilder WithContent(this RestRequestBuilder builder, HttpContent content)
        {
            builder.Content = content;
            return builder;
        }

        public static RestRequestBuilder WithContent(this RestRequestBuilder builder, string content)
        {
            builder.Content = new StringContent(content);
            return builder;
        }

        public static RestRequestBuilder WithHeader(this RestRequestBuilder builder, string key, string value)
        {
            builder.Request.Headers.Add(key, value);
            return builder;
        }

        public static RestRequestBuilder WithHeaders(this RestRequestBuilder builder, IEnumerable<KeyValuePair<string, string>> headers)
        {
            foreach (var header in headers) builder.Request.Headers.Add(header.Key, header.Value);
            return builder;
        }

        public static RestRequestBuilder WithOAuthBearerToken(this RestRequestBuilder builder, string token)
        {
            return builder.WithAuthentication("Bearer", token);
        }

        public static RestRequestBuilder WithRequestTimeout(this RestRequestBuilder builder, int seconds)
        {
            builder.Timeout = TimeSpan.FromSeconds(seconds);
            return builder;
        }

        public static RestRequestBuilder WithRoute(this RestRequestBuilder builder, string route)
        {
            builder.Route = route;
            return builder;
        }

        public static RestRequestBuilder WithQueryParam(this RestRequestBuilder builder, string key, string value)
        {
            builder.QueryParams.Add(key, value);
            return builder;
        }

        public static RestRequestBuilder WithQueryParams(this RestRequestBuilder builder, IEnumerable<KeyValuePair<string, string>> queryParams)
        {
            foreach (var param in queryParams) builder.QueryParams.Add(param.Key, param.Value);
            return builder;
        }

        public static RestRequestBuilder WithQueryParams(this RestRequestBuilder builder, NameValueCollection queryParams)
        {
            foreach (string name in queryParams.Keys) builder.QueryParams.Add(name, queryParams.GetValue<string>(name));
            return builder;
        }

        public static async Task<HttpResponseMessage> DeleteAsync(this RestRequestBuilder builder)
        {
            return await builder.DeleteAsync(CancellationToken.None).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> DeleteAsync(this RestRequestBuilder builder, CancellationToken token)
        {
            return await builder.SendAsync("Delete", token).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> GetAsync(this RestRequestBuilder builder)
        {
            return await builder.GetAsync(CancellationToken.None).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> GetAsync(this RestRequestBuilder builder, CancellationToken token)
        {
            return await builder.SendAsync("Get", token).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PostAsync(this RestRequestBuilder builder)
        {
            return await builder.SendAsync("Post", CancellationToken.None).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PostAsync(this RestRequestBuilder builder, HttpContent content)
        {
            builder.Content = content;
            return await builder.SendAsync("Post", CancellationToken.None).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PostAsync(this RestRequestBuilder builder, CancellationToken token)
        {
            return await builder.SendAsync("Post", token).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PostAsync(this RestRequestBuilder builder, HttpContent content, CancellationToken token)
        {
            builder.Content = content;
            return await builder.SendAsync("Post", token).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PutAsync(this RestRequestBuilder builder)
        {
            return await builder.SendAsync("Put", CancellationToken.None).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PutAsync(this RestRequestBuilder builder, HttpContent content)
        {
            builder.Content = content;
            return await builder.SendAsync("Put", CancellationToken.None).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PutAsync(this RestRequestBuilder builder, CancellationToken token)
        {
            return await builder.SendAsync("Put", token).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PutAsync(this RestRequestBuilder builder, HttpContent content, CancellationToken token)
        {
            builder.Content = content;
            return await builder.SendAsync("Put", token).ConfigureAwait(false);
        }
    }
}