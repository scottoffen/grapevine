# Contributing to Grapevine

We absolutely want to hear from you! **Your involvement makes Grapevine better** by improving usability, quality, and adoption. Our goal is to be responsive and transparent stewards of the project. Following these guidelines will help us help you.

## How to Get Help

Many issues created on open source projects turn out to be usage questions or general discussions rather than bugs or feature requests. These are valuable, but they don't need to be tracked as GitHub issues. Anyone in the community can help answer them.

For usage questions and discussions, please use these channels first:

* [Official documentation](https://scottoffen.github.io/grapevine)
* [GitHub community discussions](https://github.com/scottoffen/grapevine/discussions)
* [StackOverflow (tagged #grapevine)](https://stackoverflow.com/questions/tagged/grapevine?sort=newest)

To avoid confusion: **issues created solely to ask usage questions will be closed.**

## Getting Off to a Good Start

Whether reporting a bug, posting on StackOverflow, or joining discussions, always include enough context to get meaningful help:

* Use a descriptive title and clear description.
* Provide details: OS + version, .NET version, Grapevine version.
* For interoperability issues, include details about other frameworks/tools in use (logging, DI libraries, IDE, etc.).
* Share the minimum code needed to reproduce the behavior.
* Add screenshots or repo links where useful.

## Feature Requests

Got an idea? Start by opening a [discussion](https://github.com/scottoffen/grapevine/discussions) to gather feedback.
**Do not open a GitHub issue until there's positive consensus.**

When proposing features, we want to ensure we are **focused on solving problems, not attacking symptoms**, while staying true to Grapevine's guiding principles: **fast, unopinionated, minimalist**. In a word: **simple.**

If consensus supports your idea, you may open a feature request issue that reflects the discussion outcome.

## Issue Management

Issues are for things that can be **fixed, added, resolved, or implemented.**

### Issues That Can't or Won't Be Fixed

Some reports are too vague, undefined, or out of scope. In those cases, we'll ask you to clarify what "done" looks like. If the goal remains undefined or unachievable (e.g., outside Grapevine's vision), the issue will be closed.

### Abandoned Issues

If maintainers request more information, the issue will be labeled `more-information-needed`.

* If no response is received within **1 week or less**, we'll send a reminder.
* If no response after **30 days or more**, the issue will be closed.

## Coding Conventions

Grapevine is written in C#. The repository includes a comprehensive [`.editorconfig`](../.editorconfig) file that defines code style, formatting, and naming conventions across the project, as well as consistent rules for indentation, naming, style preferences, and analyzer severities.

* Prefer **[readable code](https://www.amazon.com/Art-Readable-Code-Practical-Techniques/dp/0596802293) > rigid convention.**
* Follow [Microsoft's C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions).
  * We may override conventions if it improves readability.
* All changes must include or update **unit tests**.

  * We use [xUnit](https://www.nuget.org/packages/xunit), [Shouldly](https://www.nuget.org/packages/Shouldly), and [Moq](https://www.nuget.org/packages/Moq).
  * Contributions that don't follow this testing stack will be rejected.

## Pull Requests

When submitting a PR:

* Use a descriptive title.
* Clearly list what you've done.
* Reference the issue it resolves with `Closes #123`.
* Include or update tests.
* Keep commits atomic (one feature/fix per commit).

### Purely Cosmetic PRs

Cosmetic-only changes (whitespace, reformatting, etc.) generally will not be accepted due to the hidden costs:

* They require review.
* They create noise in notifications.
* They pollute git history.

If your editor reformats a file (e.g., whitespace-only changes), please revert those before submitting.

## Documentation Changes

Good documentation is critical to Grapevine's success. Docs are written in [Docusaurus](https://docusaurus.io/) and live in the [docs folder](./docs/). Contributions of clear, concise, and accurate documentation are highly valued.