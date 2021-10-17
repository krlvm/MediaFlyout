using System.Drawing;

namespace MediaFlyout.Helpers
{
    class TrayIconHelper
    {
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
