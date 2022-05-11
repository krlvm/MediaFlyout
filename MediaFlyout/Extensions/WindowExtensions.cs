using MediaFlyout.Interop;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using static MediaFlyout.Interop.User32;

namespace MediaFlyout.Extensions
{
    static class WindowExtensions
    {
        public static void SetWindowPosNative(this Window window, double top, double left, double height, double width)
        {
            SetWindowPos(window.GetHandle(), IntPtr.Zero, (int)left, (int)top, (int)width, (int)height, WindowPosFlags.SWP_NOZORDER | WindowPosFlags.SWP_NOACTIVATE);
        }

        public static void RaiseWindow(this Window window)
        {
            window.Topmost = true;
            window.Activate();
            window.Topmost = false;
        }

        public static void Cloak(this Window window, bool hide = true)
        {
            int attributeValue = hide ? 1 : 0;
            DwmApi.DwmSetWindowAttribute(window.GetHandle(), DwmApi.DWMWINDOWATTRIBUTE.DWMA_CLOAK, ref attributeValue, Marshal.SizeOf(attributeValue));
        }

        public static void EnableRoundedCornersIfApplicable(this Window window)
        {
            if (Environment.OSVersion.IsAtLeast(OSVersions.VER_11_21H2))
            {
                int attributeValue = (int)DwmApi.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
                DwmApi.DwmSetWindowAttribute(window.GetHandle(), DwmApi.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref attributeValue, Marshal.SizeOf(attributeValue));
            }
        }

        public static void ApplyAccentPolicy(this Window window, AccentState state, double opacity, Color tint, AccentFlags flags)
        {
            var accent = new AccentPolicy();
            accent.AccentState = state;
            if (state != AccentState.ACCENT_DISABLED)
            {
                accent.GradientColor = ((uint)(opacity * 255) << 24) | (tint.ToABGR() & 0xFFFFFF);
                accent.AccentFlags = flags;
            }

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(window.GetHandle(), ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        public static void ApplyAccentPolicy(this Window window, double opacity, Color tint, AccentFlags flags)
        {
            ApplyAccentPolicy(window, 
                opacity == 1 ? AccentState.ACCENT_ENABLE_GRADIENT : AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, 
                opacity, tint, flags);
        }

        public static void ResetAccentPolicy(this Window window)
        {
            ApplyAccentPolicy(window, AccentState.ACCENT_DISABLED, 0, Colors.White, AccentFlags.DrawAllBorders);
        }

        public static void RemoveWindowStyle(this Window window, int styleToRemove)
        {
            var currentStyle = GetWindowLong(window.GetHandle(), GWL.GWL_STYLE);
            if (currentStyle == 0)
            {
                return;
            }
            SetWindowLong(window.GetHandle(), GWL.GWL_STYLE, currentStyle & ~styleToRemove);
        }

        public static void ApplyExtendedWindowStyle(this Window window, int newExStyle)
        {
            var currentExStyle = GetWindowLong(window.GetHandle(), GWL.GWL_EXSTYLE);
            if (currentExStyle == 0)
            {
                return;
            }
            SetWindowLong(window.GetHandle(), GWL.GWL_EXSTYLE, currentExStyle | newExStyle);
        }

        public static IntPtr GetHandle(this Window window)
        {
            return new WindowInteropHelper(window).Handle;
        }
    }
}
