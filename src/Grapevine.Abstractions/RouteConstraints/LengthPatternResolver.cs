namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// Resolves a length constraint argument string into a regular expression quantifier
/// for use in character count matching.
/// </summary>
/// <remarks>
/// For decimal precision constraints, use <see cref="PrecisionPatternResolver"/> instead,
/// which supports a minimum value of zero.
/// </remarks>
public static class LengthPatternResolver
{
    internal static readonly string InvalidArgumentMessage = "Invalid argument '{0}' for length constraint.";

    /// <summary>
    /// Converts a length constraint argument into a regular expression quantifier
    /// string suitable for appending to a character class pattern, e.g. <c>[a-z]{1,5}</c>.
    /// </summary>
    /// <remarks>
    /// Accepted formats:
    /// <list type="bullet">
    ///   <item><term>null or empty</term><description>any length, equivalent to <c>+</c></description></item>
    ///   <item><term><c>3</c></term><description>exactly 3 characters</description></item>
    ///   <item><term><c>1,</c></term><description>at least 1 character</description></item>
    ///   <item><term><c>,5</c></term><description>up to 5 characters</description></item>
    ///   <item><term><c>1,5</c></term><description>between 1 and 5 characters</description></item>
    /// </list>
    /// </remarks>
    /// <param name="args">
    /// A string representing a length constraint. May be <see langword="null"/>, empty,
    /// a single integer, or a range in <c>min,max</c> format. Minimum value is always 1.
    /// </param>
    /// <returns>A regex quantifier string, e.g. <c>+</c>, <c>{3}</c>, <c>{1,}</c>, or <c>{1,5}</c>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the input is invalid, improperly formatted, or below the minimum
    /// value of 1.
    /// </exception>
    public static string Resolve(string? args)
    {
        if (string.IsNullOrWhiteSpace(args))
            return "+";

        var commaIndex = args!.IndexOf(',');
        if (commaIndex >= 0)
        {
            var left = args.Substring(0, commaIndex);
            var right = args.Substring(commaIndex + 1);

            if (string.IsNullOrWhiteSpace(left) && string.IsNullOrWhiteSpace(right))
                throw new ArgumentException(string.Format(InvalidArgumentMessage, args));

            var min = 1;
            int? max = null;

            if (!string.IsNullOrWhiteSpace(left))
            {
                if (!int.TryParse(left, out var minParsed) || minParsed < 1)
                    throw new ArgumentException(string.Format(InvalidArgumentMessage, args));

                min = minParsed;
            }

            if (!string.IsNullOrWhiteSpace(right))
            {
                if (!int.TryParse(right, out var maxParsed) || maxParsed < 1)
                    throw new ArgumentException(string.Format(InvalidArgumentMessage, args));

                max = maxParsed;
            }

            if (max != null && max < min)
                throw new ArgumentException(string.Format(InvalidArgumentMessage, args));

            return max.HasValue ? $"{{{min},{max}}}" : $"{{{min},}}";
        }
        else
        {
            if (!int.TryParse(args, out var exact) || exact < 1)
                throw new ArgumentException(string.Format(InvalidArgumentMessage, args));

            return $"{{{exact}}}";
        }
    }
}