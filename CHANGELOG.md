# Changelog

All notable changes to this project will be documented here.
Versions follow [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Added

- Initial .NET 8 Windows API for foreground, window-message, DirectInput, and scan-code input.
- Window binding with separate display and input handles and cross-bitness DirectInput validation.
- Mouse buttons, scrolling, cursor queries, smooth movement, paths, and dragging.
- Virtual-key and named-key input, Unicode text, key-state queries, and configurable delays.
- Background WPF input hooks and standalone WinForms and WPF regression runners.
- Native x86 and x64 backends derived from op 0.4.8.3 with pinned dependencies.
- NuGet packaging, Windows CI, release publishing through Trusted Publishing (GitHub OIDC), and corresponding native source archives.
- English documentation, contribution guidance, security reporting guidance, and issue templates.

The first planned package version is `0.1.0`. No public release has been recorded yet.
