---
title: Route Templates
---

A route template is a string pattern that describes the shape of a URL path. When an HTTP request arrives, Grapevine compares the request path against every registered route template to find a match. Understanding how templates are parsed and matched is fundamental to working effectively with Grapevine's routing system.

## Structure

A route template is a sequence of segments separated by `/`. Each segment is either a **literal** or a **parameter**.

```
/users/orders              -- two literal segments
/users/{id}                -- one literal, one parameter
/users/{id}/orders         -- two literals, one parameter in the middle
/users/{id}/orders/{orderId:guid}   -- two literals, two parameters
```

Templates are normalized when they are registered. A leading slash is added if absent, and any trailing slash is removed. This means `/users/{id}`, `users/{id}`, and `users/{id}/` all produce the same registered template `/users/{id}`. You can write templates either way and get consistent behavior.

## Literal Segments

A literal segment matches exactly the text it contains. The segment `users` only matches the path component `users` -- nothing more, nothing less. Matching is case-insensitive, so `Users`, `USERS`, and `users` all match the segment `users`.

Literals can contain any characters that are valid in a URL path segment. If a literal contains characters that are special in regular expressions (such as `.`, `+`, or `(`), they are automatically escaped and treated as plain text.

## Parameter Segments

A parameter segment is wrapped in curly braces. It matches a portion of the incoming path and captures that value under the parameter name. The full syntax is:

```
{name}
{name:constraint}
{name:constraint(args)}
```

Where:

- `name` is the identifier used to retrieve the captured value after a match
- `constraint` is an optional type constraint that restricts what values the segment accepts
- `args` is an optional argument passed to the constraint, such as a length range or format specifier

When no constraint is specified, the segment matches one or more non-slash characters. This is equivalent to writing `{name:text}` explicitly.

A parameter segment never matches an empty value or a value containing a `/`. If you need to match a path that includes slashes, use a `regex` constraint with a pattern that explicitly allows them.

## The Parameter Name

The parameter name has two roles: it identifies the captured value in the route parameter collection, and it becomes the name of the named capture group in the underlying regular expression. Because of this, parameter names must be valid regular expression group names -- they should consist of letters, digits, and underscores, starting with a letter.

Within a single route template, every parameter name must be unique. Using the same name twice in one template is an error caught at registration time:

```csharp
// Throws at registration -- 'id' appears twice
"/users/{id:int}/orders/{id:guid}"
```

This restriction also covers named capture groups inside `regex` constraints. An inner group name that matches any other parameter name in the same template will be rejected:

```csharp
// Throws at registration -- 'id' appears as both a parameter name
// and an inner capture group name
"/archive/{id:int}/entries/{entry:regex((?<id>[a-z]+)-\d+)}"
```

## Route Parameters

When an incoming path matches a route template, the captured values from each parameter segment are collected into a `RouteParams` object -- a key-value collection keyed by parameter name.

For example, the template `/users/{userId:int}/orders/{orderId:guid}` matched against `/users/42/orders/3f2504e0-4f89-11d3-9a0c-0305e82c3301` produces:

| Key | Value |
|---|---|
| `userId` | `42` |
| `orderId` | `3f2504e0-4f89-11d3-9a0c-0305e82c3301` |

All captured values are strings. The constraint determines whether the segment matches at all, but the value you receive is always the raw string from the path -- you are responsible for parsing it into the appropriate type in your handler.

## Named Inner Capture Groups

When a parameter uses a `regex` constraint, any named capture groups inside the pattern are also captured and included in the route parameters alongside the outer parameter. This allows a single parameter segment to extract multiple structured values from a path component.

```csharp
// Template
"/events/{date:regex((?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2}))}"

// Path: /events/2023-05-21
// Route parameters:
//   date  = "2023-05-21"
//   year  = "2023"
//   month = "05"
//   day   = "21"
```

The outer parameter name (`date`) and all inner group names (`year`, `month`, `day`) must be unique across the entire template. Grapevine checks this at registration time and throws if any name is reused.

## How Matching Works

Grapevine compiles each route template into a regular expression at registration time. The compiled expression anchors the pattern to the full path (`^...$`) so that partial path matches are not accepted. A request for `/users/42/extra` will not match a template for `/users/{id}`.

Each literal segment is escaped and matched exactly. Each parameter segment is resolved through its constraint into a named capture group pattern. The segment patterns are joined with `/` separators to form the full expression.

This compilation happens once when the route is registered and the resulting `Regex` object is reused on every incoming request. There is no per-request parsing overhead.

## What a Template Does Not Cover

A route template only describes the URL path. HTTP method matching, header inspection, and any other request properties are separate concerns handled elsewhere in the routing system. A template match alone does not mean a route will handle the request.

## Constraints

Constraints give you precise control over what a parameter segment accepts. Grapevine provides a comprehensive set of built-in constraints covering text, numeric, date, and pattern-based matching. For a full reference, see [Route Constraints](./route-constraints).