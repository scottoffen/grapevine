using System;
using System.Linq;

namespace Grapevine
{
    public static class InternalExtensions
    {
        internal static string SanitizePath(this string path)
        {
            var basepath = path?.Trim().TrimEnd('/').TrimStart('/').Trim() ?? string.Empty;
            return string.IsNullOrWhiteSpace(basepath) ? basepath : $"/{basepath}";
        }

        internal static bool StartsWith(this string value, string[] values)
        {
            foreach (var x in values)
            {
                if (value.StartsWith(x, StringComparison.CurrentCultureIgnoreCase)) return true;
            }

            return false;
        }

        internal static bool IsEven(this int value)
        {
            return ((value % 2) == 0);
        }

        internal static bool HasWhiteSpace(this string s)
        {
            if (s == null) throw new ArgumentNullException("s");

            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsWhiteSpace(s[i])) return true;
            }

            return false;
        }

        internal static bool Contains(this string s, char[] chars)
        {
            foreach (var c in chars)
            {
                if (s.Contains<char>(c)) return true;
            }

            return false;
        }
    }
}