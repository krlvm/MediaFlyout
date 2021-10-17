using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace MediaFlyout.Interop
{
    class NativeMethods
    {

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const UInt32 SWP_SHOWWINDOW = 0x0040;

        public enum ShowWindowCommands
        {
            Hide = 0,
            Normal = 1,
            ShowMinimized = 2,
            Maximize = 3,
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(Rect rect)
            {
                Left = (int)rect.Left;
                Right = (int)rect.Right;
                Top = (int)rect.Top;
                Bottom = (int)rect.Bottom;
            }

            public Size Size
            {
                get => new Size(Right - Left, Bottom - Top);
            }

            public int Height => Bottom - Top;

            public int Width => Right - Left;

            public Rect ToRect() => new Rect(Left, Top, Width, Height);
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int Length;

            public int Flags;

            public ShowWindowCommands ShowCmd;

            public POINT MinPosition;

            public POINT MaxPosition;

            public RECT NormalPosition;

            public static WINDOWPLACEMENT Default
            {
                get
                {
                    WINDOWPLACEMENT result = default;
                    result.Length = Marshal.SizeOf(result);
                    return result;
                }
            }
        }

        [DllImport("user32.dll")]
        public static extern bool IsImmersiveProcess(IntPtr hProcess);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern int GetApplicationUserModelId(
            IntPtr hProcess,
            ref int applicationUserModelIdLength,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder applicationUserModelId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        internal static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ShowWindowAsync(IntPtr windowHandle, ShowWindowCommands nCmdShow);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        public const int GWL_EX_STYLE = -20;
        public const int WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, GWL nIndex);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, GWL nIndex, IntPtr dwNewLong);

        public const long WS_EX_TOPMOST = 0x00000008L;
        public enum GWL : int
        {
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
        }

        public const int DWMA_CLOAK = 13;

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        public static extern IntPtr FindWindow([MarshalAs(UnmanagedType.LPWStr)] string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);
        public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);
    }
}
