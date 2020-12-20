using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Grapevine
{
    public abstract class RouteBase : IRoute
    {
        public static readonly Regex DefaultPattern = new Regex(@"^.*$");
        public List<string> PatternKeys = new List<string>();
        public readonly Dictionary<string, Regex> MatchesOn = new Dictionary<string, Regex>();

        public string Description { get; set; }
        public bool Enabled { get; set; }

        public HttpMethod HttpMethod { get; protected set; }

        public string Name { get; protected set; }

        public Regex UrlPattern { get; protected set; }

        protected RouteBase(HttpMethod httpMethod, Regex urlPattern, bool enabled, string name, string description) : this(httpMethod, enabled, name, description)
        {
            UrlPattern = urlPattern;
        }

        protected RouteBase(HttpMethod httpMethod, string urlPattern, bool enabled, string name, string description) : this(httpMethod, enabled, name, description)
        {
            UrlPattern = (!string.IsNullOrEmpty(urlPattern))
                ? urlPattern.ToRegex(out PatternKeys)
                : DefaultPattern;
        }

        private RouteBase(HttpMethod httpMethod, bool enabled, string name, string description)
        {
            HttpMethod = httpMethod;
            Name = name;
            Description = description;
            Enabled = enabled;
        }

        public abstract Task InvokeAsync(IHttpContext context);

        public virtual bool Matches(IHttpContext context)
        {
            if (!Enabled || !context.Request.HttpMethod.Equivalent(HttpMethod) || !UrlPattern.IsMatch(context.Request.PathInfo)) return false;

            foreach (var condition in MatchesOn)
            {
                var value = context.Request.Headers.Get(condition.Key) ?? string.Empty;
                if (condition.Value.IsMatch(value)) continue;
                return false;
            }

            return true;
        }

        public virtual IRoute MatchOn(string header, Regex pattern)
        {
            MatchesOn[header] = pattern;
            return this;
        }

        /// <summary>
        /// Gets a dictionary of parameter values from the PathInfo of the request
        /// </summary>
        /// <param name="pathinfo"></param>
        /// <returns></returns>
        public Dictionary<string, string> ParseParams(string pathinfo)
        {
            var parsed = new Dictionary<string, string>();
            var idx = 0;

            var matches = UrlPattern.Matches(pathinfo)[0].Groups;
            for (int i = 1; i < matches.Count; i++)
            {
                var key = PatternKeys.Count > 0 && PatternKeys.Count > idx ? PatternKeys[idx] : $"p{idx}";
                parsed.Add(key, matches[i].Value);
                idx++;
            }

            return parsed;
        }
    }

    public class Route : RouteBase, IRoute
    {
        protected Func<IHttpContext, Task> RouteAction;

        public Route(Func<IHttpContext, Task> action, HttpMethod method, string urlPattern, bool enabled = true, string name = null, string description = null) : base(method, urlPattern, enabled, name, description)
        {
            RouteAction = action;
            if (string.IsNullOrWhiteSpace(Name)) Name = action.Method.Name;
        }

        public Route(Func<IHttpContext, Task> action, HttpMethod method, Regex urlPattern, bool enabled = true, string name = null, string description = null) : base(method, urlPattern, enabled, name, description)
        {
            RouteAction = action;
            if (string.IsNullOrWhiteSpace(Name)) Name = action.Method.Name;
        }

        public override async Task InvokeAsync(IHttpContext context)
        {
            if (!Enabled) return;
            context.Request.PathParameters = ParseParams(context.Request.PathInfo);
            await RouteAction(context).ConfigureAwait(false);
        }
    }

    public class Route<T> : RouteBase, IRoute
    {
        protected Func<T, IHttpContext, Task> RouteAction;

        public Route(MethodInfo methodInfo, HttpMethod method, string urlPattern, bool enabled = true, string name = null, string description = null) : base(method, urlPattern, enabled, name, description)
        {
            RouteAction = (Func<T, IHttpContext, Task>)Delegate.CreateDelegate(typeof(Func<T, IHttpContext, Task>), null, methodInfo);
            if (string.IsNullOrWhiteSpace(Name)) Name = methodInfo.Name;
        }

        public Route(MethodInfo methodInfo, HttpMethod method, Regex urlPattern, bool enabled = true, string name = null, string description = null) : base(method, urlPattern, enabled, name, description)
        {
            RouteAction = (Func<T, IHttpContext, Task>)Delegate.CreateDelegate(typeof(Func<T, IHttpContext, Task>), null, methodInfo);
            if (string.IsNullOrWhiteSpace(Name)) Name = methodInfo.Name;
        }

        public override async Task InvokeAsync(IHttpContext context)
        {
            if (!Enabled) return;
            context.Request.PathParameters = ParseParams(context.Request.PathInfo);
            await RouteAction(context.Services.GetRequiredService<T>(), context).ConfigureAwait(false);
        }
    }
}