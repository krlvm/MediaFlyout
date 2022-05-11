using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaFlyout.Interop
{
    class Kernel32
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern int GetApplicationUserModelId(
            IntPtr hProcess,
            ref int applicationUserModelIdLength,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder applicationUserModelId);
    }
}
