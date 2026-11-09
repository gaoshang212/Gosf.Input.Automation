using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

internal sealed class BackgroundTarget
{
    private readonly Window window;
    private readonly Button button = new() { Content = "Click", Width = 120, Height = 40 };
    private readonly TextBox textBox = new() { Width = 220, Height = 30 };
    private readonly Slider slider = new() { Width = 220, Height = 30, Minimum = 0, Maximum = 100, Value = 20 };
    private readonly ScrollViewer scroll = new() { Width = 220, Height = 100 };
    private int clicks, keyDowns, keyUps, mouseDowns, mouseUps, activations, wheelEvents;

    private BackgroundTarget()
    {
        window = new Window { Title = "Gosf background input target", Content = CreateContent(),
            Width = 300, Height = 390, Left = 100, Top = 100, ShowActivated = false };
        ObserveInput();
        window.Loaded += (_, _) => WriteSnapshot();
    }

    private Canvas CreateContent()
    {
        var canvas = new Canvas();
        Place(canvas, button, 20, 20);
        Place(canvas, textBox, 20, 90);
        Place(canvas, slider, 20, 140);
        scroll.Content = new TextBlock { Text = string.Join("\n", Enumerable.Range(1, 50)), FontSize = 16 };
        Place(canvas, scroll, 20, 200);
        return canvas;
    }

    private void ObserveInput()
    {
        button.Click += (_, _) => clicks++;
        window.PreviewMouseDown += (_, _) => mouseDowns++;
        window.PreviewMouseUp += (_, _) => mouseUps++;
        window.PreviewKeyDown += (_, _) => keyDowns++;
        window.PreviewKeyUp += (_, _) => keyUps++;
        window.PreviewMouseWheel += (_, _) => wheelEvents++;
        window.Activated += (_, _) => activations++;
    }

    public static int Run()
    {
        var app = new Application();
        var target = new BackgroundTarget();
        _ = Task.Run(target.ReadCommands);
        return app.Run(target.window);
    }

    private async Task ReadCommands()
    {
        while (await Console.In.ReadLineAsync() is { } command)
        {
            if (command == "quit") break;
            if (command == "snapshot") await window.Dispatcher.InvokeAsync(WriteSnapshot);
        }
        await window.Dispatcher.InvokeAsync(window.Close);
    }

    private void WriteSnapshot()
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        var thumb = ((Track)slider.Template.FindName("PART_Track", slider)).Thumb;
        var snapshot = new TargetSnapshot((long)hwnd, Center(button, hwnd), Center(textBox, hwnd),
            Center(thumb, hwnd), Center(scroll, hwnd), clicks, keyDowns, keyUps, mouseDowns, mouseUps,
            activations, textBox.Text, slider.Value, scroll.VerticalOffset, wheelEvents,
            Mouse.Captured != null, Keyboard.FocusedElement != null);
        Console.WriteLine(JsonSerializer.Serialize(snapshot));
    }

    private static TargetPoint Center(FrameworkElement element, nint hwnd)
    {
        var screen = element.PointToScreen(new Point(element.ActualWidth / 2, element.ActualHeight / 2));
        var point = new NativePoint { X = (int)Math.Round(screen.X), Y = (int)Math.Round(screen.Y) };
        if (!NativeDesktop.ScreenToClient(hwnd, ref point)) throw new InvalidOperationException("ScreenToClient failed.");
        return new TargetPoint(point.X, point.Y);
    }

    private static void Place(Canvas canvas, FrameworkElement element, double x, double y)
    {
        Canvas.SetLeft(element, x);
        Canvas.SetTop(element, y);
        canvas.Children.Add(element);
    }
}

internal sealed record TargetPoint(int X, int Y);
internal sealed record TargetSnapshot(long Hwnd, TargetPoint Button, TargetPoint TextBox, TargetPoint Thumb,
    TargetPoint Scroll, int Clicks, int KeyDowns, int KeyUps, int MouseDowns, int MouseUps, int Activations,
    string Text, double SliderValue, double ScrollOffset, int WheelEvents, bool Captured, bool KeyboardFocused);

[StructLayout(LayoutKind.Sequential)]
internal struct NativePoint { public int X, Y; }

internal static class NativeDesktop
{
    [DllImport("user32.dll")] internal static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] internal static extern bool ScreenToClient(nint hwnd, ref NativePoint point);
}
