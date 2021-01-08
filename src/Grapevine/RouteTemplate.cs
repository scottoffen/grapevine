using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Grapevine
{
    public interface IRouteTemplate
    {
        Regex Pattern { get; }

        List<string> PatternKeys { get; }

        bool Matches(string endpoint);

        IDictionary<string, string> Parse(string endpoint);
    }

    public class RouteTemplate : IRouteTemplate
    {
        private static readonly Regex ParseForParams = new Regex(@"\{(\w+)\}", RegexOptions.IgnoreCase);
        private static readonly Regex Default = new Regex(@"^.*$");

        public Regex Pattern { get; set; } = new Regex(@"^.*$");

        public List<string> PatternKeys { get; set; } = new List<string>();

        public RouteTemplate() { }

        public RouteTemplate(string pattern)
        {
            Pattern = ParseString(pattern, out var patternKeys);
            PatternKeys = patternKeys;
        }

        public RouteTemplate(Regex pattern, List<string> patternKeys = null)
        {
            Pattern = pattern;
            if (patternKeys != null) PatternKeys = patternKeys;
        }

        public bool Matches(string endpoint) => Pattern.IsMatch(endpoint);

        public IDictionary<string, string> Parse(string endpoint)
        {
            var parsed = new Dictionary<string, string>();
            var idx = 0;

            var matches = Pattern.Matches(endpoint)[0].Groups;
            for (int i = 1; i < matches.Count; i++)
            {
                var key = PatternKeys.Count > 0 && PatternKeys.Count > idx ? PatternKeys[idx] : $"p{idx}";
                parsed.Add(key, matches[i].Value);
                idx++;
            }

            return parsed;
        }

        public static Regex ParseString(string pattern, out List<string> patternKeys)
        {
            patternKeys = new List<string>();

            if (string.IsNullOrEmpty(pattern)) return Default;
            if (pattern.StartsWith("^")) return new Regex(pattern);

            foreach (var val in from Match match in ParseForParams.Matches(pattern) select match.Groups[1].Value)
            {
                if (patternKeys.Contains(val)) throw new ArgumentException($"Repeat parameters in path info expression {pattern}");
                patternKeys.Add(val);
            }

            var strRegex = new StringBuilder("^");

            strRegex.Append(ParseForParams.IsMatch(pattern)
                ? ParseForParams.Replace(pattern, "([^/]+)")
                : pattern
            );

            if (!pattern.EndsWith("$")) strRegex.Append("$");

            return new Regex(strRegex.ToString());
        }
    }
}
