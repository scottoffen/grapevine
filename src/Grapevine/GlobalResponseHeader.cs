using System.Collections.Generic;

namespace Grapevine
{
    public class GlobalResponseHeader
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public bool Suppress { get; set; }

        public GlobalResponseHeader(string name, string defaultValue, bool suppress = false)
        {
            Name = name;
            Value = defaultValue;
            Suppress = suppress;
        }
    }

    public static class GlobalResponseHeaderExtensions
    {
        public static void Add(this IList<GlobalResponseHeader> headers, string key, string value)
        {
            headers.Add(new GlobalResponseHeader(key, value));
        }
    }
}