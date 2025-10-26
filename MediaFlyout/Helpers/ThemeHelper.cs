using System;
using MediaFlyout.Extensions;
using MediaFlyout.Styles;
using MediaFlyout.Utilities;

namespace MediaFlyout.Helpers
{
    public sealed class ThemeHelper
    {
        private const string REG_PERSONALIZATION_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        public static event EventHandler OnThemeChanged;

        private static bool s_isInitialized = false;
        public static void Initialize()
        {
            if (s_isInitialized)
            {
                return;
            }

            AccentColors.Initialize();
            AccentColors.WatchAccentColors();

            s_isInitialized = true;
        }

        internal static void HandleThemeChange(object sender, EventArgs e)
        {
            OnThemeChanged?.Invoke(null, null);
        }

        public static ColorScheme SystemTheme => (!RegistryUtil.IsValueEnabled(REG_PERSONALIZATION_KEY, "SystemUsesLightTheme") || Environment.OSVersion.IsLessThan(OSVersions.VER_19H1)) ? ColorScheme.Dark : ColorScheme.Light;
        public static ColorScheme AppsTheme => RegistryUtil.IsValueEnabled(REG_PERSONALIZATION_KEY, "AppsUseLightTheme") ? ColorScheme.Light : ColorScheme.Dark;

        public static bool ShowAccentColorOnSurface => RegistryUtil.IsValueEnabled(REG_PERSONALIZATION_KEY, "ColorPrevalence");
        public static bool AcrylicEnabled => RegistryUtil.IsValueEnabled(REG_PERSONALIZATION_KEY, "EnableTransparency");
    }
}
