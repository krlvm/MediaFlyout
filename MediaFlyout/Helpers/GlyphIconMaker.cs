using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using MediaFlyout.Extensions;

namespace MediaFlyout.Helpers
{
    sealed class GlyphIconMaker
    {
        private const int ICON_SIZE = 16;

        public static System.Drawing.Icon MakeIcon(Typeface typeface, string glyph, Color color, uint dpi)
        {
            double dpiScaling = dpi / 96.0f;
            double iconSize = ICON_SIZE * dpiScaling;

            var visual = new DrawingVisual();
            using (var ctx = visual.RenderOpen())
            {
                var ft = new FormattedText(
                    glyph,
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    ICON_SIZE,
                    new SolidColorBrush(color),
                    dpiScaling
                );
                ctx.DrawText(ft, new Point((ICON_SIZE - ft.Width) / 2, (ICON_SIZE - ft.Height) / 2));
            }

            var rtb = new RenderTargetBitmap((int)Math.Ceiling(iconSize), (int)Math.Ceiling(iconSize), dpi, dpi, PixelFormats.Pbgra32);
            rtb.Render(visual);

            using (var bmp = new System.Drawing.Bitmap(rtb.PixelWidth, rtb.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
            {
                var data = bmp.LockBits(
                    new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                rtb.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
                bmp.UnlockBits(data);

                return System.Drawing.Icon.FromHandle(bmp.GetHicon()).AsDisposableIcon();
            }
        }
    }
}
