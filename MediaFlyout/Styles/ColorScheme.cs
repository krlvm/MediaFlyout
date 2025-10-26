using System.Windows.Media;

namespace MediaFlyout.Styles
{
    public enum ColorScheme
    {
        Light, Dark
    }
    
    public static class ColorSchemeExtensions
    {
        public static ColorScheme Inverse(this ColorScheme scheme)
        {
            return scheme == ColorScheme.Light ? ColorScheme.Dark : ColorScheme.Light;
        }
        public static Color ToColor(this ColorScheme scheme)
        {
            return scheme == ColorScheme.Light ? Colors.White : Colors.Black;
        }
        public static System.Drawing.Color ToDrawingColor(this ColorScheme scheme)
        {
            return scheme == ColorScheme.Light ? System.Drawing.Color.White : System.Drawing.Color.Black;
        }
    }
}
