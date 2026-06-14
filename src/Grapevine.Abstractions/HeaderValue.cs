using System.Diagnostics;
using System.Globalization;

namespace Grapevine;

/// <summary>
/// Represents a single HTTP header value with an optional quality factor,
/// as defined in RFC 7231.
/// </summary>
/// <remarks>
/// <para>
/// Quality factors (q-values) express a preference ordering for multiple
/// values within headers such as <c>Accept</c>, <c>Accept-Language</c>,
/// and <c>Accept-Encoding</c>. The quality factor ranges from 0 to 1,
/// where 1 is the highest preference and 0 means "not acceptable."
/// </para>
/// <para>
/// Instances are compared first by descending quality factor, then by
/// ascending value, enabling correct RFC 7231 preference ordering via
/// standard sort operations.
/// </para>
/// <para>
/// For MIME-type headers that require specificity ordering (e.g.
/// <c>text/html</c> ranks above <c>text/*</c>, which ranks above
/// <c>*/*</c>), use <see cref="FromMimeType"/> to construct instances
/// with the appropriate weight already applied.
/// </para>
/// </remarks>
[DebuggerDisplay("{ToString()}")]
public class HeaderValue : IEquatable<HeaderValue>, IComparable<HeaderValue>
{
    /// <summary>
    /// The weight used for fully specified MIME types (e.g. <c>text/html</c>).
    /// </summary>
    internal static readonly int WeightSpecific = 2;

    /// <summary>
    /// The weight used for wildcard-subtype MIME types (e.g. <c>text/*</c>).
    /// </summary>
    internal static readonly int WeightPartialWildcard = 1;

    /// <summary>
    /// The weight used for full-wildcard MIME types (<c>*/*</c>) and all
    /// non-MIME header values.
    /// </summary>
    internal static readonly int WeightWildcard = 0;

    /// <summary>
    /// Gets the string value of the header entry.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the optional quality factor for this header value, or
    /// <see langword="null"/> if no quality factor was specified.
    /// </summary>
    /// <remarks>
    /// When no quality factor is specified, the effective quality is 1.0
    /// per RFC 7231. Use <see cref="EffectiveQuality"/> to get the resolved value.
    /// </remarks>
    public double? Quality { get; }

    /// <summary>
    /// Gets the effective quality factor for this header value. Returns
    /// <see cref="Quality"/> if set, or <c>1.0</c> if no quality factor
    /// was specified.
    /// </summary>
    public double EffectiveQuality => Quality ?? 1.0;

