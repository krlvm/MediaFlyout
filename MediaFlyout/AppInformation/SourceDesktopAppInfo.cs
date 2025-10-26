using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using MediaFlyout.Helpers;
using static MediaFlyout.Interop.User32;

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
            {
                return;
            }

            IntPtr hWnd = IsWindow(Data.MainWindowHandle)
                ? Data.MainWindowHandle : sourceProcess.MainWindowHandle;

            ActivateWindow(hWnd);
        }

        public override async void FetchInfosAsync()
        {
            await Task.Run(() =>
            {
                switch (Data.DataType)
                {
                    case SourceAppInfoDataType.FromAppUserModelId:
                        var processName = Data.AppUserModelId.Substring(0, Data.AppUserModelId.Length - 4);
                        var processes = Process.GetProcessesByName(processName);
                        if (processes?.Length > 0)
                        {
                            sourceProcess = processes[0];
                        }
                        break;
                    case SourceAppInfoDataType.FromProcessId:
                        sourceProcess = Process.GetProcessById((int)Data.ProcessId);
                        break;
                }

                if (sourceProcess == null)
                {
                    return;
                }

                ProcessModule mainModule;
                try
                {
                    mainModule = sourceProcess.MainModule;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    return;
                }

                DisplayName = mainModule.FileVersionInfo.FileDescription;

                var path = mainModule.FileName;
                var ie = new IconExtractor(path);
                var icon = ie.GetIcon(0);
                var bitmap = icon.ToBitmap();
                icon.Dispose();

                if (bitmap != null)
                {
                    var memoryStream = new MemoryStream();
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

        private enum WindowSizeState
        {
            Normal,
            Minimized,
            Maximized,
            Unknown,
        }

        internal static void ActivateWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            if (GetWindowSizeState(hWnd) == WindowSizeState.Minimized)
            {
                ShowWindowAsync(hWnd, ShowWindowCommands.Restore);
            }

            SetForegroundWindow(hWnd);
            FlashWindow(hWnd, true);
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

        #endregion
    }
}