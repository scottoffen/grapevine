---
title: Route Constraints
---

A route constraint restricts what a parameter segment in a route template will match. When a constraint is specified, the segment only matches incoming path values that satisfy the constraint's pattern.

```
{id:int}               -- only matches signed integers
{id:guid}              -- only matches canonical GUIDs
{flag:bool}            -- only matches 'true' or 'false'
```

For an introduction to route template syntax, see [Route Templates](./route-templates).

## Summary

| Constraint | Aliases | Arguments | Description |
|---|---|---|---|
| `alpha` | | length | One or more alphabetic characters (a-z, A-Z) |
| `bool` | | none | Literal `true` or `false` |
| `date` | | format | A date string in a recognized format |
| `datetime` | | format | A date and time string in a recognized format |
| `decimal` | | precision | A signed or unsigned decimal number |
| `double` | `float` | precision | A decimal or scientific notation number |
| `guid` | | none | A canonical 36-character GUID |
| `int` | `long` | length | A signed or unsigned integer |
| `numeric` | | length | One or more digit characters, no sign |
| `regex` | | pattern | A user-supplied regular expression |
| `text` | `len` | length | One or more non-slash characters (default) |

## Length and Precision Arguments

Several constraints accept a length or precision argument that restricts the number of characters or decimal places matched.

| Format | Meaning |
|---|---|
| *(none)* | Any length |
| `3` | Exactly 3 |
| `1,` | At least 1 |
| `,5` | Up to 5 (minimum depends on constraint) |
| `1,5` | Between 1 and 5 |

Length constraints require a minimum of 1. Precision constraints (used by `decimal` and `double`) allow a minimum of 0, enabling whole-number-only matching.

---

## alpha

Matches one or more alphabetic characters (a-z, A-Z). Accepts an optional length argument.

```
{name:alpha}           -- one or more letters
{code:alpha(3)}        -- exactly 3 letters
{tag:alpha(2,5)}       -- between 2 and 5 letters
```

---

## bool

Matches the literal string `true` or `false`. Does not accept arguments.

```
{active:bool}
```

---

## date

Matches a date string in one of the supported formats. When no argument is given, defaults to ISO format.

```
{created:date}         -- ISO format: 2023-05-21
{created:date(ymd)}    -- 2023/05/21 or 2023-05-21
{created:date(mdy)}    -- 05/21/2023 or 05-21-2023
{created:date(dmy)}    -- 21/05/2023 or 21-05-2023
{created:date(basic)}  -- compact: 20230521
```

| Argument | Example |
|---|---|
| *(none)* or `iso` | `2023-05-21` |
| `ymd` | `2023/05/21` |
| `mdy` | `05/21/2023` |
| `dmy` | `21/05/2023` |
| `basic` | `20230521` |

:::note
The `date` constraint matches the shape of a date string, not its validity. A value like `9999-99-99` would still match the `iso` format. For validated date parsing, extract the value and parse it in your handler.
:::

For matching date and time together, use [`datetime`](#datetime) instead.

---

## datetime

Matches a date and time string in one of the supported formats. When no argument is given, defaults to ISO 8601.

```
{ts:datetime}          -- ISO 8601: 2023-05-21T14:30:00
{ts:datetime(time)}    -- time only: 14:30:00
{ts:datetime(basic)}   -- compact date: 20230521
{ts:datetime(rfc)}     -- RFC 1123: Sun, 21 May 2023 14:30:00 GMT
```

| Argument | Example |
|---|---|
| *(none)* or `iso` | `2023-05-21T14:30:00` |
| `time` | `14:30:00` |
| `basic` | `20230521` |
| `rfc` | `Sun, 21 May 2023 14:30:00 GMT` |

For date-only matching, use [`date`](#date) instead.

---

## decimal

Matches a signed or unsigned decimal number. Accepts an optional precision argument controlling the number of decimal places.

```
{price:decimal}        -- any decimal: 3, 3.14, -3.14
{price:decimal(2)}     -- exactly 2 decimal places: 3.14
{price:decimal(0)}     -- whole numbers only: 3
{price:decimal(0,3)}   -- zero to 3 decimal places
```

---

## double / float

Matches a decimal or scientific notation number. `float` is an alias for `double`. Accepts an optional precision argument.

```
{n:double}             -- 3, 3.14, -3.14, 1.5e10
{n:double(2)}          -- exactly 2 decimal places
{n:float}              -- identical to double
```

---

## guid

Matches a canonical 36-character GUID in the format `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`. Does not accept arguments.

```
{id:guid}
```

Both uppercase and lowercase hex characters are accepted.

---

## int / long

Matches a signed or unsigned integer. `long` is an alias for `int`. Accepts an optional digit length argument (the optional sign character is not counted toward the length).

```
{id:int}               -- any integer: 42, -7
{id:int(3)}            -- exactly 3 digits: 042, -042
{id:int(1,5)}          -- between 1 and 5 digits
{id:long}              -- identical to int
```

:::note
`int` and `long` are aliases and produce identical matching behavior. The distinction is semantic, useful for documentation purposes when the handler expects a specific numeric type.
:::

---

## numeric

Matches one or more digit characters (0-9) with no leading sign. Accepts an optional length argument.

```
{code:numeric}         -- any digits: 42, 007
{code:numeric(3)}      -- exactly 3 digits: 007
{code:numeric(1,5)}    -- between 1 and 5 digits
```

Unlike `int`, `numeric` does not allow a leading minus sign. Use `int` or `long` when negative values are needed.

---

## regex

Matches a user-supplied regular expression pattern. The pattern is wrapped in a named capture group using the parameter name.

```
{slug:regex([a-z0-9-]+)}
{version:regex(\d+\.\d+\.\d+)}
```

The pattern must follow these rules:

- Must not be empty
- Must not be anchored -- do not start with `^` or end with `$`
- Must not contain unnamed capturing groups -- use named groups `(?<name>...)` or non-capturing groups `(?:...)`
- Must be a valid regular expression

Named groups inside the pattern are captured alongside the outer parameter and made available as additional route parameters.

```
{date:regex((?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2}))}
```

This produces three captured values: `date`, `year`, `month`, and `day`.

:::note
The `regex` constraint is the most specific constraint in Grapevine's strictness hierarchy. Routes using `regex` sort before routes using any other constraint on the same segment position.
:::

---

## text / len

Matches one or more non-slash characters. `len` is an alias for `text`. This is the default constraint when none is specified. Accepts an optional length argument.

```
{name:text}            -- one or more non-slash characters
{name:text(10)}        -- exactly 10 characters
{name:len(1,20)}       -- between 1 and 20 characters
{name}                 -- equivalent to {name:text}
```