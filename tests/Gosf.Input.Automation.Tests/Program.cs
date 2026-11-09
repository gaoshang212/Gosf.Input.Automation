using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using Gosf.Input.Automation;

if (args.Contains("--target"))
{
    Application.Run(new RecorderWindow(true));
    return;
}

if (args.Contains("--dx-only"))
{
    TestDx(false, false);
    Console.WriteLine("PASS: background-only DX regression" );
    return;
}

var ready = new TaskCompletionSource<RecorderWindow>(TaskCreationOptions.RunContinuationsAsynchronously);
var ui = new Thread(() =>
{
    using var form = new RecorderWindow(false);
    form.Shown += (_, _) => ready.SetResult(form);
    Application.Run(form);
});
ui.SetApartmentState(ApartmentState.STA);
ui.Start();
RecorderWindow window = await ready.Task.WaitAsync(TimeSpan.FromSeconds(10));
try
{
    Assert(InputSession.BackendVersion == "0.4.8.3", "native backend version");
    using var input = new InputSession();
    Assert(!input.IsBound && input.BoundWindow == 0, "new session is unbound");
    Throws<ArgumentException>(() => input.BindWindow(0, MouseMode.WindowMessage, KeyboardMode.WindowMessage));
    Throws<ArgumentOutOfRangeException>(() => input.KeyPress(256));
    Throws<ArgumentOutOfRangeException>(() => input.MoveToSmooth(1, 1, -1));
    Throws<ArgumentException>(() => input.TypeText("a\0b"));
    Throws<ArgumentException>(() => input.MovePath([new Point(1, 1), new Point(1, 1)]));
    Assert(!input.BindWindow((nint)(-1), MouseMode.WindowMessage, KeyboardMode.WindowMessage), "invalid window is rejected");
    Assert(!input.IsBound && input.BoundWindow == 0, "failed bind resets state");
    _ = input.GetCursorPosition();
    Assert(input.BindWindow(window.Handle, MouseMode.WindowMessage, KeyboardMode.WindowMessage), "bind window");
    Assert(input.IsBound && input.BoundWindow == window.Handle, "binding state");
    string shape = input.GetCursorShape();
    Assert(shape.Length == 0 || shape.Split(',').Length == 6, "cursor shape format");
    Assert(!input.LockInput(InputLock.MouseAndKeyboard), "input lock requires dx");
    Assert(input.SetMouseDelay(MouseMode.WindowMessage, 1), "mouse delay");
    Assert(input.SetKeyboardDelay(KeyboardMode.WindowMessage, 1), "keyboard delay");
    Assert(input.MoveTo(20, 30) && input.MoveBy(2, 3), "mouse movement");
    Assert(input.MoveToRandom(10, 10, 3, 3) is { X: >= 10 and <= 13, Y: >= 10 and <= 13 }, "random move");
    Assert(input.MoveToRandomSmooth(10, 10, 3, 3, 1) is not null, "random smooth move");
    Assert(input.SetMouseTrajectory(TrajectoryMode.Straight, 0, 0, 0, 0, 0), "trajectory");
    Assert(input.MoveToSmooth(15, 15, 1), "smooth move");
    Assert(input.MovePath([new Point(1, 1), new Point(5, 5)], 1), "path");
    Assert(input.DragPath([new Point(1, 1), new Point(5, 5)], 1), "drag path");
    foreach (var button in Enum.GetValues<MouseButton>())
        Assert(input.MouseDown(button) && input.MouseUp(button) && input.Click(button) && input.DoubleClick(button), $"{button} actions");
    Assert(input.Scroll(120) && input.ScrollHorizontal(-120), "mouse wheel");
    Assert(input.KeyDown(0x41) && input.KeyUp(0x41) && input.KeyPress(0x42), "virtual keys");
    Assert(input.KeyDown("ctrl") && input.KeyUp("ctrl") && input.KeyPress("enter"), "named keys");
    Assert(input.TypeText("测试A🙂", 1), "unicode text");
    Assert(input.TypeText(""), "empty text");
    Assert(window.Messages.Any(m => m.Msg == 0x0200), "window received mouse move");
    Assert(window.Messages.Any(m => m.Msg == 0x0201), "window received left button");
    Assert(window.Messages.Any(m => m.Msg == 0x020B), "window received side button");
    Assert(window.Messages.Any(m => m.Msg == 0x020A), "window received vertical wheel");
    Assert(window.Messages.Any(m => m.Msg == 0x020E), "window received horizontal wheel");
    Assert(window.Messages.Any(m => m.Msg == 0x0100 && m.WParam == 0x41), "window received key down");
    Assert(window.Messages.Any(m => m.Msg == 0x0102 && m.WParam == '测'), "window received unicode text");
    Assert(input.UnbindWindow() && !input.IsBound, "unbind");
    Assert(input.BindWindow(window.Handle, window.Child.Handle, MouseMode.WindowMessage, KeyboardMode.WindowMessage), "bind separate input window");
    Assert(input.BoundWindow == window.Handle, "bound window is the display window");
    Assert(input.MoveTo(5, 5) && input.Click(MouseButton.Left) && input.TypeText("子"), "child input");
    Assert(window.Child.Messages.Any(m => m.Msg == 0x0201), "child received mouse click");
    Assert(window.Child.Messages.Any(m => m.Msg == 0x0102 && m.WParam == '子'), "child received text");
    Assert(input.UnbindWindow(), "child unbind");
    Assert(input.BindWindow(window.Handle, MouseMode.Foreground, KeyboardMode.ScanCode), "foreground/scancode binding");
    Assert(input.UnbindWindow(), "foreground unbind");
    input.Dispose();
    Throws<ObjectDisposedException>(() => input.TypeText(""));

    if (args.Contains("--dx") || args.Contains("--dx-target-x86") || args.Contains("--dx-target-x64"))
        TestDx(args.Contains("--dx-target-x86"), args.Contains("--dx-target-x64"));
    Console.WriteLine($"PASS: {RuntimeInformationText()} input tests");
}
finally
{
    window.BeginInvoke(window.Close);
    ui.Join(TimeSpan.FromSeconds(10));
}

