# Contributing

Bug reports, documentation improvements, and focused pull requests are welcome.

## Development

Use Windows 10 or later, .NET 8 SDK, Git, Visual Studio 2022 C++ tools, and CMake. Follow the build and test commands in [README.md](README.md). The test projects are console regression runners; `dotnet test` does not execute them.

Keep changes focused. Describe the problem, resulting behavior, and validation in your pull request. Keep business entry points as short orchestration methods, extract cohesive steps, and avoid methods longer than 25 lines or condition nesting deeper than two levels. Preserve upstream copyright and license notices.

Use English for new public documentation, changelog entries, exception messages, and diagnostics. Update the Chinese README when changing documented behavior. Add an entry under `Unreleased` in [CHANGELOG.md](CHANGELOG.md) for user-visible changes. Do not commit generated binaries, packages, build output, credentials, or machine-specific paths.

## Validation

Run the normal WinForms runner, `--dx-only`, and the WPF runner on an interactive desktop. Changes to native code or packaging should be checked on both x86 and x64, including package consumption from a separate application. Tests must assert observed behavior, not only successful API return values.

## Reporting problems

Include the library version, Windows/.NET versions, caller and target bitness, input mode, a minimal reproduction, and expected/observed results. Remove private window titles, typed text, and credentials from logs. See [SECURITY.md](SECURITY.md) for vulnerabilities.

By contributing, you agree that your contributions are provided under this project's applicable licenses. Be respectful and keep discussion focused on the work.
