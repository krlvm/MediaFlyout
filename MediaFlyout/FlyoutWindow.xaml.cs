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

namespace MediaFlyout
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class FlyoutWindow : IFlyout
    {
        private TrayManager tray;
        private GlobalSystemMediaTransportControlsSessionManager smtc;

        private bool _IsRaising = false;
        public bool IsRaising
        {
            get { return _IsRaising; }
            set { _IsRaising = value; }
        }

        public FlyoutWindow()
        {
            InitializeComponent();

            SMTC_Initialize();
            tray = new TrayManager(this);

            Theme_Initialize();

            UpdateSessions();
            UpdateStatus();

            PrepareWindow();
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
            WindowStyle = WindowStyle.None;
            Visibility = Visibility.Hidden;
            tray.isClosing = false;
        }

        #endregion

        #region Session Management

        public void TogglePlayback()
        {
            if ((bool)SMTC_IsPlaying())
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

        private void UpdateSessions()
        {
            foreach (var item in SessionsStackPanel.Children)
            {
                ((SessionItem)item).Invalidate();
            }
            SessionsStackPanel.Children.Clear();

            foreach (var session in smtc.GetSessions())
            {
                SessionsStackPanel.Children.Add(new SessionItem(this, session));
            }
        }

        #endregion

        #region SMTC Management

        private void SMTC_Initialize()
        {
            if (smtc != null) throw new InvalidOperationException();

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
                UpdateStatus();
                UpdateSessions();
            });
        }

        private bool? SMTC_IsPlaying()
        {
            if (!smtc.GetSessions().Any()) return null;

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
        private bool theme_accentColorListening = false;

        private bool bEnableFluentAcrylic = false;

        private void Theme_Initialize()
        {
            new CurrentUserRegistryWatcher(REG_PERSONALIZATION_KEY, ACCENT_SURFACE_VALUE).OnChange += Theme_AccentSurfaceChanged;
            new CurrentUserRegistryWatcher(REG_PERSONALIZATION_KEY, ACRYLIC_VALUE).OnChange += Acrylic_ValueChanged;

            SystemTheme.ThemeChanged += Theme_ThemeChanged;

            bEnableFluentAcrylic = RegistryHelper.GetCUKeyValueOrFalse(@"SOFTWARE\krlvm\MediaFlyout", "EnableFluentAcrylic");
            Theme_Apply();
        }

        private void Theme_Apply()
        {
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

            if (SystemTheme.WindowsTheme == WindowsTheme.Light)
            {
                ResourceDictionaryEx.GlobalTheme = ElementTheme.Light;
                Resources["FlyoutFallbackColor"] = Color.FromRgb(228, 228, 228);
                Resources["FlyoutTintColor"] = Color.FromRgb(228, 228, 228);
                Resources["FlyoutTintOpacity"] = Acrylic_IsEnabled() ? 0.853 : 1;
                Resources["FlyoutColorScheme"] = Color.FromRgb(255, 255, 255);
                Resources["FlyoutContrastColor"] = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                tray.SetIconColor(System.Drawing.Color.Black);
            }
            else
            {
                ResourceDictionaryEx.GlobalTheme = ElementTheme.Dark;
                double tintOpacity;
                if(Theme_IsSurfaceAccentColor())
                {
                    Resources["FlyoutFallbackColor"] = AccentColors.ImmersiveSystemAccentDark1;
                    Resources["FlyoutTintColor"] = AccentColors.ImmersiveSystemAccentDark1;
                    tintOpacity = 0.8;
                }
                else
                {
                    Resources["FlyoutFallbackColor"] = Color.FromRgb(31, 31, 31);
                    Resources["FlyoutTintColor"] = Acrylic_IsEnabled() ? Color.FromRgb(36, 36, 36) : Color.FromRgb(31, 31, 31);
                    tintOpacity = 0.85;
                }
                Resources["FlyoutTintOpacity"] = Acrylic_IsEnabled() ? tintOpacity : 1;
                Resources["FlyoutColorScheme"] = Color.FromRgb(0, 0, 0);
                Resources["FlyoutContrastColor"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                tray.SetIconColor(System.Drawing.Color.White);
            }

            if (Acrylic_IsEnabled())
            {
                if (bEnableFluentAcrylic)
                {
                    Resources["FlyoutTintOpacity"] = ((double)Resources["FlyoutTintOpacity"])
                        + (SystemTheme.WindowsTheme == WindowsTheme.Light ? -0.1 : -0.05);
                    Resources["FlyoutNoiseOpacity"] = 0.025;
                    Resources["FlyoutAccentState"] = AcrylicAccentState.AcrylicBlurBehind;
                }
                else
                {
                    Resources["FlyoutNoiseOpacity"] = 0.025;
                    Resources["FlyoutAccentState"] = AcrylicAccentState.BlurBehind;
                }
                Resources["FluentRevealEnabled"] = true;
            }
            else
            {
                Resources["FlyoutNoiseOpacity"] = 0.00000001;
                Resources["FlyoutAccentState"] = AcrylicAccentState.Disabled;
                Resources["FluentRevealEnabled"] = false;
            }
        }

        private void Theme_ThemeChanged(object sender, EventArgs args)
        {
            Theme_Apply();
        }

        private void Theme_AccentColorChanged(object sender, EventArgs args)
        {
            //Theme_Apply();
        }

        private void Theme_AccentSurfaceChanged(object sender, EventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate ()
            {
                Theme_Apply();
            });
        }

        private bool Theme_IsSurfaceAccentColor()
        {
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(REG_PERSONALIZATION_KEY))
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

        private const string ACRYLIC_VALUE = "EnableTransparency";

        private void Acrylic_ValueChanged(object sender, EventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate ()
            {
                Theme_Apply();
            });
        }

        private bool Acrylic_IsEnabled()
        {
            return RegistryHelper.GetCUKeyValueOrFalse(REG_PERSONALIZATION_KEY, ACRYLIC_VALUE);
        }

        #endregion
    }
}
