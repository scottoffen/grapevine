using System;
using System.Linq;

namespace Grapevine
{
    internal static class StringExtensions
    {
        public static string SanitizePath(this string path)
        {
            var basepath = path?.Trim().TrimEnd('/').TrimStart('/').Trim() ?? string.Empty;
            return string.IsNullOrWhiteSpace(basepath) ? basepath : $"/{basepath}";
        }

        public static bool StartsWith(this string value, string[] values)
        {
            return values.Any(x => value.StartsWith(x, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}