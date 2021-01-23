using System;
using System.Collections.Specialized;
using System.Linq;

namespace Grapevine.Client
{
    /// <summary>
    /// Provides access the cookies for the request
    /// </summary>
    public class Cookies : NameValueCollection
    {
        public override string ToString()
        {
            return Count <= 0
                ? string.Empty
                : string.Join("; ", (from key in AllKeys let value = Get(key) select Uri.EscapeDataString(key) + "=" + Uri.EscapeDataString(value)).ToArray());
        }
    }
}