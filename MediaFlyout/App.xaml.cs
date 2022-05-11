using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace MediaFlyout
{
    public partial class App : Application
    {
        App()
        {
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
    }
}
