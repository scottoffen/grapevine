namespace Grapevine.Abstractions.RouteConstraints;

/// <summary>
/// Represents a method that resolves a route constraint to a regular expression
/// pattern, a strictness value, and a compatibility group identifier.
/// </summary>
/// <param name="name">The name of the route parameter to use for the named capture group.</param>
/// <param name="args">Optional arguments for the constraint, or <see langword="null"/> if none.</param>
/// <returns>
/// A tuple containing:
/// <list type="bullet">
///   <item><term>pattern</term><description>A regular expression pattern string wrapped in a named capture group.</description></item>
///   <item><term>strictness</term><description>A numeric value indicating how strictly the pattern constrains the match. Lower values are more strict.</description></item>
///   <item><term>group</term><description>An integer identifying the compatibility group for ambiguity detection. Use <c>(int)ConstraintGroup.None</c> when no group applies.</description></item>
/// </list>
/// </returns>
public delegate (string pattern, int strictness, int group) RouteConstraintResolver(string name, string? args);