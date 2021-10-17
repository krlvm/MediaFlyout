using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using static MediaFlyout.Interop.NativeMethods;

namespace MediaFlyout.Helpers
{
    class InteropHelper
    {
        public static void HideWindowFromAltTab(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            SetWindowLong(handle, GWL_EX_STYLE, GetWindowLong(handle, GWL_EX_STYLE) | WS_EX_TOOLWINDOW);
        }

        public static void MakeWindowVisibleVD(Window window)
        {
            var wih = new WindowInteropHelper(window);
            var style = GetWindowLongPtr(wih.Handle, GWL.GWL_EXSTYLE);
            style = new IntPtr(style.ToInt64() | WS_EX_TOPMOST);
            SetWindowLongPtr(wih.Handle, GWL.GWL_EXSTYLE, style);
        }

        public static void CloakWindow(Window window, bool hide = true)
        {
            int val = hide ? 1 : 0;
            DwmSetWindowAttribute(new WindowInteropHelper(window).Handle, DWMA_CLOAK, ref val, Marshal.SizeOf(val));
        }

        // https://stackoverflow.com/a/2584672
        public static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
            {
                EnumThreadWindows(thread.Id, (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);
            }

            return handles;
        }
    }
}
