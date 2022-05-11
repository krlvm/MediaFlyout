using System.Windows.Media;

namespace MediaFlyout.Extensions
{
    static class ColorExtensions
    {
        public static uint ToABGR(this Color color)
        {
            return (uint)(
                color.A << 24 |
                color.B << 16 |
                color.G << 8 |
                color.R
            );
        }
    }
}
