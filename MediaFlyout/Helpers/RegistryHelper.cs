using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaFlyout.Helpers
{
    class RegistryHelper
    {
        public static bool GetCUKeyValueOrFalse(string subkey, string value)
        {
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(subkey))
            {
                if (key == null)
                {
                    return false;
                }
                int? val = key.GetValue(value) as int?;
                if (val == null)
                {
                    return false;
                }
                return val != 0;
            }
        }
    }
}
