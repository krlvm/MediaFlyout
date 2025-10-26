using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using MediaFlyout.Helpers;
using MediaFlyout.Models;
using MediaFlyout.Properties;

namespace MediaFlyout
{
    class MediaFlyoutTray : FlyoutTray
    {
        private readonly MediaFlyoutWindow flyout;

        private (System.Drawing.Icon playing, System.Drawing.Icon paused) icons;
        private MediaPlaybackStatus currentPlaybackStatus;

        public MediaFlyoutTray(MediaFlyoutWindow flyout)
        {
            this.flyout = flyout;
            SetStatus(MediaPlaybackStatus.Idle);
        }

        public void SetStatus(MediaPlaybackStatus status, bool force = false)
        {
            if (currentPlaybackStatus == status && !force)
            {
                return;
            }

            currentPlaybackStatus = status;

            switch (status)
            {
                case MediaPlaybackStatus.Idle:
                    notifyIcon.Visible = false;
                    return; //

                case MediaPlaybackStatus.Playing:
                    notifyIcon.Icon = icons.playing;
                    notifyIcon.Text = Resources.Tray_Pause;
                    break;
                case MediaPlaybackStatus.Paused:
                    notifyIcon.Icon = icons.paused;
                    notifyIcon.Text = Resources.Tray_Play;
                    break;
            }

            notifyIcon.Visible = true;
        }

        #region Click Handlers

        protected override async void OnClick(object sender, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Middle)
            {
                await flyout.TogglePlayback();
                return;
            }

            if (IsClosing)
            {
                return;
            }

            if (flyout.Visibility == Visibility.Visible)
            {
                flyout.DismissFlyout();
                return;
            }

            if (args.Button == MouseButtons.Right)
            {
                flyout.ReloadFlyout();
            }

            flyout.Topmost = false;
            flyout.RaiseFlyout();
        }

        protected override async void OnDoubleClick(object sender, MouseEventArgs args)
        {
            await flyout.TogglePlayback();
        }

        #endregion

        protected override void ReloadIcon(Color color)
        {
            icons.playing?.Dispose();
            icons.paused?.Dispose();

            var dpi = WindowsTaskbar.Dpi;
            icons.playing = GlyphIconMaker.MakeIcon(Constants.GlyphTypeface, Constants.GLYPH_PAUSE, color, dpi);
            icons.paused = GlyphIconMaker.MakeIcon(Constants.GlyphTypeface, Constants.GLYPH_PLAY, color, dpi);

            SetStatus(currentPlaybackStatus, true);
        }
    }
}
