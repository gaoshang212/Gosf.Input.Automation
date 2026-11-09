using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Gosf.Input.Automation;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Contains("--target")) return BackgroundTarget.Run();
        try { return RunAsync().GetAwaiter().GetResult(); }
        catch (Exception error) { Console.Error.WriteLine(error.Message); return 1; }
    }

    private static async Task<int> RunAsync()
    {
        using var target = StartTarget();
        try
        {
            var ready = await ReadSnapshot(target);
            using var input = new InputSession();
            Bind(input, ready);
            await ExerciseKeyboard(input, target, ready);
            await ExerciseMouse(input, target, ready);
            await ExerciseCleanup(input, target, ready);
            Console.WriteLine("PASS: background WPF click, keys, modifiers, drag, wheel and rebind.");
            return 0;
        }
        finally { await StopTarget(target); }
    }

    private static Process StartTarget()
    {
        var info = new ProcessStartInfo(Environment.ProcessPath!)
        {
            RedirectStandardInput = true, RedirectStandardOutput = true,
            UseShellExecute = false, CreateNoWindow = true
        };
        if (Path.GetFileNameWithoutExtension(Environment.ProcessPath) == "dotnet")
            info.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
        info.ArgumentList.Add("--target");
        return Process.Start(info) ?? throw new InvalidOperationException("Target launch failed.");
    }

    private static void Bind(InputSession input, TargetSnapshot ready)
    {
        Check(input.BindWindow((nint)ready.Hwnd, MouseMode.DirectInput, KeyboardMode.DirectInput), "bind failed");
        Check(NativeDesktop.GetForegroundWindow() != (nint)ready.Hwnd, "Target started in foreground.");
    }

    private static async Task ExerciseKeyboard(InputSession input, Process target, TargetSnapshot ready)
    {
        Click(input, ready.Button);
        var clicked = await Snapshot(target);
        Check(clicked.Clicks == 1, "background Button.Click was not delivered");
        Click(input, ready.TextBox);
        Check(input.KeyPress(0x41) && input.TypeText("test"), "keyboard API failed");
        var typed = await Snapshot(target);
        Check(typed.KeyDowns > 0 && typed.KeyUps > 0 && typed.Text == "atest", "key/text input failed");
        Check(input.KeyDown("shift") && input.KeyPress(0x42) && input.KeyUp("shift"), "Shift API failed");
        Check((await Snapshot(target)).Text == "atestB", "Shift+B did not produce uppercase B");
        Check(input.KeyPress(0x08), "Backspace API failed");
        Check((await Snapshot(target)).Text == "atest", "Backspace failed");
        Check(input.KeyDown("ctrl") && input.KeyPress(0x41) && input.KeyUp("ctrl"), "Ctrl+A API failed");
        Check(input.TypeText("后台🙂"), "Unicode API failed");
        Check((await Snapshot(target)).Text == "后台🙂", "Ctrl+A selection / Unicode replacement failed");
    }

    private static async Task ExerciseMouse(InputSession input, Process target, TargetSnapshot ready)
    {
        Check(input.MoveTo(ready.Thumb.X, ready.Thumb.Y) && input.MouseDown(MouseButton.Left), "drag start failed");
        Check((await Snapshot(target)).Captured, "slider did not capture mouse");
        Check(input.MoveBy(70, 0) && input.MouseUp(MouseButton.Left), "drag end failed");
        var dragged = await Snapshot(target);
        Check(dragged.SliderValue > ready.SliderValue + 5 && !dragged.Captured, "slider drag/capture release failed");
        Check(input.MoveTo(ready.Scroll.X, ready.Scroll.Y) && input.Scroll(-120), "wheel API failed");
        var scrolled = await Snapshot(target);
        Check(scrolled.WheelEvents > 0 && scrolled.ScrollOffset > 0, "ScrollViewer did not scroll");
    }

    private static async Task ExerciseCleanup(InputSession input, Process target, TargetSnapshot ready)
    {
        Check(input.MoveTo(ready.Button.X, ready.Button.Y) && input.MouseDown(MouseButton.Left), "held mouse failed");
        Check(input.KeyDown("shift"), "held key failed");
        Check((await Snapshot(target)).Captured, "button capture missing before unbind");
        Check(input.UnbindWindow(), "unbind failed");
        var released = await Snapshot(target);
        Check(!released.Captured && !released.KeyboardFocused && released.Clicks == 1, "unbind did not cancel input cleanly");
        await Task.Delay(1000);
        await Snapshot(target);
        Bind(input, ready);
        await Snapshot(target);
        Click(input, ready.TextBox);
        Check(input.KeyPress(0x43), "rebound key failed");
        var rebound = await Snapshot(target);
        Check(rebound.Text.Contains('c') && !rebound.Text.Contains('C'), "rebind retained Shift or lost keyboard input");
        Check(input.UnbindWindow(), "final unbind failed");
        var final = await Snapshot(target);
        Check(!final.Captured && !final.KeyboardFocused, "final cleanup failed");
    }

    private static void Click(InputSession input, TargetPoint point)
    {
        Check(input.MoveTo(point.X, point.Y) && input.Click(MouseButton.Left), "click API failed");
    }

    private static async Task<TargetSnapshot> Snapshot(Process target)
    {
        await Task.Delay(100);
        await target.StandardInput.WriteLineAsync("snapshot");
        await target.StandardInput.FlushAsync();
        var snapshot = await ReadSnapshot(target);
        Console.WriteLine(JsonSerializer.Serialize(snapshot));
        Check(snapshot.Activations == 0 && NativeDesktop.GetForegroundWindow() != (nint)snapshot.Hwnd,
            "target activated during background input");
        return snapshot;
    }

    private static async Task<TargetSnapshot> ReadSnapshot(Process target)
    {
        var line = await target.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10));
        return JsonSerializer.Deserialize<TargetSnapshot>(line ?? throw new InvalidOperationException("Target exited."))!;
    }

    private static async Task StopTarget(Process target)
    {
        if (target.HasExited) return;
        await target.StandardInput.WriteLineAsync("quit");
        await target.StandardInput.FlushAsync();
        try { await target.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5)); }
        catch (TimeoutException) { target.Kill(); await target.WaitForExitAsync(); }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}



