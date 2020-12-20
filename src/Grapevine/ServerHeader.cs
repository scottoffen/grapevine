using System.Linq;
using System.Net;
using System.Reflection;

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
            Value = Value;
            Suppress = suppress;
        }
    }
}