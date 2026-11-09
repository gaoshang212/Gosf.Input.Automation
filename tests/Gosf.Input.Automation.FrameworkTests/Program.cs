using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gosf.Input.Automation;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (Array.IndexOf(args, "--target") >= 0)
        {
            Application.Run(new InputTarget());
            return 0;
        }
        try
        {
            TestValidation();
            TestWindowInput(MouseMode.WindowMessage, KeyboardMode.WindowMessage);
            TestWindowInput(MouseMode.DirectInput, KeyboardMode.DirectInput);
            Console.WriteLine($"PASS: net462 {(Environment.Is64BitProcess ? "x64" : "x86")} input regression.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static void TestValidation()
    {
        using (var input = new InputSession())
        {
            Check(InputSession.BackendVersion == "0.4.8.3", "Backend version mismatch.");
            Check(!input.IsBound, "New session must be unbound.");
            Throws<ArgumentNullException>(() => input.TypeText(null!));
            Throws<ArgumentException>(() => input.TypeText("a\0b"));
            Throws<ArgumentNullException>(() => input.MovePath(null!));
            Throws<ArgumentException>(() => input.MovePath(new[] { new Point(1, 1) }));
            Throws<ArgumentOutOfRangeException>(() => input.LockInput((InputLock)99));
            Throws<ArgumentOutOfRangeException>(() => input.SetMouseTrajectory((TrajectoryMode)99, 0, 0, 0, 0, 0));
            Throws<ArgumentOutOfRangeException>(() => input.KeyPress(256));
            input.Dispose();
            Throws<ObjectDisposedException>(() => input.TypeText(""));
        }
    }

    private static void TestWindowInput(MouseMode mouse, KeyboardMode keyboard)
    {
        using (var target = StartTarget())
        {
            try
            {
                var window = (IntPtr)long.Parse(ReadTargetLine(target), CultureInfo.InvariantCulture);
                using (var input = new InputSession()) ExerciseInput(input, target, window, mouse, keyboard);
            }
            finally
            {
                if (!target.HasExited) target.Kill();
                target.WaitForExit();
            }
        }
    }

    private static void ExerciseInput(InputSession input, Process target, IntPtr window,
        MouseMode mouse, KeyboardMode keyboard)
    {
        Check(input.BindWindow(window, mouse, keyboard), "Window binding failed.");
        Check(input.MoveTo(20, 30) && input.Click(MouseButton.Left), "Mouse input failed.");
        Check(ReadTargetLine(target) == "click", "Target did not receive click.");
        Check(input.TypeText("中"), "Unicode input failed.");
        Check(ReadTargetLine(target) == "text", "Target did not receive Unicode text.");
        Check(input.MovePath(new[] { new Point(1, 1), new Point(5, 5) }, 1), "Path input failed.");
        Check(input.UnbindWindow() && !input.IsBound, "Unbind failed.");
    }

    private static Process StartTarget()
    {
        var info = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location, "--target")
        {
            UseShellExecute = false, RedirectStandardOutput = true, CreateNoWindow = true
        };
        return Process.Start(info) ?? throw new InvalidOperationException("Could not start test target.");
    }

    private static string ReadTargetLine(Process target)
    {
        Task<string?> read = target.StandardOutput.ReadLineAsync();
        if (!read.Wait(TimeSpan.FromSeconds(15))) throw new TimeoutException("Target response timed out.");
        return read.Result ?? throw new InvalidOperationException("Target closed its output.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}.");
    }
}

internal sealed class InputTarget : Form
{
    protected override bool ShowWithoutActivation => true;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Console.WriteLine(Handle.ToInt64().ToString(CultureInfo.InvariantCulture));
    }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == 0x0202) Console.WriteLine("click");
        if (message.Msg == 0x0102 && message.WParam.ToInt64() == '中') Console.WriteLine("text");
        base.WndProc(ref message);
    }
}
