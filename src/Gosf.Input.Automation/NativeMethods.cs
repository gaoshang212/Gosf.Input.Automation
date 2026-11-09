using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Gosf.Input.Automation;

internal sealed class AutomationHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private AutomationHandle() : base(true) { }
    protected override bool ReleaseHandle() { NativeMethods.GosfDestroy(handle); return true; }
}

internal static class NativeMethods
{
    private const string Library = "Gosf.Input.Automation.Native";

    static NativeMethods() => NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, Resolve);

    private static nint Resolve(string name, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (name != Library) return 0;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Windows is required.");
        string native = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "Gosf.Input.Automation.Native.x64.dll",
            Architecture.X86 => "Gosf.Input.Automation.Native.x86.dll",
            _ => throw new PlatformNotSupportedException("Only Windows x86 and x64 processes are supported.")
        };
        return NativeLibrary.Load(native, assembly, searchPath);
    }

    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern AutomationHandle GosfCreate();
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern void GosfDestroy(nint handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern nint GosfVer();
    [DllImport(Library, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)] internal static extern int GosfBindWindowEx(AutomationHandle handle, nint display, nint input, string capture, string mouse, string keyboard, int mode);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfUnBindWindow(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfIsBind(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern nint GosfGetBindWindow(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfLockInput(AutomationHandle handle, int mode);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfGetCursorPos(AutomationHandle handle, out int x, out int y);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern nint GosfGetCursorShape(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfMoveR(AutomationHandle handle, int x, int y);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfMoveTo(AutomationHandle handle, int x, int y);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern nint GosfMoveToEx(AutomationHandle handle, int x, int y, int width, int height);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfMoveToSmooth(AutomationHandle handle, int x, int y, int duration);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern nint GosfMoveToExSmooth(AutomationHandle handle, int x, int y, int width, int height, int duration);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)] internal static extern int GosfMovePath(AutomationHandle handle, string path, int duration);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)] internal static extern int GosfDragPath(AutomationHandle handle, string path, int duration);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfSetMouseTrajectory(AutomationHandle handle, int mode, int minDuration, int maxDuration, int jitter, int startDelay, int endDelay);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)] internal static extern int GosfSetMouseDelay(AutomationHandle handle, string mode, int delay);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfLeftClick(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfLeftDoubleClick(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfLeftDown(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfLeftUp(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfMiddleClick(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfMiddleDoubleClick(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfMiddleDown(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfMiddleUp(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfRightClick(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfRightDoubleClick(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfRightDown(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfRightUp(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfXButton1Click(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfXButton1DoubleClick(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfXButton1Down(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfXButton1Up(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfXButton2Click(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfXButton2DoubleClick(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfXButton2Down(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfXButton2Up(AutomationHandle handle);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfWheel(AutomationHandle handle, int delta);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfHWheel(AutomationHandle handle, int delta);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfGetKeyState(AutomationHandle handle, int key);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfWaitKey(AutomationHandle handle, int key, int timeout);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfKeyDown(AutomationHandle handle, int key);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfKeyUp(AutomationHandle handle, int key);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall)] internal static extern int GosfKeyPress(AutomationHandle handle, int key);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)] internal static extern int GosfKeyDownChar(AutomationHandle handle, string key);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)] internal static extern int GosfKeyUpChar(AutomationHandle handle, string key);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)] internal static extern int GosfKeyPressChar(AutomationHandle handle, string key);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)] internal static extern int GosfKeyPressStr(AutomationHandle handle, string text, int delay);
    [DllImport(Library, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)] internal static extern int GosfSetKeypadDelay(AutomationHandle handle, string mode, int delay);

    [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(nint window, out uint processId);
    [DllImport("kernel32.dll", SetLastError = true)] internal static extern SafeProcessHandle OpenProcess(uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint processId);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool IsWow64Process(SafeProcessHandle process, [MarshalAs(UnmanagedType.Bool)] out bool wow64);
}
