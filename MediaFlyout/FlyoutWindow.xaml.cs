using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Windows.Media.Control;
using SourceChord.FluentWPF;
using System.Windows.Media;
using MediaFlyout.Helpers;
using MediaFlyout.Views;
using Microsoft.Win32;

namespace MediaFlyout
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class FlyoutWindow : IFlyout
    {
        private TrayManager tray;
        private GlobalSystemMediaTransportControlsSessionManager smtc;

        #region State

        private bool _IsRaising = false;
        public bool IsRaising
        {
            get { return _IsRaising; }
            set { _IsRaising = value; }
        }

        private double _TintOpacity;
        public double TintOpacity
        {
            get { return _TintOpacity; }
            set
            {
                if (_TintOpacity != value && (_TintOpacity == 1 || value == 1))
                {
                    _needReset = true;
                }
                _TintOpacity = value; 
            }
        }
        private Color _TintColor;
        public Color TintColor
        {
            get { return _TintColor; }
            set { _TintColor = value; }
        }

        private bool _needReset = false;

        #endregion

        public FlyoutWindow()
        {
            InitializeComponent();

            SMTC_Initialize();
            tray = new TrayManager(this);

            Theme_Initialize();

            UpdateSessions();
            UpdateStatus();

            PrepareWindow();

            SystemEvents.PowerModeChanged += OnPowerModeChanged;
        }

        #region Window Management

        private void PrepareWindow()
        {
            Left = 999999;
            Show();
            Hide();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //InteropHelper.HideWindowFromAltTab(this);
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

        public async void DismissFlyout()
        {
            if (Visibility == Visibility.Hidden) return;
            Left = 999999;
            tray.isClosing = true;
            await System.Threading.Tasks.Task.Delay(System.Windows.Forms.SystemInformation.DoubleClickTime / 2);
            Visibility = Visibility.Hidden;
            tray.isClosing = false;
        }

        #endregion

        public void ReloadFlyout()
        {
            InvalidateSessions();
            SMTC_Initialize();
            UpdateAll();
        }

        #region Sleep Management
        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs args)
        {
            if (args.Mode == PowerModes.Resume)
            {
                ReloadFlyout();
            }
        }
        #endregion

        #region Session Management

        public void TogglePlayback()
        {
            bool? isPlaying = SMTC_IsPlaying();
            if (isPlaying == null) return;

            if ((bool)isPlaying)
            {
                foreach (var session in smtc.GetSessions())
                {
                    session.TryPauseAsync();
                }
            }
            else
            {
                smtc.GetCurrentSession()?.TryPlayAsync();
            }
        }

        public void UpdateStatus()
        {
            tray.SetStatus(SMTC_IsPlaying());
        }

        private void InvalidateSessions()
        {
            foreach (var item in SessionsStackPanel.Children)
            {
                ((SessionItem)item).Invalidate();
            }
            SessionsStackPanel.Children.Clear();
        }

        private void UpdateSessions()
        {
            InvalidateSessions();
            foreach (var session in smtc.GetSessions())
            {
                SessionsStackPanel.Children.Add(new SessionItem(this, session));
            }
        }

        private void UpdateAll()
        {
            UpdateStatus();
            UpdateSessions();
        }

        #endregion

        #region SMTC Management

        private void SMTC_Initialize()
        {
            if (smtc != null)
            {
                smtc.SessionsChanged -= SMTC_SessionsChanged;
            }

            //smtc = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            var task = GlobalSystemMediaTransportControlsSessionManager.RequestAsync().AsTask();
            task.Wait();
            smtc = task.Result;

            smtc.SessionsChanged += SMTC_SessionsChanged;
        }

        private void SMTC_SessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate ()
            {
                UpdateAll();
            });
        }

        private bool? SMTC_IsPlaying()
        {
            if (smtc == null || !smtc.GetSessions().Any()) return null;

            bool isPlaying = false;
            foreach (var session in smtc.GetSessions())
            {
                if (session.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                {
                    isPlaying = true;
                }
            }
            return isPlaying;
        }

        #endregion

        #region Theme Management

        private const string REG_PERSONALIZATION_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string ACCENT_SURFACE_VALUE = "ColorPrevalence";
        private const string ACRYLIC_VALUE = "EnableTransparency";

        private WindowsTheme theme_last;
        private bool theme_accentColorListening = false;

        private int OS_BUILD_NUMBER = SystemHelper.GetBuildNumber();

        private void Theme_Initialize()
        {
            new CurrentUserRegistryWatcher(REG_PERSONALIZATION_KEY, ACCENT_SURFACE_VALUE).OnChange += Theme_AccentSurfaceChanged;
            new CurrentUserRegistryWatcher(REG_PERSONALIZATION_KEY, ACRYLIC_VALUE).OnChange += Acrylic_ValueChanged;

            SystemTheme.ThemeChanged += Theme_ThemeChanged;

            Theme_Update();
        }

        public void Theme_Apply()
        {
            if (_needReset)
            {
                AcrylicHelper.Reset(this);
                _needReset = false;
            }

            AccentFlags flags;
            switch (WindowsTaskbar.Current.Side)
            {
                case WindowsTaskbar.Position.Left:
                    flags = AccentFlags.DrawRightBorder | AccentFlags.DrawTopBorder;
                    break;
                case WindowsTaskbar.Position.Right:
                    flags = AccentFlags.DrawLeftBorder | AccentFlags.DrawTopBorder;
                    break;
                case WindowsTaskbar.Position.Top:
                    flags = AccentFlags.DrawLeftBorder | AccentFlags.DrawBottomBorder;
                    break;
                case WindowsTaskbar.Position.Bottom:
                    flags = AccentFlags.DrawTopBorder | AccentFlags.DrawLeftBorder;
                    break;
                default:
                    flags = AccentFlags.None;
                    break;
            }
            AcrylicHelper.Apply(this, TintOpacity, TintColor, flags);
        }

        private void Theme_Update()
        {
            double tintOpacity;
            Color tintColor, fallbackColor;
            Color contrastScheme, contrastColor;

            bool isAccentSurface = Theme_IsSurfaceAccentColor();
            if (isAccentSurface)
            {
                if (!theme_accentColorListening)
                {
                    AccentColors.StaticPropertyChanged += Theme_AccentColorChanged;
                }
            }
            else
            {
                if (theme_accentColorListening)
                {
                    AccentColors.StaticPropertyChanged -= Theme_AccentColorChanged;
                }
            }

            WindowsTheme theme = Theme_GetSystemTheme();
            if (theme == WindowsTheme.Light)
            {
                ResourceDictionaryEx.GlobalTheme = ElementTheme.Light;
                fallbackColor = Color.FromRgb(228, 228, 228);
                tintColor = Color.FromRgb(228, 228, 228);
                tintOpacity = 0.853;
                contrastScheme = Colors.White;
                contrastColor = Colors.Black;
                tray.SetIconColor(System.Drawing.Color.Black);
            }
            else
            {
                ResourceDictionaryEx.GlobalTheme = ElementTheme.Dark;
                if (Theme_IsSurfaceAccentColor())
                {
                    fallbackColor = AccentColors.ImmersiveSystemAccentDark1;
                    tintColor = AccentColors.ImmersiveSystemAccentDark1;
                    tintOpacity = 0.8;
                }
                else
                {
                    fallbackColor = Color.FromRgb(31, 31, 31);
                    tintColor = Color.FromRgb(36, 36, 36);
                    tintOpacity = 0.85;
                }
                contrastScheme = Colors.Black;
                contrastColor = Colors.White;
                tray.SetIconColor(System.Drawing.Color.White);
            }

            TintOpacity = Acrylic_IsEnabled() ? tintOpacity : 1;
            TintColor = Acrylic_IsEnabled() ? tintColor : fallbackColor;
            Resources["FlyoutColorScheme"] = contrastScheme;
            Resources["FlyoutContrastColor"] = new SolidColorBrush(contrastColor);
            Resources["FluentRevealEnabled"] = Acrylic_IsEnabled();

            if (theme_last != theme)
            {
                UpdateSessions();
            }
            theme_last = theme;

            if (Visibility == Visibility.Visible)
            {
                Theme_Apply();
            }
        }

        private void Theme_ThemeChanged(object sender, EventArgs args)
        {
            Theme_Update();
        }

        private void Theme_AccentColorChanged(object sender, EventArgs args)
        {
            //Theme_Update();
        }

        private void Theme_AccentSurfaceChanged(object sender, EventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate ()
            {
                Theme_Update();
            });
        }

        private bool Theme_IsSurfaceAccentColor()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REG_PERSONALIZATION_KEY))
            {
                if (key == null)
                {
                    return false;
                }
                int? val = key.GetValue(ACCENT_SURFACE_VALUE) as int?;
                if (val == null)
                {
                    return false;
                }
                return val != 0;
            }
        }

        private WindowsTheme Theme_GetSystemTheme()
        {
            if (OS_BUILD_NUMBER < 18282) return WindowsTheme.Dark;
            return SystemTheme.WindowsTheme;
        }

        private void Acrylic_ValueChanged(object sender, EventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate ()
            {
                Theme_Update();
            });
        }

        private bool Acrylic_IsEnabled()
        {
            return RegistryHelper.GetCUKeyValueOrFalse(REG_PERSONALIZATION_KEY, ACRYLIC_VALUE);
        }

        #endregion
    }
}
