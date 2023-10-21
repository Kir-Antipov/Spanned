# Contributing to Spanned

Thank you for considering contributing to Spanned, we welcome and appreciate all contributions! To ensure a smooth and efficient review process, please follow the guidelines below.

For this guide, we're going to split PRs into two types: bug fixes and features. The requirements for each are slightly different.

## Bug Fixes

When submitting a bug fix, it is highly recommended to include accompanying unit tests. The tests can be found in the `/tests/Spanned.Tests` project and are categorized based on the type they are testing.

While it may not always be possible to provide a test, doing so will speed up the review process.

The commits in a bug fix pull request are recommended to follow the following order:

1. A commit with a failing unit test.
2. A commit that fixes the issues.

This approach allows the reviewer to easily identify and confirm the problem before reviewing and accepting the fix.

## Features

Before starting work on a new feature, please open an issue to discuss it with the maintainers. This ensures that everyone is on the same page and avoids wasting efforts.

All feature submissions should include unit tests. Additionally, if applicable, please provide benchmarks for the new functionality. The benchmarks can be found in the `/tests/Spanned.Benchmarks` project and are categorized based on the type they are benchmarking.

## General Guidance

### PR Description

Please provide a clear and informative description for your pull request. Failing to do so will delay the review of the PR at a minimum, or may cause it to be dismissed entirely. If English is not your preferred language, consider using tools like ChatGPT to assist you in writing the description.

### Breaking Changes

If your modifications cause any breaking changes, whether related to source code, binary compatibility, or behavior, please highlight them in the PR description.

### Commits

In addition to the guidelines mentioned in the [Bug Fixes](#bug-fixes) section, following these best practices can help speed up the review process:

- Rebase your changes to remove unnecessary commits.
- Use meaningful commit messages that accurately describe the changes.
- Avoid making unrelated code changes.
- Refrain from introducing unnecessary formatting or whitespace changes.

### Style

- Follow the general [.NET coding style](https://github.com/dotnet/runtime/blob/master/docs/coding-guidelines/coding-style.md).
- Aim to keep lines of code around 120 characters or less, although this is not a strict limit.
- Ensure that all documentable code is covered by XML documentation.
- Avoid using `#region` directives.

Please note that test method names do not follow the usual naming convention. Instead, they should be written in a sentence style, separated by underscores, that describes in English what the test is testing. For example:

```csharp
void Calling_Foo_Should_Increment_Bar()
```

## Code of Conduct

Please note that by contributing to this project, you are expected to adhere to the project's code of conduct defined by the [Contributor Covenant](https://github.com/Kir-Antipov/Spanned/blob/master/CODE_OF_CONDUCT.md).

## License

By contributing to this project, you agree that your contributions will be licensed under the [MIT License](https://github.com/Kir-Antipov/Spanned/blob/master/LICENSE.md).
