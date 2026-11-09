# Gosf.Input.Automation

[English](README.md) | 简体中文

Windows 鼠标、键盘自动化 C# 库。原生后端基于 [op 0.4.8.3](https://github.com/WallBreaker2/op) 的输入源码裁剪，保留原始来源和许可声明。仓库只保存源码；构建出的 `Gosf.Input.Automation.Native.x86.dll` 和 `Gosf.Input.Automation.Native.x64.dll` 位于已忽略的 `artifacts/`，打包时才进入 NuGet 包。原生源码位于 `src/Gosf.Input.Automation.Native/core`。

目标平台：Windows 10 或更新版本、.NET Framework 4.6.2 或更新版本 / .NET 8 或更新版本；调用进程须为 x86 或 x64。需要用户自己的目标窗口句柄，库不负责查找窗口。

## 支持的功能

| 类别 | 功能 |
| --- | --- |
| 模式 | 前台 `Foreground`；后台窗口消息 `WindowMessage`；后台注入 `DirectInput`；键盘还支持前台扫描码 `ScanCode` |
| 绑定 | 绑定和解绑窗口，可分别指定显示窗口和接收输入的窗口；DX 模式支持锁定目标窗口的外部输入 |
| 鼠标 | 位置和形状；绝对、相对、随机、平滑及路径移动；路径拖拽；左、中、右、两个侧键的按下、释放、单击和双击；水平/垂直滚轮 |
| 键盘 | 按虚拟键码或名称按下、释放、单击；Unicode 文本；前台按键状态查询和等待 |
| 配置 | 点击和按键延迟；轨迹类型、耗时、抖动与停顿 |

支持情况以目标窗口对输入的处理方式为准；API 返回 `true` 仅表示后端报告操作成功，不保证应用已响应。所有输入会话均非线程安全，调用方应串行访问。

## 使用

打包发布后：

```powershell
dotnet add package Gosf.Input.Automation
```

```csharp
using System.Drawing;
using Gosf.Input.Automation;

nint hwnd = /* 接收输入的窗口句柄 */;
using var input = new InputSession();
if (!input.BindWindow(hwnd, MouseMode.WindowMessage, KeyboardMode.WindowMessage))
    throw new InvalidOperationException("窗口绑定失败");

if (!input.MoveTo(100, 80) || !input.Click(MouseButton.Left))
    throw new InvalidOperationException("鼠标操作失败");
if (!input.TypeText("你好，Windows") || !input.KeyPress("enter"))
    throw new InvalidOperationException("键盘操作失败");

input.DragPath([new Point(10, 10), new Point(100, 80)], 200);
input.UnbindWindow(); // Dispose 也会释放绑定资源
```

`MoveTo` 等绑定后的坐标通常相对输入窗口客户区；未绑定时前台模式使用屏幕坐标。拖拽路径至少包含两个不同的点，耗时单位为毫秒；滚轮增量通常为 120 的倍数。可用 `BindWindow(displayHwnd, inputHwnd, ...)` 指定两个窗口句柄，实际接收消息的子窗口需由调用方确定；`BoundWindow` 返回显示窗口句柄。

前台和后台可以混合使用，例如 `BindWindow(hwnd, MouseMode.Foreground, KeyboardMode.WindowMessage)`。未绑定的新会话默认使用前台输入，前台输入作用于当前桌面，键盘输入会进入实际拥有焦点的窗口，不会自动激活绑定窗口。

