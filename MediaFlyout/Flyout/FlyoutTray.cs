// #define USE_FONT_FOR_ICON

using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using MediaFlyout.Helpers;
using MediaFlyout.Styles;

namespace MediaFlyout
{
    public abstract class FlyoutTray
    {
        protected readonly NotifyIcon notifyIcon;

        protected Color currentColor;

        public bool isClosing = false;
        public bool IsClosing
        {
            get { return isClosing; }
            set { isClosing = value; }
        }

        public FlyoutTray()
        {
            notifyIcon = new NotifyIcon()
            {
                Visible = false
            };

            var listener = new TrayClickListener(notifyIcon);
            listener.Click += OnClick;
            listener.DoubleClick += OnDoubleClick;

            SetIconColor(ThemeHelper.SystemTheme.Inverse().ToTrayColor(), true);
        }

        #region Click Handlers

        protected abstract void OnClick(object sender, MouseEventArgs args);
        protected abstract void OnDoubleClick(object sender, MouseEventArgs args);

        #endregion

        #region Icon Management

        public void SetIconColor(Color color, bool force = false)
        {
            if (currentColor == color && !force) return;
            currentColor = SystemParameters.HighContrast ?
                TrayIconUtil.TranslateColor(System.Windows.SystemColors.WindowTextColor) : color;

            ReloadIcon(color);
        }

        protected abstract void ReloadIcon(Color color);

#if USE_FONT_FOR_ICON
        private const int ICON_SIZE = 16;
        private static readonly Font ICON_FONT = new Font("Segoe MDL2 Assets", 14, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
        private static Icon MakeIcon(string sIcon, Color color)
        {
            float dpiX;
            int dpiW, dpiH;

            using (var bitmap = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    dpiX = g.DpiX;
                    var pixelSize = g.MeasureString(sIcon, ICON_FONT);
                    dpiW = (int)Math.Round(pixelSize.Width);
                    dpiH = (int)Math.Round(pixelSize.Height);
                }
            }

            int size = (int)Math.Round(ICON_SIZE * (dpiX / 96f));
            using (var bmp = new Bitmap(size, size))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    if (color == Color.White || true)
                    {
                        // When System Theme is Dark
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    }
#if TRUE
                    using (var brush = new SolidBrush(color))
                    {
                        g.DrawString(sIcon, ICON_FONT, brush, (size - dpiW) / 2, (size - dpiH) / 2);
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

#endif

    #endregion

        public void Dispose()
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }
    }
}
