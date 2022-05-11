#define USE_FONT_FOR_ICON

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using MediaFlyout.Helpers;
using MediaFlyout.Properties;

namespace MediaFlyout
{
    class MediaFlyoutTray : FlyoutTray
    {
        private readonly MediaFlyoutWindow flyout;

        private Icon[] icons = new Icon[2];
        private bool? currentPlaybackStatus;

        public MediaFlyoutTray(MediaFlyoutWindow flyout)
        {
            this.flyout = flyout;
            SetStatus(null);
        }

        public void SetStatus(bool? isPlaying, bool force = false)
        {
            if (currentPlaybackStatus == isPlaying && !force) return;

            currentPlaybackStatus = isPlaying;

            if (isPlaying == null)
            {
                notifyIcon.Visible = false;
                return;
            }

            try
            {
                notifyIcon.Icon = icons[(bool)isPlaying ? 0 : 1];
            }
            catch (ObjectDisposedException)
            {
                SetIconColor(currentColor, true);
            }
            notifyIcon.Visible = true;
            notifyIcon.Text = (bool)isPlaying ? Resources.Tray_Pause : Resources.Tray_Play;
        }

        #region Click Handlers

        protected override void OnClick(object sender, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Middle)
            {
                flyout.TogglePlayback();
                return;
            }

            if (isClosing) return;

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
            flyout.Theme_Apply();
        }

        protected override void OnDoubleClick(object sender, MouseEventArgs args)
        {
            flyout.TogglePlayback();
        }

        #endregion

        protected override void ReloadIcon(Color color)
        {
            foreach (var icon in icons)
            {
                icon?.Dispose();
            }

            icons[0] = TrayIconUtil.LoadIcon(Resources.Icon_Pause_100, Resources.Icon_Pause, color);
            icons[1] = TrayIconUtil.LoadIcon(Resources.Icon_Play_100,  Resources.Icon_Play, color);

            SetStatus(currentPlaybackStatus, true);
        }
    }
}
