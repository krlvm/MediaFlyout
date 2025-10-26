/**
 * MIT License
 * 
 * Copyright (c) 2016 minami_SC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using MediaFlyout.Extensions;
using MediaFlyout.Interop;

namespace MediaFlyout.Helpers
{
    public sealed class AccentColors
    {
        public static Color GetColorByTypeName(string name)
        {
            var colorSet = UxTheme.GetImmersiveUserColorSetPreference(false, false);
            var colorType = UxTheme.GetImmersiveColorTypeFromName(name);
            var rawColor = UxTheme.GetImmersiveColorFromColorSetEx(colorSet, colorType, false, 0);

            var bytes = BitConverter.GetBytes(rawColor);
            return Color.FromArgb(bytes[3], bytes[0], bytes[1], bytes[2]);
        }

        internal static void Initialize()
        {
            ImmersiveSystemAccent = GetColorByTypeName("ImmersiveSystemAccent");
            ImmersiveSystemAccentDark1 = GetColorByTypeName("ImmersiveSystemAccentDark1");
            ImmersiveSystemAccentDark2 = GetColorByTypeName("ImmersiveSystemAccentDark2");
            ImmersiveSystemAccentDark3 = GetColorByTypeName("ImmersiveSystemAccentDark3");
            ImmersiveSystemAccentLight1 = GetColorByTypeName("ImmersiveSystemAccentLight1");
            ImmersiveSystemAccentLight2 = GetColorByTypeName("ImmersiveSystemAccentLight2");
            ImmersiveSystemAccentLight3 = GetColorByTypeName("ImmersiveSystemAccentLight3");

            ImmersiveSystemAccentBrush = CreateBrush(ImmersiveSystemAccent);
            ImmersiveSystemAccentDark1Brush = CreateBrush(ImmersiveSystemAccentDark1);
            ImmersiveSystemAccentDark2Brush = CreateBrush(ImmersiveSystemAccentDark2);
            ImmersiveSystemAccentDark3Brush = CreateBrush(ImmersiveSystemAccentDark3);
            ImmersiveSystemAccentLight1Brush = CreateBrush(ImmersiveSystemAccentLight1);
            ImmersiveSystemAccentLight2Brush = CreateBrush(ImmersiveSystemAccentLight2);
            ImmersiveSystemAccentLight3Brush = CreateBrush(ImmersiveSystemAccentLight3);
        }

        public static Brush CreateBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        internal static void WatchAccentColors()
        {
            var window = Application.Current.MainWindow;
            if (window != null)
            {
                WatchAccentColors(window);
                return;
            }

            EventHandler handler = null;
            handler = (e, args) =>
            {
                if (Application.Current.MainWindow != null)
                {
                    WatchAccentColors(Application.Current.MainWindow);
                }
                Application.Current.Activated -= handler;
            };
            Application.Current.Activated += handler;
        }

        static void WatchAccentColors(Window window)
        {
            if (window.IsLoaded)
            {
                var source = HwndSource.FromHwnd(window.GetHandle());
                source.AddHook(WndProc);
                return;
            }

            window.Loaded += (_, __) =>
            {
                WatchAccentColors(window);
            };
        }

        static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case User32.WM_DWMCOLORIZATIONCOLORCHANGED:
                    Initialize();
                    ThemeHelper.HandleThemeChange(null, null);
                    break;
                case User32.WM_SETTINGCHANGE:
                    var systemParmeter = Marshal.PtrToStringAuto(lParam);
                    if (systemParmeter == "ImmersiveColorSet")
                    {
                        ThemeHelper.HandleThemeChange(null, null);
                    }
                    break;
                case User32.WM_DPICHANGED:
                    // TODO: it should not be handled as part of theme watching
                    ThemeHelper.HandleThemeChange(null, null);
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        #region Resources

        #region Colors
        private static Color immersiveSystemAccent;
        public static Color ImmersiveSystemAccent
        {
            get { return immersiveSystemAccent; }
            private set { if (!object.Equals(immersiveSystemAccent, value)) { immersiveSystemAccent = value; OnStaticPropertyChanged(); } }
        }

        private static Color immersiveSystemAccentDark1;
        public static Color ImmersiveSystemAccentDark1
        {
            get { return immersiveSystemAccentDark1; }
            private set { if (!object.Equals(immersiveSystemAccentDark1, value)) { immersiveSystemAccentDark1 = value; OnStaticPropertyChanged(); } }
        }

        private static Color immersiveSystemAccentDark2;
        public static Color ImmersiveSystemAccentDark2
        {
            get { return immersiveSystemAccentDark2; }
            private set { if (!object.Equals(immersiveSystemAccentDark2, value)) { immersiveSystemAccentDark2 = value; OnStaticPropertyChanged(); } }
        }

        private static Color immersiveSystemAccentDark3;
        public static Color ImmersiveSystemAccentDark3
        {
            get { return immersiveSystemAccentDark3; }
            private set { if (!object.Equals(immersiveSystemAccentDark3, value)) { immersiveSystemAccentDark3 = value; OnStaticPropertyChanged(); } }
        }

        private static Color immersiveSystemAccentLight1;
        public static Color ImmersiveSystemAccentLight1
        {
            get { return immersiveSystemAccentLight1; }
            private set { if (!object.Equals(immersiveSystemAccentLight1, value)) { immersiveSystemAccentLight1 = value; OnStaticPropertyChanged(); } }
        }

        private static Color immersiveSystemAccentLight2;
        public static Color ImmersiveSystemAccentLight2
        {
            get { return immersiveSystemAccentLight2; }
            private set { if (!object.Equals(immersiveSystemAccentLight2, value)) { immersiveSystemAccentLight2 = value; OnStaticPropertyChanged(); } }
        }

        private static Color immersiveSystemAccentLight3;
        public static Color ImmersiveSystemAccentLight3
        {
            get { return immersiveSystemAccentLight3; }
            private set { if (!object.Equals(immersiveSystemAccentLight3, value)) { immersiveSystemAccentLight3 = value; OnStaticPropertyChanged(); } }
        }
        #endregion

        #region Brushes
        private static Brush immersiveSystemAccentBrush;
        public static Brush ImmersiveSystemAccentBrush
        {
            get { return immersiveSystemAccentBrush; }
            private set { if (!object.Equals(immersiveSystemAccentBrush, value)) { immersiveSystemAccentBrush = value; OnStaticPropertyChanged(); } }
        }

        private static Brush immersiveSystemAccentDark1Brush;
        public static Brush ImmersiveSystemAccentDark1Brush
        {
            get { return immersiveSystemAccentDark1Brush; }
            private set { if (!object.Equals(immersiveSystemAccentDark1Brush, value)) { immersiveSystemAccentDark1Brush = value; OnStaticPropertyChanged(); } }
        }
        private static Brush immersiveSystemAccentDark2Brush;
        public static Brush ImmersiveSystemAccentDark2Brush
        {
            get { return immersiveSystemAccentDark2Brush; }
            private set { if (!object.Equals(immersiveSystemAccentDark2Brush, value)) { immersiveSystemAccentDark2Brush = value; OnStaticPropertyChanged(); } }
        }
        private static Brush immersiveSystemAccentDark3Brush;
        public static Brush ImmersiveSystemAccentDark3Brush
        {
            get { return immersiveSystemAccentDark3Brush; }
            private set { if (!object.Equals(immersiveSystemAccentDark3Brush, value)) { immersiveSystemAccentDark3Brush = value; OnStaticPropertyChanged(); } }
        }

        private static Brush immersiveSystemAccentLight1Brush;
        public static Brush ImmersiveSystemAccentLight1Brush
        {
            get { return immersiveSystemAccentLight1Brush; }
            private set { if (!object.Equals(immersiveSystemAccentLight1Brush, value)) { immersiveSystemAccentLight1Brush = value; OnStaticPropertyChanged(); } }
        }
        private static Brush immersiveSystemAccentLight2Brush;
        public static Brush ImmersiveSystemAccentLight2Brush
        {
            get { return immersiveSystemAccentLight2Brush; }
            private set { if (!object.Equals(immersiveSystemAccentLight2Brush, value)) { immersiveSystemAccentLight2Brush = value; OnStaticPropertyChanged(); } }
        }
        private static Brush immersiveSystemAccentLight3Brush;
        public static Brush ImmersiveSystemAccentLight3Brush
        {
            get { return immersiveSystemAccentLight3Brush; }
            private set { if (!object.Equals(immersiveSystemAccentLight3Brush, value)) { immersiveSystemAccentLight3Brush = value; OnStaticPropertyChanged(); } }
        }
        #endregion

        #region Property Change Events
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        internal static void OnStaticPropertyChanged([CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #endregion
    }
}
