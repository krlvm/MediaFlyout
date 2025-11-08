using System;
using System.Diagnostics;
using System.Text;
using static MediaFlyout.Interop.Kernel32;

namespace MediaFlyout.Utilities
{
    struct ProcessInfo
    {
        public string path;
        public string description;
    }

    internal class ProcessInfoExtractor
    {
        public static ProcessInfo? ExtractInfo(Process process)
        {
            try
            {
                var mainModule = process.MainModule;
                return new ProcessInfo
                {
                    path = mainModule.FileName,
                    description = mainModule.FileVersionInfo.FileDescription,
                };
            }
            catch
            {
                IntPtr hProcess = IntPtr.Zero;
                try
                {
                    hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, process.Id);
                    if (hProcess == IntPtr.Zero)
                    {
                        return null;
                    }

                    var sb = new StringBuilder(1024);
                    int size = sb.Capacity;
                    if (!QueryFullProcessImageName(hProcess, 0, sb, ref size))
                    {
                        return null;
                    }

                    var filename = sb.ToString();

                    return new ProcessInfo
                    {
                        path = filename,
                        description = FileVersionInfo.GetVersionInfo(filename).FileDescription,
                    };
                }
                catch 
                {
                    return null;
                }
                finally
                {
                    if (hProcess != IntPtr.Zero)
                    {
                        CloseHandle(hProcess);
                    }
                }

            }
        }
    }
}
