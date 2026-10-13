using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using System.Threading.Tasks;
using MediaFlyout.Helpers;
using MediaFlyout.Utilities;
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

        public override void Activate(string mediaTitle, string mediaArtist)
        {
            if (TryActivateMatchingTab(mediaTitle, mediaArtist))
            {
                return;
            }

            Activate();
        }

        private bool TryActivateMatchingTab(string mediaTitle, string mediaArtist)
        {
            if (sourceProcess == null || string.IsNullOrWhiteSpace(mediaTitle))
            {
                return false;
            }

            var windows = new List<IntPtr>();
            try
            {
                foreach (ProcessThread thread in sourceProcess.Threads)
                {
                    EnumThreadWindows(thread.Id, (hWnd, lParam) =>
                    {
                        if (hWnd != IntPtr.Zero && !windows.Contains(hWnd))
                        {
                            windows.Add(hWnd);
                        }
                        return true;
                    }, IntPtr.Zero);
                }
            }
            catch
            {
                return false;
            }

            if (windows.Count == 0 && sourceProcess.MainWindowHandle != IntPtr.Zero)
            {
                windows.Add(sourceProcess.MainWindowHandle);
            }

            var normalizedTitle = NormalizeTabName(mediaTitle);
            if (normalizedTitle.Length < 3)
            {
                return false;
            }

            var normalizedArtist = NormalizeTabName(mediaArtist);
            AutomationElement matchingTab = null;
            IntPtr matchingWindow = IntPtr.Zero;
            var bestMatchScore = -1;

            foreach (var hWnd in windows)
            {
                try
                {
                    var root = AutomationElement.FromHandle(hWnd);
                    var tabs = root.FindAll(TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));

                    foreach (AutomationElement tab in tabs)
                    {
                        var tabName = NormalizeTabName(tab.Current.Name);
                        if (tabName.Length < 3 || !tabName.Contains(normalizedTitle))
                        {
                            continue;
                        }

                        var score = normalizedArtist.Length >= 3 && tabName.Contains(normalizedArtist) ? 2 : 1;
                        if (score > bestMatchScore)
                        {
                            bestMatchScore = score;
                            matchingTab = tab;
                            matchingWindow = hWnd;
                        }
                    }
                }
                catch
                {
                    // A browser window can disappear or change its accessibility tree while scanning.
                }
            }

            if (matchingTab != null)
            {
                try
                {
                    ActivateWindow(matchingWindow);
                    var selection = matchingTab.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                    selection?.Select();
                    matchingTab.SetFocus();
                    return true;
                }
                catch
                {
                    // The browser may rebuild its accessibility tree between lookup and activation.
                }
            }

            return false;
        }

        private static string NormalizeTabName(string value)
        {
            return Regex.Replace(value ?? string.Empty, @"[^\p{L}\p{N}]+", " ").Trim().ToLowerInvariant();
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
                        if (processes.Length > 0)
                        {
                            sourceProcess = processes.FirstOrDefault(process => process.MainWindowHandle != IntPtr.Zero)
                                ?? processes[0];
                            foreach (var process in processes)
                            {
                                if (process != sourceProcess)
                                {
                                    process.Dispose();
                                }
                            }
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

                var processInfo = ProcessInfoExtractor.ExtractInfo(sourceProcess);
                if (processInfo == null)
                {
                    return;   
                }

                if (string.IsNullOrEmpty(processInfo.Value.description))
                {
                    DisplayName = Path.GetFileName(processInfo.Value.path);
                }
                else
                {
                    DisplayName = processInfo.Value.description;
                }

                var ie = new IconExtractor(processInfo.Value.path);
                if (ie.Count == 0)
                {
                    return;
                }

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
