using System;
using System.Windows.Media;
using MediaFlyout.Extensions;

namespace MediaFlyout
{
    internal class Constants
    {
        public static readonly string GLYPH_PLAY = Environment.OSVersion.IsWindows11() ? "\uF5B0" : "\uE768";
        public static readonly string GLYPH_PAUSE = Environment.OSVersion.IsWindows11() ? "\uF8AE" : "\uE769";

        public static Typeface GlyphTypeface { get; set; }
    }
}
