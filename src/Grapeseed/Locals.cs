using System.Collections.Concurrent;

namespace Grapevine
{
#if NETSTANDARD2_0
    public class Locals : ConcurrentDictionary<object, object> { }
#else
    #nullable enable
    public class Locals : ConcurrentDictionary<object, object?> { }
    #nullable restore
#endif

    public static class LocalExtensions
    {
        public static object Get(this Locals l, object key)
        {
            return l.TryGetValue(key, out object value)
                ? value
                : null;
        }

        public static T GetAs<T>(this Locals l, object key)
        {
            return (T) l.Get(key);
        }

        public static T GetOrAddAs<T>(this Locals l, object key, object value)
        {
            return (T) l.GetOrAdd(key, value);
        }
    }
}