    /// <summary>
    /// Gets the specificity weight used for tiebreaking during sort operations.
    /// </summary>
    /// <remarks>
    /// For general header values this is always 0. For MIME-type values constructed
    /// via <see cref="FromMimeType"/>, this reflects specificity: fully specified
    /// types outrank wildcard-subtype types, which outrank full wildcards.
    /// </remarks>
    internal int Weight { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="HeaderValue"/> with the specified
    /// value, optional quality factor, and optional specificity weight.
    /// </summary>
    /// <param name="value">The header value string. Must not be <see langword="null"/>.</param>
    /// <param name="quality">
    /// The optional quality factor. Must be between 0 and 1 inclusive if provided.
    /// </param>
    /// <param name="weight">
    /// The optional specificity weight used as a tiebreaker during sort operations.
    /// Defaults to 0. Use <see cref="FromMimeType"/> to set this automatically for
    /// MIME-type values.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="quality"/> is outside the range [0, 1].
    /// </exception>
    public HeaderValue(string value, double? quality = null, int weight = 0)
    {
        Value = value?.Trim() ?? throw new ArgumentNullException(nameof(value));

        if (quality.HasValue && (quality.Value < 0 || quality.Value > 1))
            throw new ArgumentOutOfRangeException(nameof(quality), "Quality must be between 0 and 1.");

        Quality = quality;
        Weight = weight;
    }

    /// <summary>
    /// Creates a <see cref="HeaderValue"/> for a MIME-type string, automatically
    /// computing the specificity weight used for RFC 7231 content negotiation ordering.
    /// </summary>
    /// <param name="value">
    /// The MIME-type string (e.g. <c>text/html</c>, <c>text/*</c>, <c>*/*</c>).
    /// </param>
    /// <param name="quality">
    /// The optional quality factor. Must be between 0 and 1 inclusive if provided.
    /// </param>
    /// <returns>
    /// A <see cref="HeaderValue"/> with the specificity weight set according to
    /// the structure of the MIME-type string.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Specificity weights are assigned as follows:
    /// </para>
    /// <para>
    /// Fully specified types (e.g. <c>text/html</c>) receive the highest weight.
    /// Wildcard-subtype types (e.g. <c>text/*</c>) receive a middle weight.
    /// Full wildcards (<c>*/*</c>) receive the lowest weight.
    /// </para>
    /// </remarks>
    public static HeaderValue FromMimeType(string value, double? quality = null)
    {
        var weight = ComputeMimeWeight(value);
        return new HeaderValue(value, quality, weight);
    }

    /// <summary>
    /// Returns a string representation of this header value in the format
    /// <c>value</c> or <c>value;q=quality</c> when a quality factor is present.
    /// </summary>
    public override string ToString()
    {
        return Quality.HasValue
            ? $"{Value};q={Quality.Value.ToString("0.###", CultureInfo.InvariantCulture)}"
            : Value;
    }

    /// <summary>
    /// Determines whether this instance is equal to another <see cref="HeaderValue"/>.
    /// Equality is based on <see cref="Value"/> (case-insensitive) and <see cref="Quality"/>.
    /// </summary>
    /// <param name="other">The instance to compare with.</param>
    public bool Equals(HeaderValue? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase)
            && Nullable.Equals(Quality, other.Quality);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is HeaderValue other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
            hash = (hash * 397) ^ (Quality?.GetHashCode() ?? 0);
            return hash;
        }
    }

    /// <summary>
    /// Compares this instance to another <see cref="HeaderValue"/> for sort ordering.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sort order is determined as follows, in priority order:
    /// </para>
    /// <para>
    /// 1. Descending effective quality factor (higher quality sorts first).
    /// 2. Descending specificity weight (more specific MIME types sort first).
    /// 3. Ascending value string (case-insensitive, for stable ordering).
    /// </para>
    /// </remarks>
    /// <param name="other">The instance to compare with.</param>
    public int CompareTo(HeaderValue? other)
    {
        if (other is null) return -1;

        // Primary: descending quality
        var qualityComparison = other.EffectiveQuality.CompareTo(EffectiveQuality);
        if (qualityComparison != 0) return qualityComparison;

        // Secondary: descending specificity weight (MIME types only)
        var weightComparison = other.Weight.CompareTo(Weight);
        if (weightComparison != 0) return weightComparison;

        // Tertiary: ascending value for stable ordering
        return string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether two <see cref="HeaderValue"/> instances are equal.
    /// </summary>
    public static bool operator ==(HeaderValue? left, HeaderValue? right) => Equals(left, right);

    /// <summary>
    /// Determines whether two <see cref="HeaderValue"/> instances are not equal.
    /// </summary>
    public static bool operator !=(HeaderValue? left, HeaderValue? right) => !Equals(left, right);

    /// <summary>
    /// Computes the specificity weight for a MIME-type string.
    /// </summary>
    /// <param name="value">The MIME-type string to evaluate.</param>
    /// <returns>
    /// <see cref="WeightSpecific"/> for fully specified types,
    /// <see cref="WeightPartialWildcard"/> for wildcard-subtype types,
    /// or <see cref="WeightWildcard"/> for full wildcards and unrecognized formats.
    /// </returns>
    private static int ComputeMimeWeight(string value)
    {
        if (string.Equals(value, "*/*", StringComparison.Ordinal))
            return WeightWildcard;

        if (value.EndsWith("/*", StringComparison.Ordinal))
            return WeightPartialWildcard;

        return WeightSpecific;
    }
}