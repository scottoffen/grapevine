using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Grapevine
{
    public static class InternalExtensions
    {
        private static readonly Regex ParseForParams = new Regex(@"\[(\w+)\]", RegexOptions.IgnoreCase);

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

        internal static Regex ToRegex(this string urlPattern, out List<string> patternKeys)
        {
            patternKeys = new List<string>();

            if (string.IsNullOrEmpty(urlPattern)) return RouteBase.DefaultPattern;
            if (urlPattern.StartsWith("^")) return new Regex(urlPattern);

            foreach (var val in from Match match in ParseForParams.Matches(urlPattern) select match.Groups[1].Value)
            {
                if (patternKeys.Contains(val)) throw new ArgumentException($"Repeat parameters in path info expression {urlPattern}");
                patternKeys.Add(val);
            }

            var pattern = new StringBuilder("^");

            pattern.Append(ParseForParams.IsMatch(urlPattern)
                ? ParseForParams.Replace(urlPattern, "([^/]+)")
                : urlPattern);

            if (!urlPattern.EndsWith("$")) pattern.Append("$");

            return new Regex(pattern.ToString());
        }
    }
}