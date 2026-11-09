# Gosf.Input.Automation

English | [简体中文](README.zh-CN.md)

A C# library for mouse and keyboard automation on Windows. The native backend is derived from the input code in [op 0.4.8.3](https://github.com/WallBreaker2/op). It preserves upstream attribution and license notices.

Requires Windows 10 or later, .NET Framework 4.6.2 or later / .NET 8 or later, and an x86 or x64 calling process. ARM64 processes are not supported. You supply the target window handle; the library does not locate windows.

## Features

| Area | Features |
| --- | --- |
| Input modes | Foreground, background window messages, background DirectInput injection, and foreground keyboard scan codes |
| Binding | Bind/unbind windows, separate display and input handles, and external-input locking in DirectInput mode |
| Mouse | Cursor position/shape, absolute/relative/random/smooth movement, paths, dragging, five buttons, double clicks, and horizontal/vertical scrolling |
| Keyboard | Virtual keys, named keys, Unicode text, key-state queries, and waiting for keys |
| Configuration | Mouse/keyboard delays and trajectory duration, jitter, and pauses |

A `true` result means the backend reported success; it does not guarantee that the target application responded. Sessions are not thread-safe. Serialize calls to each session.

## Installation and usage

Once the package is published:

```powershell
dotnet add package Gosf.Input.Automation
```

```csharp
using System.Drawing;
using Gosf.Input.Automation;

nint hwnd = /* your target window handle */;
using var input = new InputSession();
if (!input.BindWindow(hwnd, MouseMode.WindowMessage, KeyboardMode.WindowMessage))
    throw new InvalidOperationException("Window binding failed.");

if (!input.MoveTo(100, 80) || !input.Click(MouseButton.Left))
    throw new InvalidOperationException("Mouse input failed.");
if (!input.TypeText("Hello, Windows") || !input.KeyPress("enter"))
    throw new InvalidOperationException("Keyboard input failed.");

input.DragPath([new Point(10, 10), new Point(100, 80)], 200);
input.UnbindWindow(); // Dispose also releases the binding.
```

Bound coordinates generally refer to the input window's client area. Unbound foreground input uses screen coordinates. Drag paths need at least two distinct points; durations are in milliseconds, and wheel deltas are normally multiples of 120.

Use `BindWindow(displayHwnd, inputHwnd, ...)` when a child window receives input. `BoundWindow` returns the display handle. Foreground and background modes can be mixed. Foreground keyboard input goes to the focused window; binding does not activate it.

