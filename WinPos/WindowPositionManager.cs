using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static WinPos.MainForm;
using static WinPos.NativeMethods;
using System.Xml.Serialization;

namespace WinPos
{
    class WindowPositionManager
    {
        private static List<WindowInfo>? _savedWindows;
        internal static int HOTKEY_ID = 1;

        private static string[] excludedProcesses, excludedTitles;

        static WindowPositionManager()
        {
            excludedTitles = File.ReadAllLines("excluded_titles.txt");
            excludedProcesses = File.ReadAllLines("excluded_processes.txt");
        }

        internal static void RegisterHotKey(IntPtr handle)
        {
            if (!NativeMethods.RegisterHotKey(handle, HOTKEY_ID,
                MOD_WIN | MOD_SHIFT,
                VK_SUBTRACT))
            {
                MessageBox.Show("Failed to register hotkey");
            }
        }

        internal static void SaveWindowPositions()
        {
            _savedWindows = new List<WindowInfo>();
            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd)) return true;

                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                placement.length = Marshal.SizeOf(placement);
                GetWindowPlacement(hWnd, ref placement);

                if (placement.showCmd != SW_SHOWMINIMIZED)
                {
                    string windowText = GetWindowText(hWnd);

                    if (windowText != null && windowText.Length >= 5 && !excludedTitles.Any(t => windowText.StartsWith(t)))
                    {
                        GetWindowRect(hWnd, out RECT rect);
                        string exeName = GetProcessNameFromWindow(hWnd);

                        if (!excludedProcesses.Contains(exeName))
                            _savedWindows.Add(new WindowInfo(hWnd, rect, windowText, exeName));
                    }
                }

                return true;
            }, IntPtr.Zero);

            SaveToDisk();
        }

        internal static void RestoreWindowPositions()
        {
            if (_savedWindows == null || _savedWindows.Count == 0) return;

            var currentWindows = new List<WindowInfo>();

            EnumWindows((hWnd, lParam) =>
            {
                // Collect current window information
                string windowText = GetWindowText(hWnd);
                string exeName = GetProcessNameFromWindow(hWnd);

                if (windowText != null && windowText.Length >= 5 && !string.IsNullOrEmpty(exeName)
                    && GetWindowRect(hWnd, out RECT rect))
                    currentWindows.Add(new WindowInfo(hWnd, rect, windowText, exeName));

                return true;
            }, IntPtr.Zero);

            foreach (var savedWindow in _savedWindows)
            {
                // Find matching windows by title and executable name
                var matches = currentWindows.Where(w => w.ExecutableName == savedWindow.ExecutableName &&
                    w.WindowTitle != null && w.WindowTitle.StartsWith(savedWindow.WindowTitle.Substring(0, 5))
                ).ToList();

                foreach (var match in matches)
                {
                    SetWindowPlacement(match.Handle, new WINDOWPLACEMENT
                    {
                        length = Marshal.SizeOf(typeof(WINDOWPLACEMENT)),
                        flags = 0,
                        showCmd = SW_SHOWNORMAL,
                        rcNormalPosition = match.Rect
                    });

                    var res = SetWindowPos(match.Handle, IntPtr.Zero,
                        savedWindow.Left, savedWindow.Top,
                        savedWindow.Right - savedWindow.Left,
                        savedWindow.Bottom - savedWindow.Top,
                        SWP_NOZORDER | SWP_NOACTIVATE);
                }
            }
        }

        internal static string GetWindowText(nint hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            StringBuilder windowText = new StringBuilder(length + 1);
            NativeMethods.GetWindowText(hWnd, windowText, windowText.Capacity);
            return windowText.ToString();
        }

        internal static string GetProcessNameFromWindow(IntPtr hWnd)
        {
            IntPtr processHandle = default;

            try
            {
                uint processId;
                GetWindowThreadProcessId(hWnd, out processId);
                processHandle = OpenProcess(ProcessAccessFlags.PROCESS_QUERY_LIMITED_INFORMATION, false, processId);

                if (processHandle == IntPtr.Zero)
                    return "unknown";

                StringBuilder exePath = new StringBuilder(1024);
                int bufferSize = exePath.Capacity;

                if (QueryFullProcessImageName(processHandle, 0, exePath, ref bufferSize))
                    return Path.GetFileName(exePath.ToString()).ToLower();

                return "unknown";
            }
            catch
            {
                return "unknown";
            }
            finally
            {
                if (processHandle != IntPtr.Zero)
                    NativeMethods.CloseHandle(processHandle);
            }
        }

        internal static void SaveToDisk()
        {
            try
            {
                var dataToSave = _savedWindows.Select(w => new WindowInfo(
                    w.Handle,
                    w.Rect,
                    w.WindowTitle,
                    w.ExecutableName
                )).ToList();

                string savePath = Path.GetTempPath() + "WinPos\\windowPositions.xml";
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));

                using (var writer = new StreamWriter(savePath))
                {
                    var serializer = new XmlSerializer(typeof(List<WindowInfo>));
                    serializer.Serialize(writer, dataToSave);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Save error: {ex.Message}");
            }
        }

        internal static void LoadFromDisk()
        {
            string path = Path.GetTempPath() + "WinPos\\windowPositions.xml";

            try
            {
                using (var reader = new StreamReader(path))
                {
                    var serializer = new XmlSerializer(typeof(List<WindowInfo>));
                    var loadedData = (List<WindowInfo>)serializer.Deserialize(reader);

                    _savedWindows = new List<WindowInfo>();
                    foreach (var w in loadedData)
                    {
                        _savedWindows.Add(new WindowInfo(
                            IntPtr.Zero, // Handle will be looked up during restore
                            w.Rect,
                            w.WindowTitle,
                            w.ExecutableName
                        ));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load error: {ex.Message}");
            }
        }
    }
}
