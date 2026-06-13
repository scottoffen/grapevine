namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// Resolves the <c>guid</c> route constraint to a regular expression pattern
/// that matches a canonical 36-character GUID.
/// </summary>
public static class GuidResolver
{
    internal static readonly int    Strictness         = 10;
    internal static readonly int    Group              = (int)ConstraintGroup.None;
    internal static readonly string NoArgumentsMessage = "The 'guid' constraint does not accept arguments.";

    private static readonly string _pattern =
        @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";

    /// <summary>
    /// Returns a named capture group pattern matching a canonical GUID in the format
    /// <c>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</c>. This constraint does not accept arguments.
    /// </summary>
    /// <param name="name">The route parameter name for the named capture group.</param>
    /// <param name="args">Must be <see langword="null"/> or empty.</param>
    /// <returns>
    /// A tuple containing the named capture group pattern, a strictness value of
    /// <c>10</c>, and a group of <see cref="ConstraintGroup.None"/>.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="args"/> is non-empty.</exception>
    public static (string pattern, int strictness, int group) Resolve(string name, string? args)
    {
        if (!string.IsNullOrWhiteSpace(args))
            throw new ArgumentException(NoArgumentsMessage, nameof(args));

        return ($"(?<{name}>{_pattern})", Strictness, Group);
    }
}