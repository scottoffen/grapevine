using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Grapevine
{
    public static class QualityValues
    {
        private static Regex[] specificities = new Regex[2]
        {
            new Regex("[^*]+"),       // totally specific
            new Regex(@"^[^*/]/\*$"), // partially specific
        };

        public static IList<string> Parse(string header)
        {
            var values = new List<string>();

            foreach(var item in GroupByQualityFactor(header))
                values.AddRange(SortBySpecificity(item.Value));

            return values;
        }

        public static SortedDictionary<decimal, List<string>> GroupByQualityFactor(string value)
        {
            SortedDictionary<decimal, List<string>> factors = new SortedDictionary<decimal, List<string>>();
            foreach(var entry in value.Split(','))
            {
                var itemFactorPair = entry.Trim().Split(new string[]{";q="}, StringSplitOptions.None);
                var item = itemFactorPair[0];
                var factor = (itemFactorPair.Length == 2)
                    ? Convert.ToDecimal(itemFactorPair[1])
                    : Convert.ToDecimal(1);

                if (!factors.ContainsKey(factor)) factors.Add(factor, new List<string>());
                factors[factor].Add(item);
            }

            return factors;
        }

        public static IList<string> SortBySpecificity(IList<string> values)
        {
            if (values.Count == 1) return values;

            var totally = new List<string>();
            var partial = new List<string>();
            var nonspec = new List<string>();

            foreach (var value in values)
            {
                if (specificities[0].IsMatch(value)) totally.Add(value);
                else if(specificities[1].IsMatch(value)) partial.Add(value);
                else nonspec.Add(value);
            }

            partial.AddRange(nonspec);
            totally.AddRange(partial);

            return totally;
        }
    }
}