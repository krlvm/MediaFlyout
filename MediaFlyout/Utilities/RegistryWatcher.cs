using System;
using System.Management;
using System.Security.Principal;

namespace MediaFlyout.Helpers
{
    class RegistryWatcher : IDisposable
    {
        private const string BASE_QUERY = "SELECT * FROM RegistryValueChangeEvent " +
            "WHERE Hive='{0}' " +
            "AND KeyPath='{1}' " +
            "AND ValueName='{2}'";

        private ManagementEventWatcher watcher;
        public event EventHandler OnChange;

        public RegistryWatcher(string hive, string key, string value)
        {
            watcher = new ManagementEventWatcher(string.Format(BASE_QUERY, hive, key, value));
            watcher.EventArrived += new EventArrivedEventHandler(Handle_Change);
            watcher.Start();
        }

        public void Dispose()
        {
            watcher?.Dispose();
            watcher = null;
        }

        private void Handle_Change(object sender, EventArrivedEventArgs e)
        {
            if (OnChange != null)
            {
                OnChange(this, EventArgs.Empty);
            }
        }

        
        public static RegistryWatcher WatchUser(string key, string value, EventHandler handler = null)
        {
            var watcher = new RegistryWatcher("HKEY_USERS",
                WindowsIdentity.GetCurrent().User.Value + "\\\\" + key.Replace("\\", "\\\\"),
                value);
            watcher.OnChange += handler;
            return watcher;
        }
    }
}