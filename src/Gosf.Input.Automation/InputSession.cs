using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Gosf.Input.Automation;

/// <summary>One op input session. Instances must not be used concurrently.</summary>
public sealed class InputSession : IDisposable
{
    private readonly AutomationHandle handle;

    public InputSession()
    {
        handle = NativeMethods.GosfCreate();
        if (handle.IsInvalid)
        {
            handle.Dispose();
            throw new InvalidOperationException("Could not create an op input session.");
        }
    }

    public static string BackendVersion => Marshal.PtrToStringUni(NativeMethods.GosfVer()) ?? "";
    public bool IsBound => NativeMethods.GosfIsBind(handle) == 1;
    public nint BoundWindow => NativeMethods.GosfGetBindWindow(handle);

    public bool BindWindow(nint window, MouseMode mouse, KeyboardMode keyboard) =>
        BindWindow(window, window, mouse, keyboard);

    public bool BindWindow(nint displayWindow, nint inputWindow, MouseMode mouse, KeyboardMode keyboard)
    {
        if (displayWindow == 0) throw new ArgumentException("A display window handle is required.", nameof(displayWindow));
        if (inputWindow == 0) throw new ArgumentException("An input window handle is required.", nameof(inputWindow));
        if ((mouse == MouseMode.DirectInput || keyboard == KeyboardMode.DirectInput) && !CheckDirectInputTarget(inputWindow))
            return false;
        return NativeMethods.GosfBindWindowEx(handle, displayWindow, inputWindow, "normal", MouseModeName(mouse), KeyboardModeName(keyboard), 0) == 1;
    }

    public bool UnbindWindow() => NativeMethods.GosfUnBindWindow(handle) == 1;

