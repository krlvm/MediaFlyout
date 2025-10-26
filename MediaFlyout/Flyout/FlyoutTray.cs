using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using MediaFlyout.Helpers;
using MediaFlyout.Styles;

namespace MediaFlyout
{
    public abstract class FlyoutTray : IDisposable
    {
        protected readonly NotifyIcon notifyIcon;

        public bool IsClosing { get; set; }
        protected Color currentColor;

        public FlyoutTray()
        {
            notifyIcon = new NotifyIcon()
            {
                Visible = false
            };

            var listener = new TrayClickListener(notifyIcon);
            listener.Click += OnClick;
            listener.DoubleClick += OnDoubleClick;

            SetIconColor(ThemeHelper.SystemTheme.Inverse().ToColor(), true);
        }

        public void Dispose()
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }

        protected abstract void OnClick(object sender, MouseEventArgs args);
        protected abstract void OnDoubleClick(object sender, MouseEventArgs args);

        public void SetIconColor(Color color, bool force = false)
        {
            if (currentColor == color && !force)
            {
                return;
            }
            currentColor = SystemParameters.HighContrast ? SystemColors.WindowTextColor : color;

            ReloadIcon(color);
        }

        protected abstract void ReloadIcon(Color color);
    }
}
