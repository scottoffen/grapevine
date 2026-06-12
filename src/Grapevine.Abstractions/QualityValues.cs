namespace Grapevine;

/// <summary>
/// Parses and sorts HTTP quality value headers according to client preference ordering
/// as defined in RFC 7231.
/// </summary>
/// <remarks>
/// Quality value headers such as <c>Accept</c>, <c>Accept-Language</c>,
/// <c>Accept-Encoding</c>, and <c>Accept-Charset</c> allow clients to express
/// a preference ordering for multiple values using a quality factor between 0 and 1.
/// For example: <c>text/html, application/json;q=0.9, */*;q=0.8</c>.
/// </remarks>
public static class QualityValues
{
    /// <summary>
    /// Parses a quality value header string into a flat ordered list of values,
    /// sorted from most preferred to least preferred. Within the same quality
    /// factor, more specific values rank above less specific ones.
    /// </summary>
    /// <param name="header">The raw header value string to parse.</param>
    /// <returns>
    /// An ordered list of values from most preferred to least preferred.
    /// </returns>
    public static IList<string> Parse(string header)
    {
        var values = new List<string>(8);

        foreach (var item in GroupByQualityFactor(header))
            values.AddRange(SortBySpecificity(item.Value));

        return values;
    }

    /// <summary>
    /// Splits a quality value header string into groups keyed by quality factor,
    /// sorted from highest to lowest quality factor. Entries without an explicit
    /// quality factor default to <c>1.0</c>.
    /// </summary>
    /// <param name="value">The raw header value string to parse.</param>
    /// <returns>
    /// A <see cref="SortedDictionary{TKey,TValue}"/> mapping quality factors to
    /// their associated values, ordered from highest to lowest quality factor.
    /// </returns>
    internal static SortedDictionary<decimal, List<string>> GroupByQualityFactor(string value)
    {
        var factors = new SortedDictionary<decimal, List<string>>(
            Comparer<decimal>.Create((a, b) => b.CompareTo(a)));

        foreach (var entry in value.Split(','))
        {
            var itemFactorPair = entry.Trim().Split(new string[] { ";q=" }, StringSplitOptions.None);
            var item = itemFactorPair[0];
            var factor = itemFactorPair.Length == 2
                ? decimal.TryParse(itemFactorPair[1], out var parsed) ? parsed : 1m
                : 1m;

            if (!factors.ContainsKey(factor)) factors.Add(factor, new List<string>(4));
            factors[factor].Add(item);
        }

        return factors;
    }

    /// <summary>
    /// Sorts a list of values by specificity. Fully specified values (e.g.
    /// <c>text/html</c>) rank above partially specified values (e.g. <c>text/*</c>),
    /// which rank above wildcards (e.g. <c>*/*</c>).
    /// </summary>
    /// <param name="values">The list of values to sort.</param>
    /// <returns>
    /// A new list containing the same values sorted from most specific to least specific.
    /// </returns>
    internal static IList<string> SortBySpecificity(IList<string> values)
    {
        if (values.Count == 1) return values;

        var totally = new List<string>(values.Count);
        var partial = new List<string>(values.Count);
        var nonspec = new List<string>(values.Count);

        foreach (var value in values)
        {
            if (value == "*/*")
                nonspec.Add(value);
            else if (value.EndsWith("/*", StringComparison.Ordinal))
                partial.Add(value);
            else
                totally.Add(value);
        }

        partial.AddRange(nonspec);
        totally.AddRange(partial);

        return totally;
    }
}