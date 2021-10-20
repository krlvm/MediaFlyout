#define USE_FONT_FOR_ICON

using MediaFlyout.Helpers;
using MediaFlyout.Interop;
using MediaFlyout.Properties;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows;
using System.Windows.Forms;

namespace MediaFlyout
{
    class TrayManager
    {
        private readonly FlyoutWindow flyout;
        private readonly NotifyIcon tray;

        private Color trayIconColor;
        private Icon[] icons = new Icon[2];
        private bool? currentPlaybackStatus;

        public bool isClosing = false;

        public TrayManager(FlyoutWindow flyout)
        {
            this.flyout = flyout;

            tray = new NotifyIcon()
            {
                Visible = false
            };

            TrayClickListener listener = new TrayClickListener(tray);
            listener.Click += OnClick;
            listener.DoubleClick += OnDoubleClick;

            SetIconColor(SourceChord.FluentWPF.SystemTheme.WindowsTheme == SourceChord.FluentWPF.WindowsTheme.Light ? Color.Black : Color.White);
            SetStatus(null);
        }

        public void SetStatus(bool? isPlaying, bool force = false)
        {
            if (currentPlaybackStatus == isPlaying && !force) return;

            currentPlaybackStatus = isPlaying;

            if (isPlaying == null)
            {
                tray.Visible = false;
                return;
            }

            tray.Visible = true;
            tray.Text = (bool)isPlaying ? Resources.Tray_Pause : Resources.Tray_Play;

            try
            {
                tray.Icon = icons[(bool)isPlaying ? 0 : 1];
            }
            catch (ObjectDisposedException)
            {
                // The icon has been disposed
                // Generate another one
                SetIconColor(trayIconColor, true);
            }
        }

        #region Click Handlers

        private void OnClick(object sender, EventArgs args)
        {
            if (isClosing) return;

            if (flyout.Visibility == Visibility.Visible)
            {
                flyout.DismissFlyout();
                return;
            }

            AnimationHelper.ShowFlyout(flyout);
        }

        private void OnDoubleClick(object sender, EventArgs args)
        {
            flyout.TogglePlayback();
        }

        #endregion

        #region Icon Management

        public void SetIconColor(Color color, bool force = false)
        {
            if (trayIconColor == color && !force) return;
            trayIconColor = SystemParameters.HighContrast ? 
                TrayIconHelper.TranslateColor(System.Windows.SystemColors.WindowTextColor) : color;

            foreach (Icon icon in icons)
            {
                if (icon != null)
                {
                    tray.Icon.Dispose();
                }
            }

            icons[0] = GetIcon(true, color);
            icons[1] = GetIcon(false, color);

            SetStatus(currentPlaybackStatus, true);
        }

        private static Icon GetIcon(bool isPlaying, Color color)
        {
#if TRUE
            return LoadIcon(isPlaying, color);
#else
            return MakeIcon(isPlaying, color);
#endif
        }

        private static Icon LoadIcon(bool isPlaying, Color color)
        {
            Icon icon = isPlaying ? Resources.Icon_Pause : Resources.Icon_Play;
            if (color != Color.White)
            {
                Icon newIcon = TrayIconHelper.ColorIcon(icon, 1, color);
                icon.Dispose();
                return newIcon;
            }
            return icon;
        }

        // https://stackoverflow.com/a/36379999
        private static Icon MakeIcon(bool isPlaying, Color color)
        {
            var str = isPlaying ? "" : "";
            using (var font = new Font("Segoe MDL2 Assets", 16, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel))
            {
                using (var bmp = new Bitmap(16, 16))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        if (color == Color.White)
                        {
                            // When System Theme is Dark
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        }

                        g.Clear(Color.Transparent);
#if TRUE
                        using (var brush = new SolidBrush(color))
                        {
                            g.DrawString(str, font, brush, -4, 0);
                        }
#else
                        TextRenderer.DrawText(g, str, font, new Rectangle(0, 0, 16, 16), color, Color.Transparent);
#endif
                        //bmp.Save(@"%UserProfile%\Desktop\MFIcon.png", System.Drawing.Imaging.ImageFormat.Png);
                        IntPtr hIcon = bmp.GetHicon();
                        return Icon.FromHandle(hIcon);
                    }
                }
            }
        }

#endregion
    }
}
