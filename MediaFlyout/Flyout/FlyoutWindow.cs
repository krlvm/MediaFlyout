using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MediaFlyout.Extensions;
using MediaFlyout.Helpers;
using MediaFlyout.Interop;
using MediaFlyout.Styles;

namespace MediaFlyout.Flyout
{
    public abstract class FlyoutWindow : Window
    {
        public FlyoutTray Tray { get; protected set; }
        public bool IsRaising { get; set; }

        public FlyoutWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            SizeToContent = SizeToContent.Height;
            ResizeMode = ResizeMode.NoResize;
            //
            UseLayoutRounding = true;
            SnapsToDevicePixels = true;
            //
            Background = new SolidColorBrush(Color.FromArgb(1, 128, 128, 128));
            BorderThickness = new Thickness(0);
            //
            ShowInTaskbar = false;
            Visibility = Visibility.Hidden;

            Loaded += Window_Loaded;
            Deactivated += Window_Deactivated;
            Closing += Window_Closing;
            PreviewKeyDown += Window_PreviewKeyDown;

            if (Environment.OSVersion.IsWindows11())
            {
                Resources["FluentButtonRadius"] = 4;
                Resources["FluentCornerRadius"] = new CornerRadius(4);

                FontFamily = new FontFamily("Segoe UI Variable Text");

                Constants.GlyphTypeface = new Typeface("Segoe Fluent Icons");
                Resources["IconFont"] = Constants.GlyphTypeface.FontFamily;
            }
            else
            {
                Constants.GlyphTypeface = new Typeface("Segoe MDL2 Assets");
                Resources["IconFont"] = Constants.GlyphTypeface.FontFamily;
            }

            ThemeHelper.Initialize();
            ThemeHelper.OnThemeChanged += Theme_ThemeChanged;
            Theme_Update();

            Left = 999999;
            Show();
            Hide();
        }

        public virtual void RaiseFlyout()
        {
            Theme_Update();
            Theme_Apply();
            AnimationHelper.ShowFlyout(this, true);
        }

        public virtual void DismissFlyout()
        {
            if (Visibility == Visibility.Hidden)
            {
                return;
            }
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

        private static bool IS_W11 = Environment.OSVersion.IsWindows11();

        private readonly Color COLOR_LIGHT_TINT = IS_W11 ? Color.FromRgb(243, 243, 243) : Color.FromRgb(228, 228, 228);
        private readonly Color COLOR_LIGHT_FALLBACK = IS_W11 ? Color.FromRgb(238, 238, 238) : Color.FromRgb(228, 228, 228);
        private readonly Color COLOR_DARK_TINT = IS_W11 ? Color.FromRgb(32, 32, 32) : Color.FromRgb(36, 36, 36);
        private readonly Color COLOR_DARK_FALLBACK = IS_W11 ? Color.FromRgb(28, 28, 28) : Color.FromRgb(31, 31, 31);

        private ColorScheme? _theme;
        private bool _showAccentColorOnSurface;
        private bool _useAcrylic;
        private Color _accentTintColor;
        private Color _tintColor;
        private double _tintOpacity;

        private bool _accentPolicyResetRequired = false;
        public void Theme_Apply()
        {
            if (_accentPolicyResetRequired)
            {
                this.ResetAccentPolicy();
                _accentPolicyResetRequired = false;
            }

            User32.AccentFlags flags = User32.AccentFlags.DrawAllBorders;
            if (Environment.OSVersion.IsWindows11())
            {
                flags |= User32.AccentFlags.Win11Luminosity;
            }
            else
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

            this.ApplyAccentPolicy(_tintOpacity, _tintColor, flags);
        }

        protected void Theme_TrayApply()
        {
            Tray?.SetIconColor((_theme ?? ColorScheme.Dark).Inverse().ToColor());
        }

        private void Theme_Update()
        {
            var theme = ThemeHelper.SystemTheme;
            var useAcrylic = ThemeHelper.AcrylicEnabled;
            var showAccentColorOnSurface = ThemeHelper.ShowAccentColorOnSurface;
            var accentTintColor = IS_W11 ? AccentColors.ImmersiveSystemAccentDark2 : AccentColors.ImmersiveSystemAccentDark1;

            if (theme == _theme && useAcrylic == _useAcrylic && showAccentColorOnSurface == _showAccentColorOnSurface)
            {
                if (!showAccentColorOnSurface || (accentTintColor == _accentTintColor))
                {
                    return;
                }
            }

            ResourceDictionaryEx.Theme = theme;
            Resources["FluentRevealEnabled"] = useAcrylic;
            Resources["FluentRevealEffectsVisibility"] = useAcrylic ? Visibility.Visible : Visibility.Hidden;

            double tintOpacity;
            Color tintColor, fallbackColor;
            //
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

            if (tintOpacity != _tintOpacity)  // also implies (_useAcrylic != useAcrylic), see below
            {
                _accentPolicyResetRequired = true;
            }

            _theme = theme;
            _useAcrylic = useAcrylic;
            _showAccentColorOnSurface = showAccentColorOnSurface;
            _tintOpacity = useAcrylic ? tintOpacity : 1;  // <-- see ApplyAccentPolicy
            _tintColor = useAcrylic ? tintColor : fallbackColor;

            //
            Theme_TrayApply();
            //
        }

        private void Theme_ThemeChanged(object sender, EventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)(() =>
            {
                Theme_Update();

                if (Visibility == Visibility.Visible)
                {
                    Theme_Apply();
                }
            }));
        }

        protected virtual void Theme_ThemeUpdated() { }

        #endregion
    }
}
