using System.Diagnostics;
using System.Globalization;
using System.Text;
using Grapevine.Abstractions;

namespace Grapevine;

/// <summary>
/// Represents a parsed HTTP header with one or more values, including optional
/// quality factors for content negotiation headers.
/// </summary>
/// <remarks>
/// <para>
/// This type is the parsed view of a header. It is not the storage format used
/// internally by header collections. Instances are returned on demand by the
/// parsed-access API on <see cref="RequestHeaderCollection"/> and
/// <see cref="ResponseHeaderCollection"/>, and are not cached until the
/// collection is sealed.
/// </para>
/// <para>
/// For headers that carry quality-factor values (e.g. <c>Accept</c>,
/// <c>Accept-Language</c>, <c>Accept-Encoding</c>), values are parsed into
/// <see cref="HeaderValue"/> instances that support preference ordering via
/// <see cref="HeaderValue.CompareTo"/>. For MIME-type quality headers, use
/// <see cref="HeaderValue.FromMimeType"/> to obtain specificity-weighted values.
/// </para>
/// <para>
/// Multi-value headers are parsed from a comma-separated raw string. The
/// <see cref="Value"/> property returns the first value, or
/// <see langword="null"/> if the header has no values.
/// </para>
/// </remarks>
[DebuggerDisplay("{Name}: {Values.Count} value(s)")]
public class Header : IEquatable<Header>
{
    private readonly List<HeaderValue> _values = new();

    /// <summary>
    /// Gets the name of the HTTP header.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the string representation of the first value, or
    /// <see langword="null"/> if the header has no values.
    /// </summary>
    public string? Value => _values.Count > 0 ? _values[0].Value : null;

    /// <summary>
    /// Gets all parsed values for this header, including optional quality factors.
    /// </summary>
    public IReadOnlyList<HeaderValue> Values => _values.AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether this header has at least one value.
    /// </summary>
    public bool HasValue => _values.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this header has more than one value.
    /// </summary>
    public bool IsMultiValue => _values.Count > 1;

    /// <summary>
    /// Initializes a new instance of <see cref="Header"/> with the specified name
    /// and optional raw value string.
    /// </summary>
    /// <param name="name">
    /// The header name. Must not be <see langword="null"/>.
    /// </param>
    /// <param name="value">
    /// The raw header value string. Comma-separated values are parsed into
    /// individual <see cref="HeaderValue"/> instances. May be
    /// <see langword="null"/> or empty.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    public Header(string name, string? value = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ParseValues(value);
    }

    /// <summary>
    /// Returns the formatted HTTP header string in the form
    /// <c>Name: value1, value2</c>, or just <c>Name</c> if no values are present.
    /// </summary>
    public override string ToString()
    {
        if (_values.Count == 0) return Name;

        var sb = new StringBuilder(Name);
        sb.Append(": ");
        sb.Append(string.Join(", ", _values));
        return sb.ToString();
    }

    /// <summary>
    /// Determines whether this instance is equal to another <see cref="Header"/>.
    /// Equality is based on <see cref="Name"/> (case-insensitive) and the full
    /// sequence of <see cref="Values"/>.
    /// </summary>
    /// <param name="other">The instance to compare with.</param>
    public bool Equals(Header? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
            && _values.Count == other._values.Count
            && _values.SequenceEqual(other._values);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Header other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
            foreach (var val in _values)
                hash = (hash * 397) ^ val.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Implicitly converts a <see cref="Header"/> to its formatted string representation.
    /// </summary>
    public static implicit operator string(Header header) => header.ToString();

    /// <summary>
    /// Determines whether two <see cref="Header"/> instances are equal.
    /// </summary>
    public static bool operator ==(Header? left, Header? right) => Equals(left, right);

    /// <summary>
    /// Determines whether two <see cref="Header"/> instances are not equal.
    /// </summary>
    public static bool operator !=(Header? left, Header? right) => !Equals(left, right);

    /// <summary>
    /// Parses a raw header value string into <see cref="HeaderValue"/> instances
    /// and populates <see cref="_values"/>.
    /// </summary>
    /// <remarks>
    /// Values are split on commas. Each segment is trimmed and checked for an
    /// optional <c>;q=</c> quality factor. Segments that are empty after trimming
    /// are skipped. Quality factors that fail to parse are silently ignored and
    /// the value is added without a quality factor.
    /// </remarks>
    /// <param name="value">The raw value string to parse.</param>
    private void ParseValues(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        var segments = value!.Split(',');
        foreach (var segment in segments)
        {
            var trimmed = segment.Trim();
            if (trimmed.Length == 0) continue;

            var semiIndex = trimmed.IndexOf(';');
            if (semiIndex < 0)
            {
                _values.Add(new HeaderValue(trimmed));
                continue;
            }

            var val = trimmed.Substring(0, semiIndex).Trim();
            var qPart = trimmed.Substring(semiIndex + 1).Trim();

            if (qPart.StartsWith("q=", StringComparison.OrdinalIgnoreCase)
                && double.TryParse(
                    qPart.Substring(2),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var q))
            {
                _values.Add(new HeaderValue(val, q));
            }
            else
            {
                _values.Add(new HeaderValue(val));
            }
        }
    }
}