using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace Grapevine
{
    /// <summary>
    /// Utilities for finding open ports on the local machine
    /// </summary>
    public static class PortFinder
    {
        /// <summary>
        /// Represents the smallest port number possible
        /// </summary>
        public static int FirstPort { get; } = 1;

        /// <summary>
        /// Represents the largest port number possible
        /// </summary>
        public static int LastPort { get; } = 65535;

        private static string OutOfRangeMsg { get; } = $"must be an integer between {FirstPort} and {LastPort}.";

        private static string FindLocalOpenPort(int idx0, int idx1)
        {
            return (idx0 > idx1)
                 ? FindPreviousLocalOpenPort(idx0, idx1)
                 : FindNextLocalOpenPort(idx0, idx1);
        }

        public static string FindNextLocalOpenPort()
        {
            return FindNextLocalOpenPort(FirstPort, LastPort);
        }

        public static string FindPreviousLocalOpenPort()
        {
            return FindNextLocalOpenPort(LastPort, FirstPort);
        }

        public static string FindNextLocalOpenPort(int startIndex)
        {
            return FindNextLocalOpenPort(startIndex, LastPort);
        }

        public static string FindPreviousLocalOpenPort(int endIndex)
        {
            return FindPreviousLocalOpenPort(FirstPort, endIndex);
        }

        public static string FindNextLocalOpenPort(int startIndex, int endIndex)
        {
            if (!startIndex.IsInRange())
                throw new ArgumentOutOfRangeException(nameof(startIndex), OutOfRangeMsg);

            if (!endIndex.IsInRange())
                throw new ArgumentOutOfRangeException(nameof(endIndex), OutOfRangeMsg);

            var inUse = IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Select(l => l.Port)
                .ToList();

            for (var i = Math.Min(startIndex, endIndex); i <= Math.Max(startIndex, endIndex); i++)
            {
                if (!inUse.Contains(i)) return i.ToString();
            }

            throw new IndexOutOfRangeException($"No local open ports found in range {Math.Min(startIndex, endIndex)} - {Math.Max(startIndex, endIndex)}!");
        }

        public static string FindPreviousLocalOpenPort(int startIndex, int endIndex)
        {
            if (!startIndex.IsInRange())
                throw new ArgumentOutOfRangeException(nameof(startIndex), OutOfRangeMsg);

            if (!endIndex.IsInRange())
                throw new ArgumentOutOfRangeException(nameof(endIndex), OutOfRangeMsg);

            var inUse = IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Select(l => l.Port)
                .ToList();

            for (var i = Math.Max(startIndex, endIndex); i >= Math.Min(startIndex, endIndex); i--)
            {
                if (!inUse.Contains(i)) return i.ToString();
            }

            throw new IndexOutOfRangeException($"No local open ports found in range {Math.Min(startIndex, endIndex)} - {Math.Max(startIndex, endIndex)}!");
        }

        private static bool IsInRange(this int value) => (value >= FirstPort && value <= LastPort);
    }
}