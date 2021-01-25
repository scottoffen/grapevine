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
        private static char[] _invalidNameChars = @"()<>@,;:\/[]?={}""".ToCharArray();

        private static char[] _invalidValueChars = @",;\""".ToCharArray();

        public override void Add(string name, string value)
        {
            ValidateName(name);
            ValidateValue(value);
            base.Add(name, value);
        }

        public new void Add(NameValueCollection c)
        {
            c.AllKeys.ToList().ForEach(k =>
            {
                ValidateName(k);
                ValidateValue(c[k]);
            });

            base.Add(c);
        }

        public override void Set(string name, string value)
        {
            ValidateName(name);
            ValidateValue(value);
            base.Set(name, value);
        }

        public override string ToString()
        {
            return Count <= 0
                ? string.Empty
                : string.Join("; ", (from key in AllKeys let value = Get(key) select Uri.EscapeDataString(key) + "=" + Uri.EscapeDataString(value)).ToArray());
        }

        private void ValidateName(string name)
        {
            if (name.HasWhiteSpace() || name.Contains(_invalidNameChars))
                throw new ArgumentOutOfRangeException($"Invalid cookie name {name}");
        }

        private void ValidateValue(string value)
        {
            if (value.HasWhiteSpace() || value.Contains(_invalidValueChars))
                throw new ArgumentOutOfRangeException($"Invalid cookie value {value}");
        }
    }
}