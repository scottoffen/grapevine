using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Grapevine
{
    public delegate string RouteConstraintResolver(string args);

    public interface IRouteTemplate
    {
        Regex Pattern { get; }

        List<string> PatternKeys { get; }

        bool Matches(string endpoint);

        IDictionary<string, string> ParseEndpoint(string endpoint);
    }

    public class RouteTemplate : IRouteTemplate
    {
        private static readonly Regex Default = new Regex(@"^.*$");

        public Regex Pattern { get; set; } = new Regex(@"^.*$");

        public List<string> PatternKeys { get; set; } = new List<string>();

        public RouteTemplate() { }

        public RouteTemplate(string pattern)
        {
            Pattern = ConvertToRegex(pattern, out var patternKeys);
            PatternKeys = patternKeys;
        }

        public RouteTemplate(Regex pattern, List<string> patternKeys = null)
        {
            Pattern = pattern;
            if (patternKeys != null) PatternKeys = patternKeys;
        }

        public bool Matches(string endpoint) => Pattern.IsMatch(endpoint);

        public IDictionary<string, string> ParseEndpoint(string endpoint)
        {
            var parsed = new Dictionary<string, string>();
            var idx = 0;

            var matches = Pattern.Matches(endpoint)[0].Groups;
            for (int i = 1; i < matches.Count; i++)
            {
                var key = (PatternKeys?.Count > 0 && PatternKeys?.Count > idx)
                    ? PatternKeys[idx]
                    : $"p{idx}";

                parsed.Add(key, matches[i].Value);
                idx++;
            }

            return parsed;
        }

        public static Regex ConvertToRegex(string pattern, out List<string> patternKeys)
        {
            patternKeys = new List<string>();

            if (string.IsNullOrEmpty(pattern)) return Default;
            if (pattern.StartsWith("^")) return new Regex(pattern);

            var builder = new StringBuilder("(?i)^");
            var sections = pattern.SanitizePath() // Ensures the string begins with '/'
                .TrimEnd('$')                     // Removes any trailing '$'
                .Split(new char[] { '{', '}' });  // splits into sections

            for (var i = 0; i < sections.Length; i++)
            {
                if (i.IsEven())
                {
                    // Even sections don't contain constraints
                    builder.Append(sections[i]);
                }
                else
                {
                    var constraints = sections[i].Split(':').ToList();
                    patternKeys.Add(constraints[0]);
                    constraints.RemoveAt(0);
                    builder.Append(RouteConstraints.Resolve(constraints));
                }
            }

            builder.Append("$");
            return new Regex(builder.ToString());
        }
    }

    internal static class RouteConstraints
    {
        public static readonly string DefaultPattern = "([^/]+)";

        private static readonly Dictionary<string, RouteConstraintResolver> _resolvers = new Dictionary<string, RouteConstraintResolver>();

        private static readonly List<string> _protectedKeys;

        static RouteConstraints()
        {
            _resolvers.Add("alpha", AlphaResolver);
            _resolvers.Add("alphanum", AlphaNumericResolver);
            _resolvers.Add("guid", GuidResolver);
            _resolvers.Add("num", NumericResolver);
            _resolvers.Add("string", StringResolver);

            _protectedKeys = _resolvers.Keys.ToList();
        }

        public static void AddResolver(string key, RouteConstraintResolver resolver)
        {
            if (_protectedKeys.Contains(key)) throw new ArgumentException($"Cannot override protected resolver {key}");
            _resolvers[key] = resolver;
        }

        public static string Resolve(List<string> constraints)
        {
            if (constraints.Count == 0) return DefaultPattern;

            var constraint = (constraints[0].Contains('('))
                ? constraints[0]
                : (constraints.Count > 1 && constraints[1].Contains('('))
                    ? constraints[1]
                    : string.Empty;

            var resolver = _resolvers.ContainsKey(constraints[0])
                ? _resolvers[constraints[0]]
                : _resolvers["string"];

            return resolver(constraint);
        }

        private static string AlphaResolver(string args)
        {
            var quantifier = LengthResolver(args);
            return $"([a-zA-Z]{quantifier})";
        }

        private static string AlphaNumericResolver(string args)
        {
            var quantifier = LengthResolver(args);
            return $"(\\w{quantifier})";
        }

        private static string GuidResolver(string args)
        {
            return @"[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?";
        }

        private static string NumericResolver(string args)
        {
            var quantifier = LengthResolver(args);
            return $"(\\d{quantifier})";
        }

        private static string StringResolver(string args)
        {
            var quantifier = LengthResolver(args);
            return $"([^/]{quantifier})";
        }

        public static string LengthResolver(string args)
        {
            if (string.IsNullOrWhiteSpace(args)) return "+";
            var sections = args.Split(new char[] { '(', ')' });

            if (sections.Length < 2) throw new ArgumentException($"Length parameters not specified in {args}");

            var length = string.Empty;
            var range = sections[1].Split(',');

            switch (sections[0].ToLower())
            {
                case "minlength":
                    length = "{" + Int32.Parse(range[0]) + ",}";
                    break;
                case "maxlength":
                    length = "{1," + Int32.Parse(range[0]) + "}";
                    break;
                case "length":
                    length = (range.Length == 2)
                        ? "{" + Int32.Parse(range[0]) + "," + Int32.Parse(range[1]) + "}"
                        : "{" + Int32.Parse(range[0]) + "}";
                    break;
                default:
                    throw new ArgumentException($"Invalid length parameter specified in {args}");
            }

            return length;
        }
    }
}
