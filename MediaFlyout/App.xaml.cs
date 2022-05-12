using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace MediaFlyout
{
    public partial class App : Application
    {
        App()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
#if !DEBUG
            // Kill other MediaFlyout instances
            Process currentProcess = Process.GetCurrentProcess();
            Process.GetProcesses()
                .Where(process => process.ProcessName == currentProcess.ProcessName)
                .Where(process => process.Id != currentProcess.Id)
                .ToList()
                .ForEach(process => process.Kill());
#endif
        }

        public void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            RestartApp();
        }

        public static void RestartApp()
        {
            Process.Start(ResourceAssembly.Location);
            Current.Shutdown();
        }
    }
}
