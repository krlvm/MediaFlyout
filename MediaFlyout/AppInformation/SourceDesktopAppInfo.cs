using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaFlyout.Helpers;
using static MediaFlyout.Interop.NativeMethods;

namespace MediaFlyout.AppInformation
{
    internal class SourceDesktopAppInfo : SourceAppInfo
    {
        public SourceDesktopAppInfo(SourceAppInfoData data)
        {
            Data = data;
        }

        public override event EventHandler InfoFetched;

        private Process sourceProcess;

        public override void Activate()
        {
            if (sourceProcess == null)
                return;

#if FALSE
            sourceProcess.Refresh();
            IntPtr hWnd;
            if (Data.MainWindowHandle != IntPtr.Zero && IsWindow(Data.MainWindowHandle))
            {
                hWnd = Data.MainWindowHandle;
            }
            else
            {
                if (sourceProcess.MainWindowHandle == IntPtr.Zero)
                {
                    sourceProcess.Refresh();
                }
                if (sourceProcess.MainWindowHandle == IntPtr.Zero)
                {
                    var handles = InteropHelper.EnumerateProcessWindowHandles(sourceProcess.Id);
                    hWnd = handles.Any() ? handles.First() : IntPtr.Zero;
                }
                else
                {
                    hWnd = sourceProcess.MainWindowHandle;
                }
            }
#else
            IntPtr hWnd = IsWindow(Data.MainWindowHandle)
                ? Data.MainWindowHandle : sourceProcess.MainWindowHandle;
#endif

            ActivateWindow(hWnd);
        }

        public override async void FetchInfosAsync()
        {
            Bitmap bitmap = new Bitmap(16, 16);

            await Task.Run(() =>
            {
                if (Data.DataType == SourceAppInfoDataType.FromAppUserModelId)
                {
                    var processName = Data.AppUserModelId.Substring(0, Data.AppUserModelId.Length - 4);
                    var processes = Process.GetProcessesByName(processName);

                    if (processes?.Length > 0)
                    {
                        sourceProcess = processes[0];
                    }
                }
                else if (Data.DataType == SourceAppInfoDataType.FromProcessId)
                {
                    sourceProcess = Process.GetProcessById((int)Data.ProcessId);
                }

                if (sourceProcess == null)
                    return;

                DisplayName = sourceProcess.MainModule.FileVersionInfo.FileDescription;

                var path = sourceProcess.MainModule.FileName;
                var ie = new IconExtractor(path);
                var icon = ie.GetIcon(0);
                bitmap = icon.ToBitmap();
                icon.Dispose();

                if (bitmap != null)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    LogoStream = memoryStream;
                }
                bitmap.Dispose();
            });

            InfoFetched?.Invoke(this, null);
        }

        protected override void Disconnect()
        {
            base.Disconnect();

            sourceProcess?.Dispose();
            sourceProcess = null;
        }

        #region Window activation things

        internal static void ActivateWindow(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                if (GetWindowSizeState(hWnd) == WindowSizeState.Minimized)
                {
                    ShowWindowAsync(hWnd, ShowWindowCommands.Restore);
                }

                SetForegroundWindow(hWnd);
                FlashWindow(hWnd, true);
            }
        }

        private static WindowSizeState GetWindowSizeState(IntPtr hWnd)
        {
            GetWindowPlacement(hWnd, out WINDOWPLACEMENT placement);

            switch (placement.ShowCmd)
            {
                case ShowWindowCommands.Normal: return WindowSizeState.Normal;
                case ShowWindowCommands.Minimize: case ShowWindowCommands.ShowMinimized: return WindowSizeState.Minimized;
                case ShowWindowCommands.Maximize: return WindowSizeState.Maximized;
                default: return WindowSizeState.Unknown;
            }
        }

        private enum WindowSizeState
        {
            Normal,
            Minimized,
            Maximized,
            Unknown,
        }

        #endregion
    }
}