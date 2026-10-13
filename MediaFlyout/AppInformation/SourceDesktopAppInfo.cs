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

            // Opening the browser's main window is not a successful match: it may
            // leave the user on an unrelated tab. Keep that fallback for ordinary
            // desktop apps, but never use it for a browser media card.
            if (!IsBrowserProcess())
            {
                Activate();
            }
        }

        private bool TryActivateMatchingTab(string mediaTitle, string mediaArtist)
        {
            if (sourceProcess == null || string.IsNullOrWhiteSpace(mediaTitle))
            {
                return false;
            }

            var normalizedTitle = NormalizeTabName(mediaTitle);
            if (normalizedTitle.Length < 3)
            {
                return false;
            }
            var normalizedArtist = NormalizeTabName(mediaArtist);

            string processName;
            try
            {
                processName = sourceProcess.ProcessName;
            }
            catch
            {
                return false;
            }

            var browserProcesses = new List<Process>();
            try
            {
                browserProcesses.AddRange(Process.GetProcessesByName(processName));
            }
            catch
            {
                return false;
            }

            var previousWindow = GetForegroundWindow();
            var initialBrowserWindow = IsWindow(Data.MainWindowHandle)
                ? Data.MainWindowHandle : sourceProcess.MainWindowHandle;
            if (IsWindow(initialBrowserWindow))
            {
                // Some browser accessibility providers expose the tab strip reliably
                // only after their window becomes active.
                ActivateWindow(initialBrowserWindow);
            }

            AutomationElement matchingTab = null;
            IntPtr matchingWindow = IntPtr.Zero;
            int[] matchingRuntimeId = null;
            var bestMatchScore = int.MinValue;
            var bestMatchIsSelected = false;
            var ambiguousBestMatch = false;

            try
            {
                foreach (var process in browserProcesses)
                {
                    var windows = new List<IntPtr>();
                    try
                    {
                        foreach (ProcessThread thread in process.Threads)
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

                        if (process.MainWindowHandle != IntPtr.Zero && !windows.Contains(process.MainWindowHandle))
                        {
                            windows.Add(process.MainWindowHandle);
                        }
                    }
                    catch
                    {
                        // Browser subprocesses can exit while the tab list is read.
                        continue;
                    }

                    foreach (var hWnd in windows)
                    {
                        try
                        {
                            var root = AutomationElement.FromHandle(hWnd);
                            var tabs = root.FindAll(TreeScope.Descendants,
                                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));

                            foreach (AutomationElement tab in tabs)
                            {
                                try
                                {
                                    var tabName = NormalizeTabName(tab.Current.Name);
                                    if (tabName.Length < 3 || !tabName.Contains(normalizedTitle))
                                    {
                                        continue;
                                    }

                                    var selection = tab.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                                    var isSelected = selection?.Current.IsSelected == true;
                                    var score = 100 + Math.Min(normalizedTitle.Length, 100);
                                    if (normalizedArtist.Length >= 3 && tabName.Contains(normalizedArtist)) score += 100;
                                    if (tabName.Contains("audio playing") || tabName.Contains("playing audio")) score += 25;
                                    if (isSelected) score += 10;

                                    if (score > bestMatchScore)
                                    {
                                        bestMatchScore = score;
                                        matchingTab = tab;
                                        matchingWindow = hWnd;
                                        matchingRuntimeId = tab.GetRuntimeId();
                                        bestMatchIsSelected = isSelected;
                                        ambiguousBestMatch = false;
                                    }
                                    else if (score == bestMatchScore)
                                    {
                                        // Prefer the already-selected tab on an exact tie;
                                        // otherwise refuse to open an arbitrary duplicate.
                                        if (isSelected && !bestMatchIsSelected)
                                        {
                                            matchingTab = tab;
                                            matchingWindow = hWnd;
                                            matchingRuntimeId = tab.GetRuntimeId();
                                            bestMatchIsSelected = true;
                                            ambiguousBestMatch = false;
                                        }
                                        else if (isSelected == bestMatchIsSelected)
                                        {
                                            ambiguousBestMatch = true;
                                        }
                                    }
                                }
                                catch
                                {
                                    // Individual tab accessibility nodes can become stale.
                                }
                            }
                        }
                        catch
                        {
                            // A browser window may close or rebuild its tab tree mid-scan.
                        }
                    }
                }
            }
            finally
            {
                foreach (var process in browserProcesses)
                {
                    process.Dispose();
                }
            }

            if (matchingTab == null || matchingRuntimeId == null || ambiguousBestMatch || !IsWindow(matchingWindow))
            {
                RestoreForegroundWindow(previousWindow);
                return false;
            }

            try
            {
                // UI Automation may refuse to select a tab in a background browser.
                // Bring the owning browser window forward first, then reacquire the
                // tab because activation can rebuild the browser's automation tree.
                ActivateWindow(matchingWindow);
                var root = AutomationElement.FromHandle(matchingWindow);
                var tabs = root.FindAll(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));
                AutomationElement currentTab = null;
                foreach (AutomationElement tab in tabs)
                {
                    try
                    {
                        if (tab.GetRuntimeId().SequenceEqual(matchingRuntimeId))
                        {
                            currentTab = tab;
                            break;
                        }
                    }
                    catch
                    {
                        // Ignore stale tab nodes while reacquiring the selected target.
                    }
                }

                var selection = currentTab?.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                if (selection == null)
                {
                    if (IsWindow(previousWindow)) SetForegroundWindow(previousWindow);
                    return false;
                }

                selection.Select();
                currentTab.SetFocus();
                return true;
            }
            catch
            {
                if (IsWindow(previousWindow)) SetForegroundWindow(previousWindow);
                return false;
            }
        }

        private bool IsBrowserProcess()
        {
            try
            {
                return new[] { "msedge", "chrome", "firefox", "opera", "brave", "vivaldi" }
                    .Contains(sourceProcess?.ProcessName, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static void RestoreForegroundWindow(IntPtr hWnd)
        {
            if (IsWindow(hWnd))
            {
                SetForegroundWindow(hWnd);
            }
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
