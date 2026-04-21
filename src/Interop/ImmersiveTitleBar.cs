using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Ordir.Interop;

/// <summary>Uses DWM immersive dark mode for the system caption (Win10 1809+ / Win11).</summary>
internal static class ImmersiveTitleBar
{
    private const int DwmwaUseImmersiveDarkMode = 20;

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(nint hwnd, int attr, ref int attrValue, int attrSize);

    public static void TryApplyDark(Window window)
    {
        var h = new WindowInteropHelper(window).Handle;
        if (h == 0) return;
        var on = 1;
        _ = DwmSetWindowAttribute(h, DwmwaUseImmersiveDarkMode, ref on, sizeof(int));
    }
}
