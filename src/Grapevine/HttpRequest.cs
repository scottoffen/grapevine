using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace Grapevine
{
    public abstract class HttpRequestBase : IHttpRequest
    {
        public HttpListenerRequest Advanced { get; }

        public string[] AcceptTypes => Advanced.AcceptTypes;

        public Encoding ContentEncoding => Advanced.ContentEncoding;

        public long ContentLength64 => Advanced.ContentLength64;

        public string ContentType => Advanced.ContentType;

        public CookieCollection Cookies => Advanced.Cookies;

        public NameValueCollection Headers => Advanced.Headers;

        public HttpMethod HttpMethod => Advanced.HttpMethod;

        public Stream InputStream => Advanced.InputStream;

        public abstract string Name { get; }

        public abstract string Endpoint { get; }

        public abstract Dictionary<string, string> PathParameters { get; set; }

        public NameValueCollection QueryString => Advanced.QueryString;

        public string RawUrl => Advanced.RawUrl;

        public IPEndPoint RemoteEndPoint => Advanced.RemoteEndPoint;

        public Uri Url => Advanced.Url;

        public Uri UrlReferrer => Advanced.UrlReferrer;

        public string UserAgent => Advanced.UserAgent;

        public string UserHostAddress => Advanced.UserHostAddress;

        public string UserHostname => Advanced.UserHostName;

        public string[] UserLanguages => Advanced.UserLanguages;

        public HttpRequestBase(HttpListenerRequest request)
        {
            Advanced = request;
        }
    }

    public class HttpRequest : HttpRequestBase, IHttpRequest
    {
        public override string Name => $"{HttpMethod} {Endpoint}";

        public override string Endpoint { get; }

        public override Dictionary<string, string> PathParameters { get; set; } = new Dictionary<string, string>();

        public HttpRequest(HttpListenerRequest request) : base(request)
        {
            Endpoint = request.RawUrl.Split(new[] { '?' }, 2)[0].TrimEnd('/');
        }
    }
}