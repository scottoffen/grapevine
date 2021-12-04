using System;

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
            foreach (var x in values)
            {
                if (value.StartsWith(x, StringComparison.CurrentCultureIgnoreCase)) return true;
            }

            return false;
        }
    }
}