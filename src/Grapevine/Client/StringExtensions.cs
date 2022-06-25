using System;
using System.Collections.Generic;
using System.Linq;

namespace Grapevine.Client
{
    internal static class StringExtensions
    {
        public static bool HasWhiteSpace(this string s)
        {
            if (s == null) throw new ArgumentNullException("s");

            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsWhiteSpace(s[i])) return true;
            }

            return false;
        }

        public static bool Contains(this string s, char[] chars)
        {
            HashSet<char> searchChars = new HashSet<char>(chars);
            return s.Any(x => searchChars.Contains(x));
        }
    }
}