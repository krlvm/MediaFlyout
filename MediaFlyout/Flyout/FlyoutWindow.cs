using MediaFlyout.Extensions;
using MediaFlyout.Helpers;
using MediaFlyout.Interop;
using MediaFlyout.Styles;
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
            UseLayoutRounding = true;
            SnapsToDevicePixels = true;
            Background = new SolidColorBrush(Color.FromArgb(1, 128, 128, 128));
            BorderThickness = new Thickness(0);
            ShowInTaskbar = false;
            Visibility = Visibility.Hidden;

            Loaded += Window_Loaded;
            Deactivated += Window_Deactivated;
            Closing += Window_Closing;
            PreviewKeyDown += Window_PreviewKeyDown;

            if (Environment.OSVersion.IsAtLeast(OSVersions.VER_11_21H2))
            {
                FontFamily = new FontFamily("Segoe UI Variable Text");
                Resources["FluentButtonRadius"] = 4;
                Resources["FluentCornerRadius"] = new CornerRadius(4);
            }

            ThemeHelper.Instance.OnThemeChanged += Theme_ThemeChanged;
            Theme_Update();

            Left = 999999;
            Show();
            Hide();
        }

        public virtual void RaiseFlyout()
        {
            Theme_Update();
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
            this.EnableRoundedCornersIfApplicable();
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

        private static bool IS_W11 = Environment.OSVersion.IsAtLeast(OSVersions.VER_11_21H2);

        private readonly Color COLOR_LIGHT_TINT = IS_W11 ? Color.FromRgb(243, 243, 243) : Color.FromRgb(228, 228, 228);
        private readonly Color COLOR_LIGHT_FALLBACK = IS_W11 ? Color.FromRgb(238, 238, 238) : Color.FromRgb(228, 228, 228);
        private readonly Color COLOR_DARK_TINT = IS_W11 ? Color.FromRgb(32, 32, 32) : Color.FromRgb(36, 36, 36);
        private readonly Color COLOR_DARK_FALLBACK = IS_W11 ? Color.FromRgb(28, 28, 28) : Color.FromRgb(31, 31, 31);

        private bool _needReset = false;

        private ColorScheme? _theme = null;
        private bool? _showAccentColorOnSurface = null;
        private bool? _useAcrylic = null;
        private Color _accentTintColor;
        private Color _tintColor;
        private double __tintOpacity;
        private double _tintOpacity
        {
            get { return __tintOpacity; }
            set
            {
                if (__tintOpacity != value && (__tintOpacity == 1 || value == 1))
                {
                    _needReset = true;
                }
                __tintOpacity = value;
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

            this.ApplyAccentPolicy(_tintOpacity, _tintColor, flags | User32.AccentFlags.Win11Luminosity);
        }

        private void Theme_Update()
        {
            var theme = ThemeHelper.SystemTheme;
            var useAcrylic = ThemeHelper.AcrylicEnabled;
            var showAccentColorOnSurface = ThemeHelper.ShowAccentColorOnSurface;
            var accentTintColor = Environment.OSVersion.IsAtLeast(OSVersions.VER_11_21H2) ?
                AccentColors.ImmersiveSystemAccentDark2 : AccentColors.ImmersiveSystemAccentDark1;

            if (theme == _theme && useAcrylic == _useAcrylic && showAccentColorOnSurface == _showAccentColorOnSurface)
            {
                if (!showAccentColorOnSurface || (accentTintColor == _accentTintColor))
                {
                    return;
                }
            }

            _theme = theme;
            _useAcrylic = useAcrylic;
            _showAccentColorOnSurface = showAccentColorOnSurface;

            ResourceDictionaryEx.Theme = theme;

            double tintOpacity;
            Color tintColor, fallbackColor;

            if (theme == ColorScheme.Light)
            {
                fallbackColor = COLOR_LIGHT_FALLBACK;
                tintColor = COLOR_LIGHT_TINT;
                tintOpacity = IS_W11 ? 0.9 : 0.853;
            }
            else
            {
                if (showAccentColorOnSurface)
                {
                    _accentTintColor = accentTintColor;
                    fallbackColor = accentTintColor;
                    tintColor = accentTintColor;
                    tintOpacity = IS_W11 ? 0.8 : 0.8;
                }
                else
                {
                    fallbackColor = COLOR_DARK_FALLBACK;
                    tintColor = COLOR_DARK_TINT;
                    tintOpacity = IS_W11 ? 0.5 : 0.85;
                }
            }
            _tray?.SetIconColor(theme.Inverse().ToTrayColor());

            _tintOpacity = useAcrylic ? tintOpacity : 1;
            _tintColor = useAcrylic ? tintColor : fallbackColor;
            Resources["FluentRevealEnabled"] = useAcrylic;
            Resources["FluentRevealEffectsVisibility"] = useAcrylic ? Visibility.Visible : Visibility.Hidden;

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
