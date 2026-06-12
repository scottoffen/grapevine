namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// Resolves a decimal precision constraint argument string into a regular expression
/// quantifier for use in decimal place matching.
/// </summary>
/// <remarks>
/// Unlike <see cref="LengthPatternResolver"/>, this resolver allows a minimum value
/// of zero, enabling patterns such as <c>{param:decimal(0)}</c> to match whole numbers
/// with no decimal places. Use <see cref="LengthPatternResolver"/> for character count
/// constraints where a minimum of 1 is always required.
/// </remarks>
public static class PrecisionPatternResolver
{
    /// <summary>
    /// Converts a precision constraint argument into a regular expression quantifier
    /// string suitable for appending to a decimal place pattern, e.g. <c>\d{1,3}</c>.
    /// </summary>
    /// <remarks>
    /// Accepted formats:
    /// <list type="bullet">
    ///   <item><term>null or empty</term><description>any precision, equivalent to <c>+</c></description></item>
    ///   <item><term><c>0</c></term><description>no decimal places (whole numbers only)</description></item>
    ///   <item><term><c>2</c></term><description>exactly 2 decimal places</description></item>
    ///   <item><term><c>0,</c></term><description>zero or more decimal places</description></item>
    ///   <item><term><c>,3</c></term><description>up to 3 decimal places</description></item>
    ///   <item><term><c>1,3</c></term><description>between 1 and 3 decimal places</description></item>
    /// </list>
    /// </remarks>
    /// <param name="args">
    /// A string representing a precision constraint. May be <see langword="null"/>, empty,
    /// a single non-negative integer, or a range in <c>min,max</c> format.
    /// </param>
    /// <returns>A regex quantifier string, e.g. <c>+</c>, <c>{0}</c>, <c>{1,3}</c>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the input is invalid, improperly formatted, or contains a negative value.
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
                throw new ArgumentException($"Invalid argument '{args}' for precision constraint.");

            var min = 0;
            int? max = null;

            if (!string.IsNullOrWhiteSpace(left))
            {
                if (!int.TryParse(left, out var minParsed) || minParsed < 0)
                    throw new ArgumentException($"Invalid argument '{args}' for precision constraint.");

                min = minParsed;
            }

            if (!string.IsNullOrWhiteSpace(right))
            {
                if (!int.TryParse(right, out var maxParsed) || maxParsed < 0)
                    throw new ArgumentException($"Invalid argument '{args}' for precision constraint.");

                max = maxParsed;
            }

            if (max != null && max < min)
                throw new ArgumentException($"Invalid argument '{args}' for precision constraint.");

            return max.HasValue ? $"{{{min},{max}}}" : $"{{{min},}}";
        }
        else
        {
            if (!int.TryParse(args, out var exact) || exact < 0)
                throw new ArgumentException($"Invalid argument '{args}' for precision constraint.");

            return $"{{{exact}}}";
        }
    }
}