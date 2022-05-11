using Microsoft.Win32;

namespace MediaFlyout.Utilities
{
    class RegistryUtil
    {
        public static bool IsValueEnabled(string subKey, string value, bool defaultValue = false)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(subKey))
            {
                if (key == null)
                {
                    return defaultValue;
                }
                int? val = key.GetValue(value) as int?;
                if (val == null)
                {
                    return defaultValue;
                }
                return val != 0;
            }
        }
    }
}
