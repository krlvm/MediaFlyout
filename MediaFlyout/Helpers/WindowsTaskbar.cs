using System.Windows;

namespace MediaFlyout.Helpers
{
    public sealed class WindowsTaskbar
    {
        public struct State
        {
            public Position Side;
            public double Right;
            public double Bottom;

            public bool IsHorizontal
            {
                get
                {
                    return Side == Position.Bottom || Side == Position.Top;
                }
            }
            public bool IsAutoHideEnabled
            {
                get
                {
                    return IsHorizontal ? (SystemParameters.PrimaryScreenHeight == SystemParameters.WorkArea.Height) :
                        (SystemParameters.PrimaryScreenWidth == SystemParameters.WorkArea.Width);
                }
            }
        }

        public enum Position
        {
            Left,
            Top,
            Right,
            Bottom
        }

        public static State Current
        {
            get
            {
                Position side;
                double right, bottom;

                var wk = SystemParameters.WorkArea;
                if (wk.Width == SystemParameters.PrimaryScreenWidth)
                {
                    right = wk.Right;
                    if (wk.Top > 0)
                    {
                        side = Position.Top;
                        bottom = wk.Top;
                    }
                    else
                    {
                        side = Position.Bottom;
                        bottom = wk.Height;
                    }
                }
                else
                {
                    bottom = wk.Bottom;
                    if (wk.Left > 0)
                    {
                        side = Position.Left;
                        right = wk.Left;
                    }
                    else
                    {
                        side = Position.Right;
                        right = wk.Width;
                    }
                }

                return new State
                {
                    Side = side,
                    Right = right,
                    Bottom = bottom
                };
            }
        }

        //private static IntPtr GetHwnd() => User32.FindWindow("Shell_TrayWnd", null);
    }
}
