---
title: Route Sorting and Ambiguity
---

When multiple routes could match an incoming request, Grapevine must decide which one to use. Routes are sorted at registration time so that the most specific route always wins. If two routes are genuinely indistinguishable, Grapevine throws an exception immediately rather than silently picking one at runtime.

## General Rules

Routes are sorted by specificity. More specific routes sort first and are evaluated before less specific ones. The following rules are applied in order:

**1. Routes with a specific HTTP method sort before routes that match any method.**

A route registered for `GET` always sorts before a route registered for `*` (any method), even if both templates are identical.

**2. When methods differ, routes are sorted alphabetically by method name.**

`DELETE` sorts before `GET`, which sorts before `POST`, and so on. This is a stable ordering for diagnostics and inspection, not a ranking by importance.

**3. When methods are equal, routes are sorted by template specificity.**

Templates are compared segment by segment from left to right. The first segment that produces a difference determines the winner. A route wins at a given position when:

- Its segment is a **literal** and the other is a **parameter** -- literals are more specific
- Both segments are parameters, and its constraint is **stricter** -- a `guid` beats a `text`
- Both templates are equal up to the shorter template's last segment -- the **longer** template wins

## Constraint Strictness

When two parameter segments are being compared, the one with the stricter constraint sorts first. Stricter constraints match a narrower set of values.

From strictest to least strict:

| Strictness | Constraint |
|---|---|
| 1 | `regex` |
| 10 | `guid` |
| 20 | `bool` |
| 30 | `date` |
| 40 | `datetime` |
| 50 | `decimal` |
| 60 | `int`, `long` |
| 70 | `double`, `float` |
| 80 | `numeric` |
| 90 | `alpha` |
| 100 | `text`, `len` |

A route using `{id:guid}` at a given segment position sorts before `{id:int}`, which sorts before `{id:text}`.

## Constraint Groups and Ambiguity

Some constraints belong to the same **compatibility group** because their match sets overlap significantly. Two routes that differ only in which group member is used on the same segment position may produce unpredictable behavior at runtime -- for example, both `int` and `numeric` would match the value `42`.

The built-in groups are:

- **Numeric:** `decimal`, `int`, `long`, `double`, `float`, `numeric`
- **Date:** `date`, `datetime`
- **Text:** `alpha`, `text`, `len`

By default, registering two routes whose templates are structurally identical and differ only in which same-group constraint is used on a particular segment is treated as a potential conflict. The routes can still be sorted by strictness, but the behavior can be configured.

:::caution
Consider this example:

```
GET /orders/{id:int}
GET /orders/{id:double}
```

The value `2` matches both routes. `int` is stricter, so it sorts first and wins -- but a client sending `2` when it intended to reach the `double` route will silently hit the wrong one. Prefer distinct URL structures rather than relying on numeric constraint differences alone.
:::

## The `regex` Special Case

Two routes using `regex` on the same segment are only ambiguous when the patterns are identical. Different patterns produce different routes and are sorted alphabetically by pattern string. If the patterns are the same, the routes are indistinguishable and registration throws.

## Ambiguity Errors

If two routes cannot be distinguished after applying all sorting rules, Grapevine throws an `InvalidOperationException` at registration time. This happens when:

- Both routes have the same HTTP method
- Their templates are structurally identical at every segment position -- same literals, same constraints, same arguments

The error is intentionally thrown at startup rather than at runtime so that configuration mistakes are caught immediately during development.

## Technical Detail

Sorting is implemented as a comparison operation applied during route table construction. Each `RouteDescriptor` exposes a `CompareTo` method that applies the rules above. The comparison walks each segment pair using `SegmentDescriptor.CompareTo`, which returns a negative value, a positive value, or zero. Zero means the two segments are indistinguishable at that position -- not necessarily a conflict on their own, since a later segment or the HTTP method may still differentiate them.

`SegmentDescriptor.CompareTo` never throws. Ambiguity detection is the responsibility of `RouteDescriptor.CompareTo`, which throws when the full comparison exhausts all differentiators and still produces no winner.

The sort rules above are applied in this order at the segment level:

1. Literal before parameter
2. Both literals: alphabetical by raw value
3. Both parameters in the same constraint group: indistinguishable at this position (configurable)
4. Both parameters: stricter constraint sorts first
5. Equal strictness, neither is `regex`: indistinguishable at this position
6. Both `regex`: alphabetical by pattern; identical patterns are indistinguishable