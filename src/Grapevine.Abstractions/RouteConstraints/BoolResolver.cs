namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// Resolves the <c>bool</c> route constraint to a regular expression pattern
/// that matches <c>true</c> or <c>false</c>.
/// </summary>
public static class BoolResolver
{
    internal static readonly string NoArgumentsMessage = "The 'bool' constraint does not accept arguments.";

    /// <summary>
    /// Returns a named capture group pattern matching <c>true</c> or <c>false</c>.
    /// This constraint does not accept arguments.
    /// </summary>
    /// <param name="name">The route parameter name for the named capture group.</param>
    /// <param name="args">Must be <see langword="null"/> or empty.</param>
    /// <returns>A named capture group regular expression pattern.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="args"/> is non-empty.</exception>
    public static string Resolve(string name, string? args)
    {
        if (!string.IsNullOrWhiteSpace(args))
            throw new ArgumentException(NoArgumentsMessage, nameof(args));

        return $"(?<{name}>true|false)";
    }
}