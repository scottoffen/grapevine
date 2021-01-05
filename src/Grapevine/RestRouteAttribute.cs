using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Grapevine
{
    /// <summary>
    /// <para>Method attribute for defining a RestRoute</para>
    /// <para>Targets: Method, AllowMultipe: true</para>
    /// <para>&#160;</para>
    /// <para>A method with the RestRoute attribute can have traffic routed to it by a RestServer if the request matches the assigned properties.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RestRouteAttribute : Attribute, IRouteProperties
    {
        public string Description { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        public HttpMethod HttpMethod { get; set; } = HttpMethod.Any;

        public string Name { get; set; } = string.Empty;

        public string PathInfo { get; set; } = string.Empty;

        public RestRouteAttribute()
        {
            HttpMethod = HttpMethod.Any;
        }

        public RestRouteAttribute(string httpMethod)
        {
            HttpMethod = httpMethod;
        }

        public RestRouteAttribute(string httpMethod, string pathInfo)
        {
            HttpMethod = httpMethod;
            PathInfo = pathInfo;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class HeaderAttribute : Attribute
    {
        public string Key { get; set; }
        public Regex Value { get; set; }

        public HeaderAttribute(string key, string value)
        {
            Key = key;
            Value = new Regex(value);
        }
    }

    public static class RestRouteAttributeExtensions
    {
        public static object[] GenerateRouteConstructorArguments(this RestRouteAttribute attribute, MethodInfo methodInfo, string basePath = null)
        {
            var args = new object[6];

            if (!methodInfo.IsStatic)
            {
                args[0] = methodInfo;
            }
            else
            {
                Func<IHttpContext, Task> action = async (context) => await (Task)methodInfo.Invoke(null, new object[] { context });
                args[0] = action;
            }

            args[1] = attribute.HttpMethod;

            args[2] = (string.IsNullOrWhiteSpace(basePath))
                ? attribute.PathInfo.SanitizePath()
                : $"{basePath.SanitizePath()}{attribute.PathInfo.SanitizePath()}";

            args[3] = attribute.Enabled;
            args[4] = (!string.IsNullOrWhiteSpace(attribute.Name))
                ? attribute.Name
                : $"{methodInfo.DeclaringType.Name}.{methodInfo.Name}";

            args[5] = attribute.Description;

            return args;
        }
    }
}