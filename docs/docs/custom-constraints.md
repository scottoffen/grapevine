---
title: Custom Constraint Resolvers
---

Grapevine's built-in constraints cover common types, but you can register your own constraints when the built-ins are not sufficient. A custom constraint resolver is a delegate that translates a parameter name and optional argument string into a regular expression pattern, a strictness value, and a group identifier.

## The Resolver Delegate

A constraint resolver has the following signature:

```csharp
(string name, string? args) => (string pattern, int strictness, int group)
```

| Parameter | Description |
|---|---|
| `name` | The route parameter name, used as the named capture group identifier |
| `args` | The argument string from the template, or `null` if none was provided |
| `pattern` | A regular expression string wrapped in a named capture group: `(?<name>...)` |
| `strictness` | A numeric value indicating how strictly the pattern constrains the match. Lower values are stricter. |
| `group` | An integer identifying the compatibility group for ambiguity detection. Use `(int)ConstraintGroup.None` when no group applies. |

## Writing a Resolver

A resolver must return a named capture group wrapping the pattern. The capture group must use the `name` parameter so that Grapevine can extract the matched value as a route parameter.

Here is a resolver for a `slug` constraint that matches URL-friendly strings -- lowercase letters, digits, and hyphens:

```csharp
using Grapevine.Abstractions;
using Grapevine.Abstractions.RouteConstraints;

(string pattern, int strictness, int group) SlugResolver(string name, string? args)
{
    return ($"(?<{name}>[a-z0-9]+(?:-[a-z0-9]+)*)", 85, (int)ConstraintGroup.Text);
}
```

A few things to note:

- The pattern is wrapped in `(?<{name}>...)` so the captured value is accessible by parameter name
- A strictness of `85` places this constraint between `alpha` (90) and `numeric` (80) in the ordering -- stricter than `alpha` since it requires lowercase only, but less strict than `numeric` since it also allows letters and hyphens
- `ConstraintGroup.Text` is used because a slug is a subset of text values, making it compatible with the text family for conflict detection purposes

## Registering a Resolver

Use `ResolverRegistry.RegisterResolver` to add a new constraint under a key. The key is the name consumers will use in route templates.

```csharp
ResolverRegistry.RegisterResolver("slug", SlugResolver);
```

Once registered, the constraint is available in any route template:

```
{title:slug}
{title:slug}    -- matches: my-article-title
                -- does not match: My Article Title, my_article
```

`RegisterResolver` throws if the key is already registered. To replace an existing constraint, use `OverrideResolver` instead:

```csharp
ResolverRegistry.OverrideResolver("slug", SlugResolver);
```

:::note
`RegisterResolver` and `OverrideResolver` are intended for use at startup, before any routes are registered. Registering constraints after routes have already been added to the routing table is not supported.
:::

## Choosing a Strictness Value

Strictness determines how your constraint competes with others when two routes differ only in which constraint is used on the same segment. Lower values are stricter and sort first.

The built-in values run from `1` (most strict, `regex`) to `100` (least strict, `text`/`len`) in increments of 10. Choose a value that reflects where your constraint sits relative to the built-ins. Gaps between the built-in values leave room for custom constraints to be inserted at any point in the hierarchy.

If your constraint is a specialization of an existing built-in -- for example, a `slug` is a more restrictive form of `alpha` -- choose a strictness value lower than the built-in it refines.

## Choosing a Constraint Group

The constraint group controls ambiguity detection. When two routes on the same segment use constraints from the same group, they may be flagged as a potential conflict.

Grapevine provides three built-in groups:

```csharp
ConstraintGroup.None     // 0  -- no group, singleton constraints
ConstraintGroup.Numeric  // 10 -- decimal, int, long, double, float, numeric
ConstraintGroup.Date     // 20 -- date, datetime
ConstraintGroup.Text     // 30 -- alpha, text, len
```

If your constraint's match set overlaps significantly with an existing group, use that group. If it is a completely independent type with no meaningful overlap, use `ConstraintGroup.None`.

You can also define your own group by choosing an integer value that does not conflict with the built-ins. The built-in values are multiples of 10, so values like `15`, `25`, or `100` are safe for custom groups:

```csharp
const int MyCustomGroup = 100;

(string pattern, int strictness, int group) MyResolver(string name, string? args)
{
    return ($"(?<{name}>...)", 55, MyCustomGroup);
}
```

## Handling Arguments

If your constraint accepts arguments, parse them from the `args` parameter. Always handle `null` and empty values gracefully -- they represent a constraint used without arguments.

```csharp
(string pattern, int strictness, int group) PrefixResolver(string name, string? args)
{
    if (string.IsNullOrWhiteSpace(args))
        throw new ArgumentException(
            "The 'prefix' constraint requires a prefix argument, e.g. {id:prefix(usr_)}.",
            nameof(args));

    var escaped = Regex.Escape(args.Trim());
    return ($"(?<{name}>{escaped}[a-zA-Z0-9]+)", 85, (int)ConstraintGroup.None);
}
```

Used in a template:

```
{id:prefix(usr_)}    -- matches: usr_abc123
                     -- does not match: 123, abc123
```

## Complete Example

The following registers a `version` constraint that matches a semantic version string in the format `major.minor.patch`:

```csharp
using System.Text.RegularExpressions;
using Grapevine.Abstractions;
using Grapevine.Abstractions.RouteConstraints;

ResolverRegistry.RegisterResolver("version", (name, args) =>
{
    if (!string.IsNullOrWhiteSpace(args))
        throw new ArgumentException(
            "The 'version' constraint does not accept arguments.",
            nameof(args));

    return ($@"(?<{name}>\d+\.\d+\.\d+)", 15, (int)ConstraintGroup.None);
});
```

The strictness value of `15` places it between `regex` (1) and `guid` (10) -- it is a highly specific pattern but not consumer-supplied, so it sits just above the built-in singletons.

Used in a template:

```
{v:version}    -- matches: 1.0.0, 2.13.4
               -- does not match: 1.0, v1.0.0, 1.0.0.0
```