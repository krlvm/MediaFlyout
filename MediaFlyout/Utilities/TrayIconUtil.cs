using MediaFlyout.Interop;
using System;
using System.Drawing;

namespace MediaFlyout.Helpers
{
    class TrayIconUtil
    {
        public static Icon LoadIcon(string path, uint dpi, Color color)
        {
            System.Windows.MessageBox.Show("" + User32.GetSystemMetricsForDpi(User32.SystemMetrics.SM_CXICON, dpi));
            using (var stream = System.Windows.Application.GetResourceStream(new Uri(path)).Stream)
            {
                using (var icon = new Icon(stream, new Size(
                    User32.GetSystemMetricsForDpi(User32.SystemMetrics.SM_CXICON, dpi),
                    User32.GetSystemMetricsForDpi(User32.SystemMetrics.SM_CYICON, dpi))
                ))
                {
                    return ColorIcon(icon, 1, color);
                }
            }
        }

        public static Color TranslateColor(System.Windows.Media.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        // https://github.com/File-New-Project/EarTrumpet/blob/master/EarTrumpet/Interop/Helpers/IconHelper.cs
        public static Icon ColorIcon(Icon originalIcon, double fillPercent, Color newColor)
        {
            using (var bitmap = originalIcon.ToBitmap())
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width * fillPercent; x++)
                    {
                        var pixel = bitmap.GetPixel(x, y);

                        if (pixel.R > 220)
                        {
                            bitmap.SetPixel(x, y, Color.FromArgb(pixel.A, newColor.R, newColor.G, newColor.B));
                        }
                    }
                }

                return Icon.FromHandle(bitmap.GetHicon());
            }
        }
    }
}
