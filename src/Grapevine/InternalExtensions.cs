using System;

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
    }
}