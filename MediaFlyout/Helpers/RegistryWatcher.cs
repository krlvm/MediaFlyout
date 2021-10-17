using System;
using System.Management;

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
    }

    class CurrentUserRegistryWatcher : RegistryWatcher
    {
        public CurrentUserRegistryWatcher(string key, string value) : base(
            "HKEY_USERS",
            System.Security.Principal.WindowsIdentity.GetCurrent().User.Value + "\\\\" + key.Replace("\\", "\\\\"),
            value
        )
        {

        }
    }
}