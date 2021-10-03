using System.Collections.Generic;

namespace Grapevine
{
    public class GlobalResponseHeaders
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public bool Suppress { get; set; }

        public GlobalResponseHeaders(string name, string defaultValue, bool suppress = false)
        {
            Name = name;
            Value = defaultValue;
            Suppress = suppress;
        }
    }

    public static class GlobalResponseHeaderExtensions
    {
        public static void Add(this IList<GlobalResponseHeaders> headers, string key, string value)
        {
            headers.Add(new GlobalResponseHeaders(key, value));
        }
    }
}