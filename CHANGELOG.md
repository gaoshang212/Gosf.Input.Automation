# Changelog

All notable changes to this project will be documented here.
Versions follow [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [0.2.0] - 2026-11-27

### Added

- Support for .NET 6 Windows applications. NuGet now ships `net462` and `net6.0-windows` assemblies. .NET 8 Windows applications automatically use the `net6.0-windows` assembly.

## [0.1.1] - 2026-11-19

### Fixed

- Call hook exports directly when binding to a window in the current process.
- Keep the native module loaded after unbinding without releasing a loader-owned reference or terminating the caller's thread.
- Preserve failed cleanup state for retry, propagate mouse and keyboard unbind failures, and reject further input until cleanup succeeds.
- Cancel unsupported WPF OLE drag-and-drop during virtual input and report the triggering input as failed instead of entering the desktop drag loop.

## [0.1.0] - 2026-11-09

### Added

- .NET Framework 4.6.2 target with automatic x86/x64 native deployment, Framework-compatible loading, and packaged input regressions on both architectures.
- Initial .NET 8 Windows API for foreground, window-message, DirectInput, and scan-code input.
- Window binding with separate display and input handles and cross-bitness DirectInput validation.
- Mouse buttons, scrolling, cursor queries, smooth movement, paths, and dragging.
- Virtual-key and named-key input, Unicode text, key-state queries, and configurable delays.
- Background WPF input hooks and standalone WinForms and WPF regression runners.
- Native x86 and x64 backends derived from op 0.4.8.3 with pinned dependencies.
- NuGet packaging, Windows CI, release publishing through Trusted Publishing (GitHub OIDC), and corresponding native source archives.
- English documentation, contribution guidance, security reporting guidance, and issue templates.