键盘组合键可调用 `KeyDown("ctrl")`、`KeyPress("a")`、`KeyUp("ctrl")`，名称参见 [op 按键表](https://github.com/WallBreaker2/op/wiki/keypad)。`IsKeyDown` 和 `WaitForKey` 在普通/窗口消息模式查询系统前台状态，在 DX 模式查询当前会话模拟的按键状态。`WaitForKey` 返回按下的虚拟键码，超时返回 0；按键码为 0 表示任意键，超时为 0 表示立即检查。`SetMouseDelay` 和 `SetKeyboardDelay` 是 op 后端的进程级设置，会影响同进程的其他会话。

## 后台模式限制

- `WindowMessage` 通过窗口消息输入，不需要切换前台，也不会改变真实键盘状态。目标必须处理相应消息；使用 Raw Input、DirectInput、全局按键状态或自定义输入的程序可能忽略它。
- `DirectInput` 对应 op 的 `dx` 模式，会向目标进程注入对应位数的 `Gosf.Input.Automation.Native` 模块并使用 hook，可能支持部分 Raw Input/DirectInput 场景；它不是硬件输入，受目标进程权限、保护机制和具体实现影响，不保证兼容所有程序。调用进程和目标进程必须同为 x86 或同为 x64；跨位数 DX 可能卡住或导致目标进程退出，库会抛出 `PlatformNotSupportedException`。窗口消息模式不受此限制。
- WPF 请同时使用 `MouseMode.DirectInput` 和 `KeyboardMode.DirectInput`。绑定 `HwndWrapper[` 窗口时，自动启用目标 UI 线程的鼠标位置、焦点、鼠标捕获和键盘状态 Hook，再通过正常窗口消息进入 WPF 输入管线。没有使用 UI Automation、反射调用控件方法或直接修改控件值，也没有引入 EasyHook。`WindowMessage` 模式没有这些状态模拟，不能据此保证 WPF 响应。
- WPF 适配已在本机 .NET 8 的 x86/x64 独立进程验证普通 Button、TextBox、Slider、ScrollViewer。需要 Windows 10 1607 或更新版本；按键转字符使用 `ToUnicodeEx` 的不修改系统键盘缓冲区选项。支持普通按键、Shift/Ctrl 和直接 Unicode 文本；IME 组合输入、死键、AltGr、系统快捷键尚未支持或验证。跨 HWND 弹出菜单、HwndHost、多个 UI 线程、最小化/隐藏窗口及其他 WPF 版本尚未验证。
- WPF Hook 在绑定期间为目标 UI 线程维护独立输入状态，会屏蔽目标窗口收到的真实鼠标消息以防模拟位置被覆盖；不适合同时手动操作同一目标窗口。解绑会在目标线程同步取消虚拟捕获、清理焦点，再移除 Hook。它仍是软件模拟，并不保证目标无法辨别输入来源。
- x86 输入 Hook 的导出名称通过原生项目的 `hook-exports-x86.def` 提供，供 DX 模式解析。
- Windows 的权限隔离会限制对更高完整性级别窗口的消息与输入操作。请在合适的权限级别下运行。参见 [PostMessage](https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-postmessagew) 和 [SendInput](https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-sendinput)。
- 有些窗口对 Unicode 文本或后台组合键有自己的处理规则；目标接收键盘消息的句柄可能与显示窗口不同。

## 构建、测试和打包

GitHub Actions 的构建与 NuGet 发布步骤见 [发布指南](docs/RELEASING.md)。发布时会附带原生依赖源码归档；修改依赖并离线重建的方法见 [原生重建说明](docs/NATIVE-REBUILD.md)。英文版本更新记录见 [CHANGELOG.md](CHANGELOG.md)。

从源码构建时，先构建原生后端。需要 Windows、Git、Visual Studio 2022 C++ 工具和 CMake；脚本从固定的源码版本获取 BlackBone 和 MinHook，在 `artifacts/gosf-input-automation-native-x86/Release/` 和 `artifacts/gosf-input-automation-native-x64/Release/` 生成 DLL：

```powershell
./scripts/build-native.ps1
```

然后构建、测试和打包 .NET 项目。源码仓库没有预编译 DLL；直接执行 `dotnet build` 会提示先运行上述脚本。NuGet 包会包含这两份生成的 DLL，使用 NuGet 包的项目不需要 C++ 构建工具。

NuGet 包同时包含 `net462` 和 `net8.0-windows` 程序集，公共 API 一致。.NET Framework 项目通过包内 MSBuild targets 自动复制原生后端到程序集旁的 `native/x86/`、`native/x64/`，部署时需保留这两个目录及其中的 DLL；运行时按进程位数自动选择。每个目录同时包含注入使用的原始文件名和供 Framework 加载的别名。构建时通过仅开发依赖的引用程序集包获取 .NET Framework 4.6.2 API，无需安装对应 Developer Pack。

打包后可验证 Framework 的双架构输入和包消费：

```powershell
./scripts/test-package.ps1 -PackageDirectory artifacts/packages -Version 0.1.0 -TargetFramework net462 -Platform x86
./scripts/test-package.ps1 -PackageDirectory artifacts/packages -Version 0.1.0 -TargetFramework net462 -Platform x64
```

上述测试针对 4.6.2 引用程序集编译，并在本机安装的 Framework 4.x 运行时执行窗口消息、DirectInput、Unicode、路径及参数校验；不代表已在仅安装 4.6.2 的系统验证。WPF 控件回归的验证范围仍为下述 .NET 8 测试。

```powershell
dotnet build Gosf.Input.Automation.sln -c Release
dotnet run --project tests/Gosf.Input.Automation.Tests -c Release
dotnet run --project tests/Gosf.Input.Automation.Tests -c Release -- --dx
# 只执行后台 DX 回归，跳过其他测试流程
dotnet run --project tests/Gosf.Input.Automation.Tests -c Release -- --dx-only
dotnet run --project tests/Gosf.Input.Automation.Tests -c Release -- --dx-target-x86
dotnet run --project tests/Gosf.Input.Automation.WpfTests -c Release
dotnet pack src/Gosf.Input.Automation/Gosf.Input.Automation.csproj -c Release -o artifacts
```

常规测试使用本地临时窗口；`--dx` 会启动本项目的测试子进程并向其注入 Gosf 原生模块；`--dx-only` 仅运行后台 DX 测试。`--dx-target-x86` 验证 x64 调用进程对跨位数 DX 的拦截，要求已安装 x86 .NET 8 Windows Desktop 运行时。

WPF 测试默认使用 `DirectInput`，只测试后台。控制器启动独立 WPF 子进程，窗口以 `ShowActivated=false` 显示；目标进程只提供控件位置与结果快照，不接收控件操作指令。测试断言 Button.Click、实际文本、Shift+B、Backspace、Ctrl+A、Unicode、滑块拖动、滚轮、解绑清理与再次绑定，并持续检查窗口激活次数及外部前台窗口。任何控件结果不符都会以非零退出；不再把 API 返回成功当作测试通过。测试不发送系统前台输入，但会显示测试窗口，请勿手动操作该窗口。

用已安装的 x86 .NET Desktop 运行时验证 32 位后台输入：

```powershell
& "${env:ProgramFiles(x86)}/dotnet/dotnet.exe" tests/Gosf.Input.Automation.WpfTests/bin/Release/net8.0-windows/Gosf.Input.Automation.WpfTests.dll
```

打包后可将 `artifacts/Gosf.Input.Automation.*.nupkg` 发布到 NuGet；发布者应先确认包 ID、版本、作者信息、仓库地址和 NuGet 账户权限。

项目已配置 [GitHub 仓库](https://github.com/gaoshang212/Gosf.Input.Automation)作为 NuGet 包的仓库和项目链接；如需调整包作者，可在打包时传入 `-p:Authors=<作者>`。完成下方第三方许可核查后，通过 GitHub Release 触发发布：工作流使用 NuGet Trusted Publishing（OIDC）获取短期凭据，无需保存长期 API key。配置 nuget.org 信任策略后，登录用户名默认使用 GitHub 仓库 owner；若与 NuGet 用户名不同，可通过仓库或组织级变量 `NUGET_USER` 覆盖，具体步骤见 [发布指南](docs/RELEASING.md)。

## 许可证

本项目的 C# 代码采用 MIT 许可证，见 [LICENSE](LICENSE)。原生后端衍生自 MIT 许可的 op 源码，原始版权声明及当前构建所用依赖的许可见 [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) 与 `licenses/`。`licenses/` 仅保留 op、BlackBone、AsmJit、MinHook、WOW64Ext 的授权文本，以及 LGPL-3.0 所引用的 GPL-3.0 文本。原生后端的静态依赖涉及 LGPL；公开分发前仍须核对精确源码版本及允许重新链接的材料。
