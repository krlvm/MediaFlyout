using System;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Windows.Media.Control;
using Microsoft.Win32;
using MediaFlyout.Views;
using MediaFlyout.Flyout;
using MediaFlyout.Extensions;
using System.Threading.Tasks;
using MediaFlyout.Models;

namespace MediaFlyout
{
    public partial class MediaFlyoutWindow : FlyoutWindow
    {
        private MediaFlyoutTray _mediaFlyoutTray;
        private GlobalSystemMediaTransportControlsSessionManager _smtc;

        public MediaFlyoutWindow() : base()
        {
            _mediaFlyoutTray = new MediaFlyoutTray(this);
            Tray = _mediaFlyoutTray;

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

        public async Task TogglePlayback()
        {
            switch (SMTC_GetStatus())
            {
                case MediaPlaybackStatus.Idle: break;
                case MediaPlaybackStatus.Playing:
                    var tasks = _smtc.GetSessions().Select(session => session.TryPauseAsync().AsTask());
                    await Task.WhenAll(tasks);
                    break;
                case MediaPlaybackStatus.Paused:
                    await _smtc.GetCurrentSession()?.TryPlayAsync().AsTask();
                    break;
            }
        }

        public void UpdateStatus()
        {
            _mediaFlyoutTray.SetStatus(SMTC_GetStatus());
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
            foreach (var session in _smtc.GetSessions())
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
            if (_smtc != null)
            {
                _smtc.SessionsChanged -= SMTC_SessionsChanged;
            }

            _smtc = GlobalSystemMediaTransportControlsSessionManager.RequestAsync()
                .AsTask()
                .GetAwaiter()
                .GetResult();

            _smtc.SessionsChanged += SMTC_SessionsChanged;
        }

        private void SMTC_SessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate ()
            {
                UpdateAll();
            });
        }

        private MediaPlaybackStatus SMTC_GetStatus()
        {
            if (_smtc == null || !_smtc.GetSessions().Any())
            {
                return MediaPlaybackStatus.Idle;
            }

            return _smtc.GetSessions()
                .Any(session => session.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                    ? MediaPlaybackStatus.Playing : MediaPlaybackStatus.Paused;
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
