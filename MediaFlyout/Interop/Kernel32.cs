using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaFlyout.Interop
{
    class Kernel32
    {
        internal const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern int GetApplicationUserModelId(
            IntPtr hProcess,
            ref int applicationUserModelIdLength,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder applicationUserModelId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenProcess(int processAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags,
            StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);
    }
}
