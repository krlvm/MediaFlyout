using System;
using System.Runtime.InteropServices;

namespace MediaFlyout.Interop
{
    class DwmApi
    {
        public enum DWMWINDOWATTRIBUTE
        {
            DWMA_CLOAK = 13,
            DWMWA_WINDOW_CORNER_PREFERENCE = 33
        }

        public enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        internal static extern void DwmSetWindowAttribute(
            IntPtr hwnd,
            DWMWINDOWATTRIBUTE attr,
            ref int attrValue,
            int attrSize);
    }
}