    public bool LockInput(InputLock mode)
    {
        if (!Enum.IsDefined(typeof(InputLock), mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        return NativeMethods.GosfLockInput(handle, (int)mode) == 1;
    }

    public Point GetCursorPosition()
    {
        if (NativeMethods.GosfGetCursorPos(handle, out int x, out int y) != 1)
            throw new InvalidOperationException("Could not read the cursor position.");
        return new Point(x, y);
    }

    public string GetCursorShape() => Marshal.PtrToStringUni(NativeMethods.GosfGetCursorShape(handle)) ?? "";
    public bool MoveTo(int x, int y) => NativeMethods.GosfMoveTo(handle, x, y) == 1;
    public bool MoveBy(int x, int y) => NativeMethods.GosfMoveR(handle, x, y) == 1;
    public bool MoveToSmooth(int x, int y, int durationMilliseconds = 0) =>
        NativeMethods.GosfMoveToSmooth(handle, x, y, NonNegative(durationMilliseconds)) == 1;
    public Point? MoveToRandom(int x, int y, int width, int height) =>
        ParsePoint(NativeMethods.GosfMoveToEx(handle, x, y, width, height));
    public Point? MoveToRandomSmooth(int x, int y, int width, int height, int durationMilliseconds = 0) =>
        ParsePoint(NativeMethods.GosfMoveToExSmooth(handle, x, y, width, height, NonNegative(durationMilliseconds)));
    public bool MovePath(IReadOnlyList<Point> points, int durationMilliseconds = 0) =>
        NativeMethods.GosfMovePath(handle, FormatPath(points), NonNegative(durationMilliseconds)) == 1;
    public bool DragPath(IReadOnlyList<Point> points, int durationMilliseconds = 0) =>
        NativeMethods.GosfDragPath(handle, FormatPath(points), NonNegative(durationMilliseconds)) == 1;

    public bool SetMouseTrajectory(TrajectoryMode mode, int minDurationMilliseconds, int maxDurationMilliseconds,
        int jitter, int startDelayMilliseconds, int endDelayMilliseconds)
    {
        if (!Enum.IsDefined(typeof(TrajectoryMode), mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        NonNegative(minDurationMilliseconds); NonNegative(maxDurationMilliseconds);
        NonNegative(startDelayMilliseconds); NonNegative(endDelayMilliseconds);
        if (maxDurationMilliseconds != 0 && maxDurationMilliseconds < minDurationMilliseconds)
            throw new ArgumentOutOfRangeException(nameof(maxDurationMilliseconds));
        if (jitter is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(jitter));
        return NativeMethods.GosfSetMouseTrajectory(handle, (int)mode, minDurationMilliseconds, maxDurationMilliseconds,
            jitter, startDelayMilliseconds, endDelayMilliseconds) == 1;
    }

    public bool SetMouseDelay(MouseMode mode, int milliseconds) =>
        NativeMethods.GosfSetMouseDelay(handle, MouseModeName(mode), NonNegative(milliseconds)) == 1;
    public bool SetKeyboardDelay(KeyboardMode mode, int milliseconds) =>
        NativeMethods.GosfSetKeypadDelay(handle, KeyboardModeName(mode), NonNegative(milliseconds)) == 1;

    public bool Click(MouseButton button) => (button switch
    {
        MouseButton.Left => NativeMethods.GosfLeftClick(handle),
        MouseButton.Middle => NativeMethods.GosfMiddleClick(handle),
        MouseButton.Right => NativeMethods.GosfRightClick(handle),
        MouseButton.XButton1 => NativeMethods.GosfXButton1Click(handle),
        MouseButton.XButton2 => NativeMethods.GosfXButton2Click(handle),
        _ => throw new ArgumentOutOfRangeException(nameof(button))
    }) == 1;

    public bool DoubleClick(MouseButton button) => (button switch
    {
        MouseButton.Left => NativeMethods.GosfLeftDoubleClick(handle),
        MouseButton.Middle => NativeMethods.GosfMiddleDoubleClick(handle),
        MouseButton.Right => NativeMethods.GosfRightDoubleClick(handle),
        MouseButton.XButton1 => NativeMethods.GosfXButton1DoubleClick(handle),
        MouseButton.XButton2 => NativeMethods.GosfXButton2DoubleClick(handle),
        _ => throw new ArgumentOutOfRangeException(nameof(button))
    }) == 1;

    public bool MouseDown(MouseButton button) => (button switch
    {
        MouseButton.Left => NativeMethods.GosfLeftDown(handle),
        MouseButton.Middle => NativeMethods.GosfMiddleDown(handle),
        MouseButton.Right => NativeMethods.GosfRightDown(handle),
        MouseButton.XButton1 => NativeMethods.GosfXButton1Down(handle),
        MouseButton.XButton2 => NativeMethods.GosfXButton2Down(handle),
        _ => throw new ArgumentOutOfRangeException(nameof(button))
    }) == 1;

    public bool MouseUp(MouseButton button) => (button switch
    {
        MouseButton.Left => NativeMethods.GosfLeftUp(handle),
        MouseButton.Middle => NativeMethods.GosfMiddleUp(handle),
        MouseButton.Right => NativeMethods.GosfRightUp(handle),
        MouseButton.XButton1 => NativeMethods.GosfXButton1Up(handle),
        MouseButton.XButton2 => NativeMethods.GosfXButton2Up(handle),
        _ => throw new ArgumentOutOfRangeException(nameof(button))
    }) == 1;

    public bool Scroll(int delta) => NativeMethods.GosfWheel(handle, delta) == 1;
    public bool ScrollHorizontal(int delta) => NativeMethods.GosfHWheel(handle, delta) == 1;

    public bool IsKeyDown(int virtualKey) => NativeMethods.GosfGetKeyState(handle, Key(virtualKey)) == 1;
    public int WaitForKey(int virtualKey, int timeoutMilliseconds = 0)
    {
        if (virtualKey != 0) Key(virtualKey);
        return NativeMethods.GosfWaitKey(handle, virtualKey, NonNegative(timeoutMilliseconds));
    }
    public bool KeyDown(int virtualKey) => NativeMethods.GosfKeyDown(handle, Key(virtualKey)) == 1;
    public bool KeyUp(int virtualKey) => NativeMethods.GosfKeyUp(handle, Key(virtualKey)) == 1;
    public bool KeyPress(int virtualKey) => NativeMethods.GosfKeyPress(handle, Key(virtualKey)) == 1;
    public bool KeyDown(string name) => NativeMethods.GosfKeyDownChar(handle, Text(name, nameof(name))) == 1;
    public bool KeyUp(string name) => NativeMethods.GosfKeyUpChar(handle, Text(name, nameof(name))) == 1;
    public bool KeyPress(string name) => NativeMethods.GosfKeyPressChar(handle, Text(name, nameof(name))) == 1;
    public bool TypeText(string text, int delayMilliseconds = 0)
    {
        Text(text, nameof(text));
        NonNegative(delayMilliseconds);
        if (handle.IsClosed) throw new ObjectDisposedException(nameof(InputSession));
        return text.Length == 0 || NativeMethods.GosfKeyPressStr(handle, text, delayMilliseconds) == 1;
    }

    public void Dispose() => handle.Dispose();

    private static int Key(int value) => value is > 0 and <= 255 ? value : throw new ArgumentOutOfRangeException(nameof(value));
    private static int NonNegative(int value) => value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
    private static string Text(string value, string name)
    {
        if (value == null) throw new ArgumentNullException(name);
        if (value.IndexOf('\0') >= 0) throw new ArgumentException("Embedded NUL is not supported.", name);
        return value;
    }

    private static string MouseModeName(MouseMode mode) => mode switch
    {
        MouseMode.Foreground => "normal", MouseMode.WindowMessage => "windows", MouseMode.DirectInput => "dx",
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };
    private static string KeyboardModeName(KeyboardMode mode) => mode switch
    {
        KeyboardMode.Foreground => "normal", KeyboardMode.ScanCode => "normal.hd",
        KeyboardMode.WindowMessage => "windows", KeyboardMode.DirectInput => "dx",
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };

    private static Point? ParsePoint(nint pointer)
    {
        string[] parts = (Marshal.PtrToStringUni(pointer) ?? "").Split(',');
        return parts.Length == 2 && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int x)
            && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int y) ? new Point(x, y) : null;
    }

    private static string FormatPath(IReadOnlyList<Point> points)
    {
        if (points == null) throw new ArgumentNullException(nameof(points));
        if (points.Count < 2 || points.All(p => p == points[0]))
            throw new ArgumentException("A path must contain at least two different points.", nameof(points));
        return string.Join("|", points.Select(p => FormattableString.Invariant($"{p.X},{p.Y}")));
    }

    private static bool CheckDirectInputTarget(nint window)
    {
        // shortcut: op cross-bitness DX can hang or crash, lift this guard after upstream fixes it.
        if (!IsDirectInputOperatingSystemSupported())
            throw new PlatformNotSupportedException("DirectInput requires an x86 or x64 Windows installation.");
        if (NativeMethods.GetWindowThreadProcessId(window, out uint processId) == 0) return false;
        using var process = NativeMethods.OpenProcess(0x1000, false, processId);
        if (process.IsInvalid || !NativeMethods.IsWow64Process(process, out bool wow64)) return false;
        bool targetIs64Bit = Environment.Is64BitOperatingSystem && !wow64;
        if (targetIs64Bit != Environment.Is64BitProcess)
            throw new PlatformNotSupportedException("DirectInput requires the caller and target to have the same bitness.");
        return true;
    }

    private static bool IsDirectInputOperatingSystemSupported()
    {
#if NETFRAMEWORK
        return FrameworkNativeLoader.IsSupportedOSArchitecture();
#else
        return RuntimeInformation.OSArchitecture is Architecture.X86 or Architecture.X64;
#endif
    }
}
