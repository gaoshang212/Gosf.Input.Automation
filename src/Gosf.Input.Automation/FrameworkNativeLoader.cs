#if NETFRAMEWORK
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Gosf.Input.Automation;

internal static class FrameworkNativeLoader
{
    // Keep the loaded module for the process lifetime, just like a P/Invoke module.
    private static nint module;

    internal static void Load()
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            throw new PlatformNotSupportedException("Windows is required.");
        string architecture = Environment.Is64BitProcess ? "x64" : "x86";
        string directory = Path.GetDirectoryName(typeof(FrameworkNativeLoader).Assembly.Location)!;
        string path = Path.Combine(directory, "native", architecture, "Gosf.Input.Automation.Native.dll");
        module = LoadLibraryEx(path, 0, 0x00001100); // DLL_LOAD_DIR | DEFAULT_DIRS
        if (module == 0)
            throw new DllNotFoundException($"Could not load the native backend at '{path}'.",
                new Win32Exception(Marshal.GetLastWin32Error()));
    }

    internal static bool IsSupportedOSArchitecture()
    {
        GetNativeSystemInfo(out var information);
        return information.ProcessorArchitecture is 0 or 9; // Intel or AMD64
    }

    [StructLayout(LayoutKind.Explicit, Size = 48)]
    private struct SystemInformation
    {
        [FieldOffset(0)] public ushort ProcessorArchitecture;
    }

    [DllImport("kernel32.dll", EntryPoint = "LoadLibraryExW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint LoadLibraryEx(string path, nint file, uint flags);

    [DllImport("kernel32.dll")]
    private static extern void GetNativeSystemInfo(out SystemInformation information);
}
#endif
