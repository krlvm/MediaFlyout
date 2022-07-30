using System;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Windows.Media.Control;
using Microsoft.Win32;
using MediaFlyout.Views;
using MediaFlyout.Flyout;
using MediaFlyout.Extensions;

namespace MediaFlyout
{
    public partial class MediaFlyoutWindow : FlyoutWindow
    {
        private MediaFlyoutTray tray;
        private GlobalSystemMediaTransportControlsSessionManager smtc;

        private bool problemDetected = false;

        public MediaFlyoutWindow()
        {
            tray = new MediaFlyoutTray(this);
            _tray = tray;

            InitializeComponent();

            SMTC_Initialize();

            UpdateSessions();
            UpdateStatus();

            if (Environment.OSVersion.IsWindows11())
            {
                Resources["ButtonPlaySize"] = 14.0;
                Resources["ButtonPrev"] = "\uF8AC";
                Resources["ButtonNext"] = "\uF8AD";
            }
            else
            {
                Resources["ButtonPlaySize"] = 18.0;
                Resources["ButtonPrev"] = "\uE892";
                Resources["ButtonNext"] = "\uE893";
            }

            SystemEvents.PowerModeChanged += OnPowerModeChanged;
        }

        public override void RaiseFlyout()
        {
            if (SessionsStackPanel.Children.Count == 0)
            {
                if (problemDetected)
                {
                    App.RestartApp();
                    return;
                }
                problemDetected = true;
                ReloadFlyout();
            }
            base.RaiseFlyout();
        }

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

        protected override void Theme_ThemeUpdated()
        {
            UpdateSessions();
        }

        #endregion
    }
}