static void TestDx(bool targetX86, bool targetX64)
{
    string executable = targetX86
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "dotnet", "dotnet.exe")
        : targetX64
            ? Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432") ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "dotnet.exe")
            : Environment.ProcessPath!;
    var info = new ProcessStartInfo(executable) { UseShellExecute = false, RedirectStandardOutput = true };
    if (Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
        info.ArgumentList.Add(typeof(RecorderWindow).Assembly.Location);
    info.ArgumentList.Add("--target");
    using var target = Process.Start(info) ?? throw new InvalidOperationException("Could not start DX target.");
    try
    {
        string? line = target.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(15)).GetAwaiter().GetResult();
        Assert(long.TryParse(line, out long hwnd), "DX target handle");
        using var input = new InputSession();
        if ((targetX86 && Environment.Is64BitProcess) || (targetX64 && !Environment.Is64BitProcess))
        {
            Throws<PlatformNotSupportedException>(() => input.BindWindow((nint)hwnd, MouseMode.DirectInput, KeyboardMode.DirectInput));
            Console.WriteLine("DX: cross-bitness binding rejected");
            return;
        }
        Console.WriteLine("DX: binding target");
        Assert(input.BindWindow((nint)hwnd, MouseMode.DirectInput, KeyboardMode.DirectInput), "DX bind");
        Assert(input.IsBound && input.BoundWindow == (nint)hwnd, "DX binding state");
        string shape = input.GetCursorShape();
        Assert(shape.Length == 0 || shape.Split(',').Length == 6, "DX cursor shape format");
        Console.WriteLine("DX: sending input");
        Assert(input.MoveTo(10, 10), $"DX mouse move; target exited: {target.HasExited}");
        Assert(input.Click(MouseButton.Left), $"DX mouse click; target exited: {target.HasExited}");
        Assert(input.KeyPress(0x41) && input.TypeText("中A"), "DX keyboard and text");
        Assert(input.KeyDown(0x87), "DX key down");
        Assert(input.IsKeyDown(0x87) && input.WaitForKey(0x87, 100) == 0x87, "DX key state and wait");
        Assert(ReadTargetLine(target) == "F24 down", "DX target observed key down");
        Assert(input.KeyUp(0x87), "DX key up");
        Assert(ReadTargetLine(target) == "F24 up", "DX target observed key up");
        Console.WriteLine("DX: locking and unlocking input");
        Assert(input.LockInput(InputLock.MouseAndKeyboard), "DX input lock");
        Assert(input.LockInput(InputLock.None), "DX input unlock");
        using var shared = new InputSession();
        Assert(shared.BindWindow((nint)hwnd, MouseMode.DirectInput, KeyboardMode.DirectInput), "second DX session binds");
        Console.WriteLine("DX: unbinding");
        Assert(input.UnbindWindow(), "DX unbind");
        Assert(shared.KeyDown(0x87), "remaining DX session sends key down");
        Assert(ReadTargetLine(target) == "F24 down", "target still sees second session key down");
        Assert(shared.KeyUp(0x87), "remaining DX session sends key up");
        Assert(ReadTargetLine(target) == "F24 up", "target still sees second session key up");
        Assert(shared.UnbindWindow(), "second DX session unbind");
    }
    finally
    {
        if (!target.HasExited) target.Kill(entireProcessTree: true);
        target.WaitForExit();
    }
}

static string? ReadTargetLine(Process target) =>
    target.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();

static string RuntimeInformationText() => System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString();
static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}.");
}

sealed class RecorderWindow : Form
{
    public ConcurrentQueue<(int Msg, long WParam)> Messages { get; } = new();
    public RecorderControl Child { get; } = new();
    private readonly System.Windows.Forms.Timer? keyTimer;
    protected override bool ShowWithoutActivation => true;

    public RecorderWindow(bool target)
    {
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(-30000, -30000);
        Child.SetBounds(0, 0, 100, 100);
        Controls.Add(Child);
        if (target)
        {
            bool wasDown = false;
            keyTimer = new System.Windows.Forms.Timer { Interval = 10 };
            keyTimer.Tick += (_, _) =>
            {
                bool down = (GetAsyncKeyState(0x87) & 0x8000) != 0;
                if (down != wasDown) Console.WriteLine(down ? "F24 down" : "F24 up");
                wasDown = down;
            };
            Shown += (_, _) => { Console.WriteLine(Handle.ToInt64()); keyTimer.Start(); };
            FormClosed += (_, _) => keyTimer.Dispose();
        }
    }

    protected override void WndProc(ref Message m)
    {
        Messages.Enqueue((m.Msg, m.WParam.ToInt64()));
        base.WndProc(ref m);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int key);
}

sealed class RecorderControl : Control
{
    public ConcurrentQueue<(int Msg, long WParam)> Messages { get; } = new();

    protected override void WndProc(ref Message m)
    {
        Messages.Enqueue((m.Msg, m.WParam.ToInt64()));
        base.WndProc(ref m);
    }
}

