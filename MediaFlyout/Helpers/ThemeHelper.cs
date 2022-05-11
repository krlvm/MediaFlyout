using System;
using System.Windows.Media;
using MediaFlyout.Extensions;
using MediaFlyout.Utilities;

namespace MediaFlyout.Helpers
{
    public sealed class ThemeHelper
    {
        private const string REG_PERSONALIZATION_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string ACCENT_SURFACE_VALUE = "ColorPrevalence";
        private const string ACRYLIC_VALUE = "EnableTransparency";

        private RegistryWatcher regAccentSurfaceWatcher;
        private RegistryWatcher regAcrylicWatcher;
        public event EventHandler OnThemeChanged;

        public ThemeHelper()
        {
            regAccentSurfaceWatcher = RegistryWatcher.WatchUser(REG_PERSONALIZATION_KEY, ACCENT_SURFACE_VALUE, HandleThemeChange);
            regAcrylicWatcher = RegistryWatcher.WatchUser(REG_PERSONALIZATION_KEY, ACRYLIC_VALUE, HandleThemeChange);

            SourceChord.FluentWPF.SystemTheme.ThemeChanged += HandleThemeChange;
        }

        private void HandleThemeChange(object sender, EventArgs e)
        {
            if (OnThemeChanged != null)
            {
                OnThemeChanged(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            regAccentSurfaceWatcher.Dispose();
            regAcrylicWatcher.Dispose();

            SourceChord.FluentWPF.SystemTheme.ThemeChanged -= HandleThemeChange;
        }

        public static FlyoutTheme GetTheme()
        {
            return new FlyoutTheme
            {
                SystemTheme = Environment.OSVersion.IsLessThan(OSVersions.VER_19H1) ? ColorScheme.Dark : (
                    SourceChord.FluentWPF.SystemTheme.WindowsTheme == SourceChord.FluentWPF.WindowsTheme.Dark ? ColorScheme.Dark : ColorScheme.Light
                ),
                AccentColor = SourceChord.FluentWPF.AccentColors.ImmersiveSystemAccentDark1,
                ShowAccentColorOnSurface = IsShowAccentColorOnSurface(),
                AcrylicEnabled = IsAcrylicEnabled()
            };
        }

        public static bool IsShowAccentColorOnSurface()
        {
            return RegistryUtil.IsValueEnabled(REG_PERSONALIZATION_KEY, ACCENT_SURFACE_VALUE);
        }

        public static bool IsAcrylicEnabled()
        {
            return RegistryUtil.IsValueEnabled(REG_PERSONALIZATION_KEY, ACRYLIC_VALUE);
        }
    }

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
        public static System.Drawing.Color ToTrayColor(this ColorScheme scheme)
        {
            return scheme == ColorScheme.Light ? System.Drawing.Color.White : System.Drawing.Color.Black;
        }
    }

    public struct FlyoutTheme
    {
        public ColorScheme SystemTheme;
        public Color AccentColor;
        public bool ShowAccentColorOnSurface;
        public bool AcrylicEnabled;
    } 
}
