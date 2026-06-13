namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// Resolves the <c>bool</c> route constraint to a regular expression pattern
/// that matches <c>true</c> or <c>false</c>.
/// </summary>
public static class BoolResolver
{
    internal static readonly int    Strictness         = 20;
    internal static readonly int    Group              = (int)ConstraintGroup.None;
    internal static readonly string NoArgumentsMessage = "The 'bool' constraint does not accept arguments.";

    private static readonly string _pattern = "(?<{0}>true|false)";

    /// <summary>
    /// Returns a named capture group pattern matching <c>true</c> or <c>false</c>.
    /// This constraint does not accept arguments.
    /// </summary>
    /// <param name="name">The route parameter name for the named capture group.</param>
    /// <param name="args">Must be <see langword="null"/> or empty.</param>
    /// <returns>
    /// A tuple containing the named capture group pattern, a strictness value of
    /// <c>20</c>, and a group of <see cref="ConstraintGroup.None"/>.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="args"/> is non-empty.</exception>
    public static (string pattern, int strictness, int group) Resolve(string name, string? args)
    {
        if (!string.IsNullOrWhiteSpace(args))
            throw new ArgumentException(NoArgumentsMessage, nameof(args));

        return (string.Format(_pattern, name), Strictness, Group);
    }
}