using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace MultiBranchTexter.View;


//following (partial) enums are used to enable the aeroglass effect using Windows10 
//see https://github.com/riverar/sample-win10-aeroglass (2016/08)
internal enum AccentState
{
    AccentDisabled = 0,
    AccentEnableGradient = 1,
    AccentEnableTransparentgradient = 2,
    AccentEnableBlurbehind = 3,
    AccentInvalidState = 4
}

[StructLayout(LayoutKind.Sequential)]
internal struct AccentPolicy
{
    public AccentState AccentState;
    public int AccentFlags;
    public int GradientColor;
    public int AnimationId;
}

[StructLayout(LayoutKind.Sequential)]
internal struct WindowCompositionAttributeData
{
    public WindowCompositionAttribute Attribute;
    public IntPtr Data;
    public int SizeOfData;
}

internal enum WindowCompositionAttribute
{
    // ...
    WcaAccentPolicy = 19
    // ...
}

[StructLayout(LayoutKind.Sequential)]
public struct MARGINS
{
    public MARGINS(Thickness t)
    {
        Left = (int)t.Left;
        Right = (int)t.Right;
        Top = (int)t.Top;
        Bottom = (int)t.Bottom;
    }
    public int Left;
    public int Right;
    public int Top;
    public int Bottom;
}

public class GlassHelper
{
    [DllImport("dwmapi.dll", PreserveSig = false)]
    static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    [DllImport("dwmapi.dll", PreserveSig = false)]
    static extern bool DwmIsCompositionEnabled();

    [DllImport("user32.dll")]
    static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    /// <summary>
    /// this method uses the SetWindowCompositionAttribute to apply an AeroGlass effect to the window
    /// </summary>
    public static void EnableBlur(Window win, bool enable)
    {
        //this code is taken from a sample application provided by Rafael Rivera
        //see the full code sample here: (2016/08)
        // https://github.com/riverar/sample-win10-aeroglass
        var windowHelper = new WindowInteropHelper(win);

        var accent = new AccentPolicy
        {
            AccentState = enable ? AccentState.AccentEnableBlurbehind : AccentState.AccentDisabled
        };

        var accentStructSize = Marshal.SizeOf(accent);

        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
        Marshal.StructureToPtr(accent, accentPtr, false);

        var data = new WindowCompositionAttributeData
        {
            Attribute = WindowCompositionAttribute.WcaAccentPolicy,
            SizeOfData = accentStructSize,
            Data = accentPtr
        };

        SetWindowCompositionAttribute(windowHelper.Handle, ref data);

        Marshal.FreeHGlobal(accentPtr);
    }

    public static bool ExtendGlassFrame(Window window, Thickness margin)
    {
        if (!DwmIsCompositionEnabled())
        { return false; }

        IntPtr hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            throw new InvalidOperationException("The Window must be shown before extending glass.");
        }

        // Set the background to transparent from both the WPF and Win32 perspectives
        window.Background = Brushes.Transparent;
        HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = Colors.Transparent;

        MARGINS margins = new MARGINS(margin);
        DwmExtendFrameIntoClientArea(hwnd, ref margins);
        return true;
    }
}
