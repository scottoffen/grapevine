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
            set { base[ValidateName(key)] = ValidateValue(value); }
        }

        public new void Add(string key, string value)
        {
            base.Add(ValidateName(key), ValidateValue(value));
        }

        public void AddObject(string key, object value)
        {
            base.Add(ValidateName(key), ValidateValue(value.ToString()));
        }

#if !NETSTANDARD2_0

        public new bool TryAdd(string key, string value)
        {
            return base.TryAdd(ValidateName(key), ValidateValue(value));
        }

        public bool TryAddObject(string key, object value)
        {
            return base.TryAdd(ValidateName(key), ValidateValue(value.ToString()));
        }

#endif

        public override string ToString()
        {
            return Count <= 0
                ? string.Empty
                : string.Join("; ", (from key in Keys let value = base[key] select $"{key}={Uri.EscapeDataString(value)}").ToArray());
        }

        private string ValidateName(string name)
        {
            if (name.HasWhiteSpace() || name.Contains(_invalidNameChars))
                throw new ArgumentOutOfRangeException($"Invalid cookie name {name}");

            return name;
        }

        private string ValidateValue(string value)
        {
            if (value.HasWhiteSpace() || value.Contains(_invalidValueChars))
                throw new ArgumentOutOfRangeException($"Invalid cookie value {value}");

            return value;
        }
    }
}
