using System;
using System.Collections.Generic;
using System.Linq;

namespace Grapevine.Client
{
    /// <summary>
    /// Provides access the cookies for the request
    /// </summary>
    public class Cookies : Dictionary<string, string>
    {
        private static char[] _invalidNameChars = @"()<>@,;:\/[]?={}""".ToCharArray();
        private static char[] _invalidValueChars = @",;\""".ToCharArray();

        public new string this[string key]
        {
            get { return base[key]; }
            set
            {
                ValidateName(key);
                ValidateValue(value);
                base[key] = value;
            }
        }

        public new void Add(string key, string value)
        {
                ValidateName(key);
                ValidateValue(value);
                base.Add(key, value);
        }

        public override string ToString()
        {
            return Count <= 0
                ? string.Empty
                : string.Join("; ", (from key in Keys let value = base[key] select $"{key}={Uri.EscapeUriString(value)}").ToArray());
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
