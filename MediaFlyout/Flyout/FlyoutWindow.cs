using MediaFlyout.Extensions;
using MediaFlyout.Helpers;
using MediaFlyout.Interop;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MediaFlyout.Flyout
{
    public abstract class FlyoutWindow : Window
    {
        protected FlyoutTray _tray;
        protected bool isRaising = false;

        public FlyoutTray Tray
        {
            get { return _tray; }
        }
        public bool IsRaising
        {
            get { return isRaising; }
            set { isRaising = value; }
        }

        public FlyoutWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            SizeToContent = SizeToContent.Height;
            ResizeMode = ResizeMode.NoResize;
            UseLayoutRounding = false;
            Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            BorderThickness = new Thickness(0);
            ShowInTaskbar = false;
            Visibility = Visibility.Hidden;

            Loaded += Window_Loaded;
            Deactivated += Window_Deactivated;
            Closing += Window_Closing;
            PreviewKeyDown += Window_PreviewKeyDown;

            if (Environment.OSVersion.IsAtLeast(OSVersions.VER_11_21H2))
            {
                this.EnableRoundedCornersIfApplicable();
            }

            themeHelper = new ThemeHelper();
            themeHelper.OnThemeChanged += Theme_ThemeChanged;
            Theme_Update();

            Left = 999999;
            Show();
            Hide();
        }

        public virtual void RaiseFlyout()
        {
            AnimationHelper.ShowFlyout(this, true);
        }

        public virtual void DismissFlyout()
        {
            if (Visibility == Visibility.Hidden) return;
            AnimationHelper.HideFlyout(this);
        }


        #region Window Management

        private void Window_Loaded(object sender, EventArgs e)
        {
            this.Cloak();
            this.ApplyExtendedWindowStyle(User32.WS_EX_TOOLWINDOW);
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (IsRaising)
            {
                IsRaising = false;
                return;
            }
            DismissFlyout();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            DismissFlyout();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DismissFlyout();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.Space)
            {
                e.Handled = true;
            }
        }

        #endregion

        #region Theme Management

        private ThemeHelper themeHelper;
        private ColorScheme theme_last;

        private bool _needReset = false;
        private Color tintColor;
        private double _tintOpacity;
        private double tintOpacity
        {
            get { return _tintOpacity; }
            set
            {
                if (_tintOpacity != value && (_tintOpacity == 1 || value == 1))
                {
                    _needReset = true;
                }
                _tintOpacity = value;
            }
        }

        public void Theme_Apply()
        {
            if (_needReset)
            {
                this.ResetAccentPolicy();
                _needReset = false;
            }

            User32.AccentFlags flags = User32.AccentFlags.DrawAllBorders;
            if (Environment.OSVersion.IsLessThan(OSVersions.VER_11_21H2))
            {
                switch (WindowsTaskbar.Current.Side)
                {
                    case WindowsTaskbar.Position.Left:
                        flags = User32.AccentFlags.DrawRightBorder | User32.AccentFlags.DrawTopBorder;
                        break;
                    case WindowsTaskbar.Position.Right:
                        flags = User32.AccentFlags.DrawLeftBorder | User32.AccentFlags.DrawTopBorder;
                        break;
                    case WindowsTaskbar.Position.Top:
                        flags = User32.AccentFlags.DrawLeftBorder | User32.AccentFlags.DrawBottomBorder;
                        break;
                    case WindowsTaskbar.Position.Bottom:
                        flags = User32.AccentFlags.DrawTopBorder | User32.AccentFlags.DrawLeftBorder;
                        break;
                }
            }

            this.ApplyAccentPolicy(tintOpacity, tintColor, flags);
        }

        private void Theme_Update()
        {
            var theme = ThemeHelper.GetTheme();

            double tintOpacity;
            Color tintColor, fallbackColor;

            Color colorScheme = theme.SystemTheme.ToColor();
            Color contrastColor = theme.SystemTheme.Inverse().ToColor();

            if (theme.SystemTheme == ColorScheme.Light)
            {
                SourceChord.FluentWPF.ResourceDictionaryEx.GlobalTheme = SourceChord.FluentWPF.ElementTheme.Light;
                fallbackColor = Color.FromRgb(228, 228, 228);
                tintColor = Color.FromRgb(228, 228, 228);
                tintOpacity = 0.853;
                _tray?.SetIconColor(System.Drawing.Color.Black);
            }
            else
            {
                SourceChord.FluentWPF.ResourceDictionaryEx.GlobalTheme = SourceChord.FluentWPF.ElementTheme.Dark;
                if (theme.ShowAccentColorOnSurface)
                {
                    fallbackColor = theme.AccentColor;
                    tintColor = theme.AccentColor;
                    tintOpacity = 0.8;
                }
                else
                {
                    fallbackColor = Color.FromRgb(31, 31, 31);
                    tintColor = Color.FromRgb(36, 36, 36);
                    tintOpacity = 0.85;
                }
                _tray?.SetIconColor(System.Drawing.Color.White);
            }

            this.tintOpacity = theme.AcrylicEnabled ? tintOpacity : 1;
            this.tintColor = theme.AcrylicEnabled ? tintColor : fallbackColor;
            Resources["FlyoutColorScheme"] = colorScheme;
            Resources["FlyoutContrastColor"] = new SolidColorBrush(contrastColor);
            Resources["FluentRevealEnabled"] = theme.AcrylicEnabled;

            if (theme_last != theme.SystemTheme)
            {
                Theme_ThemeUpdated();
            }
            theme_last = theme.SystemTheme;

            if (Visibility == Visibility.Visible)
            {
                Theme_Apply();
            }
        }

        private void Theme_ThemeChanged(object sender, EventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate ()
            {
                Theme_Update();
            });
        }

        protected virtual void Theme_ThemeUpdated() { }

        #endregion
    }
}