For shortcuts, use `KeyDown("ctrl")`, `KeyPress("a")`, and `KeyUp("ctrl")`. See the upstream [key names](https://github.com/WallBreaker2/op/wiki/keypad). `IsKeyDown` and `WaitForKey` query foreground system state in normal/window-message modes and simulated session state in DirectInput mode. `WaitForKey` returns the virtual-key code or 0 on timeout; key 0 means any key, and timeout 0 checks immediately. Delay settings affect all sessions in the process.

## Compatibility and limitations

- `WindowMessage` sends window messages without changing real keyboard state or activating the target. Applications using Raw Input, DirectInput, global key state, or custom input handling may ignore it.
- `DirectInput` injects the matching native module and installs hooks. Caller and target must have the same bitness; cross-bitness binding throws `PlatformNotSupportedException`. Permissions, protected processes, and application behavior affect compatibility.
- For WPF, use both `MouseMode.DirectInput` and `KeyboardMode.DirectInput`. On `HwndWrapper[` windows, hooks emulate mouse position, focus, capture, and keyboard state on the target UI thread. Input enters the normal WPF message pipeline.
- Local WPF validation covers .NET 8 x86/x64 Button, TextBox, Slider, and ScrollViewer controls. WPF support requires Windows 10 1607 or later. Ordinary keys, Shift/Ctrl, and Unicode text are covered. IME composition, dead keys, AltGr, system shortcuts, cross-HWND popups, HwndHost, multiple UI threads, hidden/minimized windows, and other WPF versions are not supported or have not been validated.
- WPF hooks suppress real mouse messages to the target while bound. Avoid manually interacting with the same window during automation. Unbinding clears virtual capture and focus before removing hooks.
- Windows integrity levels restrict input and messages sent to elevated windows. Run at a suitable permission level. See [PostMessage](https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-postmessagew) and [SendInput](https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-sendinput).

## Build and test

Install Git, .NET 8 SDK, Visual Studio 2022 with C++ build tools, and CMake on Windows. Build the native backends first:

```powershell
./scripts/build-native.ps1
dotnet build Gosf.Input.Automation.sln -c Release
dotnet run --project tests/Gosf.Input.Automation.Tests -c Release --no-build
dotnet run --project tests/Gosf.Input.Automation.Tests -c Release --no-build -- --dx-only
dotnet run --project tests/Gosf.Input.Automation.WpfTests -c Release --no-build
dotnet pack src/Gosf.Input.Automation/Gosf.Input.Automation.csproj -c Release -o artifacts/packages
```

Native dependencies are pinned in CMake. Generated DLLs and packages stay in ignored `artifacts/`. NuGet consumers receive both native architectures and do not need C++ tools.

The package includes `net462` and `net8.0-windows` assemblies. For .NET Framework, NuGet build targets copy the native backends into `native/x86/` and `native/x64/` beside the managed assembly. Keep these directories when deploying. Each contains the original architecture-specific DLL used for injection and a loader alias. The caller's process bitness selects the backend at runtime, including for AnyCPU applications. The public API is the same on both frameworks.

Validate the .NET Framework package with both process architectures:

```powershell
./scripts/test-package.ps1 -PackageDirectory artifacts/packages -Version 0.1.0 -TargetFramework net462 -Platform x86
./scripts/test-package.ps1 -PackageDirectory artifacts/packages -Version 0.1.0 -TargetFramework net462 -Platform x64
```

These tests compile against .NET Framework 4.6.2 reference assemblies and exercise window messages, DirectInput, Unicode, paths, and argument validation in standalone consumers. They run on the installed .NET Framework 4.x runtime; a machine with only the 4.6.2 runtime is not covered by this check. The .NET Framework reference assemblies are restored as a build-only dependency.

The tests are executable regression runners, not `dotnet test` projects. They create temporary windows; DirectInput tests inject only into their own target process. WPF tests assert actual control results and check that the target stays in the background. Use an interactive Windows desktop and avoid touching test windows. Hosted CI runs the WinForms message and DirectInput regressions; WPF desktop validation is a separate local release check.

With the x86 .NET 8 Windows Desktop runtime installed, also run:

```powershell
& "${env:ProgramFiles(x86)}/dotnet/dotnet.exe" tests/Gosf.Input.Automation.WpfTests/bin/Release/net8.0-windows/Gosf.Input.Automation.WpfTests.dll
dotnet run --project tests/Gosf.Input.Automation.Tests -c Release --no-build -- --dx-target-x86
```

## Contributing and releases

See [CONTRIBUTING.md](CONTRIBUTING.md), [security reporting](SECURITY.md), [CHANGELOG.md](CHANGELOG.md), and the [release guide](docs/RELEASING.md). CI builds and packages on pull requests and pushes to `main`/`master`. Publishing a GitHub Release with a matching `v<Version>` tag runs the NuGet publishing workflow.

## License and native sources

Project-owned code is MIT licensed; see [LICENSE](LICENSE). Native code and dependencies retain their own terms, including LGPL-3.0-or-later for WOW64Ext. See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) and `licenses/`.

Each automated release attaches `Gosf.Input.Automation.<version>.native-sources.zip`, containing this project's build sources and the native dependency sources used by the build. See [native rebuilding instructions](docs/NATIVE-REBUILD.md) for modifying and rebuilding the native modules. Distribution review remains part of the release checklist.
