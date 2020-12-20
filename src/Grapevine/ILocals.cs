using System.Collections.Concurrent;

namespace Grapevine
{
    public interface ILocals
    {
        bool Contains(string key);

         object Get(string key);

         void Set(string key, object val);
    }

    public static class ILocalsExtensions
    {
        public static T GetAs<T>(this ILocals locals, string key)
        {
            var val = locals.Get(key);
            return (T) val;
        }
    }

    public abstract class Locals : ILocals
    {
        private ConcurrentDictionary<string, object> _properties;

        protected ConcurrentDictionary<string, object> Properties
        {
            get
            {
                if (_properties == null) _properties = new ConcurrentDictionary<string, object>();
                return _properties;
            }
        }

        public bool Contains(string key)
        {
            return Properties.ContainsKey(key);
        }

        public object Get(string key)
        {
            return (Contains(key))
                ? Properties[key]
                : null;
        }

        public void Set(string key, object val)
        {
            Properties[key] = val;
        }
    }
}