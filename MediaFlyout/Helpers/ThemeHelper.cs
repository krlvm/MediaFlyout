using System;
using MediaFlyout.Extensions;
using MediaFlyout.Styles;
using MediaFlyout.Utilities;

namespace MediaFlyout.Helpers
{
    public sealed class ThemeHelper
    {
        private const string REG_PERSONALIZATION_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private const string SYSTEM_THEME_VALUE = "SystemUsesLightTheme";
        private const string APPS_THEME_VALUE = "AppsUseLightTheme";
        private const string ACCENT_SURFACE_VALUE = "ColorPrevalence";
        private const string ACRYLIC_VALUE = "EnableTransparency";

        public event EventHandler OnThemeChanged;

        private static ThemeHelper _instance = null;
        public static ThemeHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ThemeHelper();
                }
                return _instance;
            }
        }

        private ThemeHelper()
        {
            AccentColors.Initialize();
            AccentColors.WatchAccentColors();
        }

        internal void HandleThemeChange(object sender, EventArgs e)
        {
            if (OnThemeChanged != null)
            {
                OnThemeChanged(this, EventArgs.Empty);
            }
        }

        private void Dispose()
        {
            OnThemeChanged = null;
        }

        public static ColorScheme SystemTheme
        {
            get
            {
                if (Environment.OSVersion.IsLessThan(OSVersions.VER_19H1)) return ColorScheme.Dark;
                return RegistryUtil.IsValueEnabled(REG_PERSONALIZATION_KEY, SYSTEM_THEME_VALUE) ? ColorScheme.Light : ColorScheme.Dark;
            }
        }

        public static ColorScheme Apps
        {
            get
            {
                return RegistryUtil.IsValueEnabled(REG_PERSONALIZATION_KEY, APPS_THEME_VALUE) ? ColorScheme.Light : ColorScheme.Dark;
            }
        }

        public static bool ShowAccentColorOnSurface
        {
            get
            {
                return RegistryUtil.IsValueEnabled(REG_PERSONALIZATION_KEY, ACCENT_SURFACE_VALUE);
            }
        }

        public static bool AcrylicEnabled
        {
            get
            {
                return RegistryUtil.IsValueEnabled(REG_PERSONALIZATION_KEY, ACRYLIC_VALUE);
            }
        }
    }
}
