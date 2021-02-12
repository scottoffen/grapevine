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
        public string Description { get; set; }

        public bool Enabled { get; set; }

        public readonly Dictionary<string, Regex> HeaderConditions = new Dictionary<string, Regex>();

        public HttpMethod HttpMethod { get; set; }

        public string Name { get; set; }

        public IRouteTemplate RouteTemplate { get; set; }

        protected RouteBase(HttpMethod httpMethod, IRouteTemplate routeTemplate, bool enabled, string name, string description)
        {
            HttpMethod = httpMethod;
            RouteTemplate = routeTemplate;
            Name = name;
            Description = description;
            Enabled = enabled;
        }

        public abstract Task InvokeAsync(IHttpContext context);

        public virtual bool IsMatch(IHttpContext context)
        {
            if (!Enabled || !context.Request.HttpMethod.Equivalent(HttpMethod) || !RouteTemplate.Matches(context.Request.Endpoint)) return false;

            foreach (var condition in HeaderConditions)
            {
                var value = context.Request.Headers.Get(condition.Key) ?? string.Empty;
                if (condition.Value.IsMatch(value)) continue;
                return false;
            }

            return true;
        }

        public virtual IRoute WithHeader(string header, Regex pattern)
        {
            HeaderConditions[header] = pattern;
            return this;
        }
    }

    public class Route : RouteBase, IRoute
    {
        protected Func<IHttpContext, Task> RouteAction;

        public Route(Func<IHttpContext, Task> action, HttpMethod method, string routePattern, bool enabled = true, string name = null, string description = null)
            : this(action, method, new RouteTemplate(routePattern), enabled, name, description) { }

        public Route(Func<IHttpContext, Task> action, HttpMethod method, Regex routePattern, bool enabled = true, string name = null, string description = null)
            : this(action, method, new RouteTemplate(routePattern), enabled, name, description) { }

        public Route(Func<IHttpContext, Task> action, HttpMethod method, IRouteTemplate routeTemplate, bool enabled = true, string name = null, string description = null)
            : base(method, routeTemplate, enabled, name, description)
        {
            RouteAction = action;
            Name = (string.IsNullOrWhiteSpace(name))
                ? action.Method.Name
                : name;
        }

        public override async Task InvokeAsync(IHttpContext context)
        {
            if (!Enabled) return;
            context.Request.PathParameters = RouteTemplate.ParseEndpoint(context.Request.Endpoint);
            await RouteAction(context).ConfigureAwait(false);
        }
    }

    public class Route<T> : RouteBase, IRoute
    {
        protected Func<T, IHttpContext, Task> RouteAction;

        public Route(MethodInfo methodInfo, HttpMethod method, string routePattern, bool enabled = true, string name = null, string description = null)
            : this(methodInfo, method, new RouteTemplate(routePattern), enabled, name, description) { }

        public Route(MethodInfo methodInfo, HttpMethod method, Regex routePattern, bool enabled = true, string name = null, string description = null)
            : this(methodInfo, method, new RouteTemplate(routePattern), enabled, name, description) { }

        public Route(MethodInfo methodInfo, HttpMethod method, IRouteTemplate routeTemplate, bool enabled = true, string name = null, string description = null) : base(method, routeTemplate, enabled, name, description)
        {
            RouteAction = (Func<T, IHttpContext, Task>)Delegate.CreateDelegate(typeof(Func<T, IHttpContext, Task>), null, methodInfo);
            if (string.IsNullOrWhiteSpace(Name)) Name = methodInfo.Name;
        }

        public override async Task InvokeAsync(IHttpContext context)
        {
            if (!Enabled) return;
            context.Request.PathParameters = RouteTemplate.ParseEndpoint(context.Request.Endpoint);
            await RouteAction(context.Services.GetRequiredService<T>(), context).ConfigureAwait(false);
        }
    }
}