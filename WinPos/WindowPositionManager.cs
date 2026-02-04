using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;
using static WinPos.MainForm;
using static WinPos.NativeMethods;

namespace WinPos
{
    class WindowPositionManager
    {
        private static List<WindowInfo>? _savedWindows;
        internal static int HOTKEY_ID = 1;

        // Use HashSet for O(1) lookups instead of array with O(n) Contains/Any
        private static readonly HashSet<string> excludedProcesses;
        private static readonly string[] excludedTitles;
        
        // Use Lazy<T> to defer costly XmlSerializer reflection until first use
        private static readonly Lazy<XmlSerializer> _windowInfoSerializer = 
            new(() => new XmlSerializer(typeof(List<WindowInfo>)), LazyThreadSafetyMode.PublicationOnly);
        
        // Reusable StringBuilder to reduce allocations
        [ThreadStatic]
        private static StringBuilder? _windowTextBuffer;

        static WindowPositionManager()
        {
            excludedTitles = File.Exists("excluded_titles.txt") ? File.ReadAllLines("excluded_titles.txt") : [];
            excludedProcesses = File.Exists("excluded_processes.txt") ?
                new HashSet<string>(File.ReadAllLines("excluded_processes.txt"), StringComparer.OrdinalIgnoreCase) : [];
        }

        internal static void RegisterHotKey(IntPtr handle,
            bool ctrl, bool shift, bool win, bool alt, uint key)
        {
            uint modifiers = 0;
            modifiers |= ctrl ? MOD_CONTROL : 0;
            modifiers |= shift ? MOD_SHIFT : 0;
            modifiers |= win ? MOD_WIN : 0;
            modifiers |= alt ? MOD_ALT : 0;

            try
            {
                if (!NativeMethods.RegisterHotKey(handle, HOTKEY_ID, modifiers, key))
                    throw new InvalidOperationException();
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to register hotkey");
            }
        }

        internal static void SaveWindowPositions()
        {
            var savedWindows = new List<WindowInfo>();
            
            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd)) return true;

                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                placement.length = Marshal.SizeOf(placement);
                GetWindowPlacement(hWnd, ref placement);

                if (placement.showCmd != SW_SHOWMINIMIZED)
                {
                    string windowText = GetWindowTextOptimized(hWnd);

                    if (windowText.Length >= 5 &&
                    !excludedTitles.Any(t => windowText.StartsWith(t, StringComparison.Ordinal)))
                    {
                        Debug.WriteLine($"Window: {windowText}");
                        GetWindowRect(hWnd, out RECT rect);
                        string exeName = GetProcessNameFromWindow(hWnd);

                        if (!excludedProcesses.Contains(exeName))
                            savedWindows.Add(new WindowInfo(hWnd, rect, windowText, exeName));
                    }
                }

                return true;
            }, IntPtr.Zero);

            _savedWindows = savedWindows;
            SaveToDisk();
        }

        internal static void RestoreWindowPositions()
        {
            if (_savedWindows == null || _savedWindows.Count == 0) return;

            var currentWindows = new List<WindowInfo>();

            EnumWindows((hWnd, lParam) =>
            {
                // Collect current window information
                string windowText = GetWindowTextOptimized(hWnd);
                string exeName = GetProcessNameFromWindow(hWnd);

                if (windowText.Length >= 5 && !string.IsNullOrEmpty(exeName)
                    && GetWindowRect(hWnd, out RECT rect))
                    currentWindows.Add(new WindowInfo(hWnd, rect, windowText, exeName));

                return true;
            }, IntPtr.Zero);

            // Pre-calculate window placement struct size once
            int placementSize = Marshal.SizeOf(typeof(WINDOWPLACEMENT));

            foreach (var savedWindow in _savedWindows)
            {
                // Cache the prefix for matching
                string? savedTitlePrefix = savedWindow.WindowTitle?.Length >= 5 
                    ? savedWindow.WindowTitle.Substring(0, 5) 
                    : null;
                    
                if (savedTitlePrefix == null) continue;

                foreach (var match in currentWindows)
                {
                    if (match.ExecutableName == savedWindow.ExecutableName &&
                        match.WindowTitle != null && 
                        match.WindowTitle.StartsWith(savedTitlePrefix, StringComparison.Ordinal))
                    {
                        SetWindowPlacement(match.Handle, new WINDOWPLACEMENT
                        {
                            length = placementSize,
                            flags = 0,
                            showCmd = SW_SHOWNORMAL,
                            rcNormalPosition = match.Rect
                        });

                        SetWindowPos(match.Handle, IntPtr.Zero,
                            savedWindow.Left, savedWindow.Top,
                            savedWindow.Right - savedWindow.Left,
                            savedWindow.Bottom - savedWindow.Top,
                            SWP_NOZORDER | SWP_NOACTIVATE);
                    }
                }
            }
        }
        
        private static string GetWindowTextOptimized(nint hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            if (length == 0) return string.Empty;
            
            // Reuse StringBuilder when possible
            _windowTextBuffer ??= new StringBuilder(256);
            
            if (_windowTextBuffer.Capacity < length + 1)
                _windowTextBuffer.Capacity = length + 1;
            
            _windowTextBuffer.Clear();
            NativeMethods.GetWindowText(hWnd, _windowTextBuffer, length + 1);
            return _windowTextBuffer.ToString();
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
                GetWindowThreadProcessId(hWnd, out uint processId);
                processHandle = OpenProcess(ProcessAccessFlags.PROCESS_QUERY_LIMITED_INFORMATION, false, processId);

                if (processHandle == IntPtr.Zero)
                    return "unknown";

                StringBuilder exePath = new StringBuilder(1024);
                int bufferSize = exePath.Capacity;

                if (QueryFullProcessImageName(processHandle, 0, exePath, ref bufferSize))
                    return Path.GetFileName(exePath.ToString()).ToLowerInvariant();

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
            if (_savedWindows == null) return;
            
            try
            {
                string savePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    Application.ProductName, "windowPositions.xml");

                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

                using (var writer = new StreamWriter(savePath))
                {
                    _windowInfoSerializer.Value.Serialize(writer, _savedWindows);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Save error: {ex.Message}");
            }
        }

        internal static void LoadFromDisk()
        {
            try
            {
                string path = Path.GetTempPath() + "WinPos\\windowPositions.xml";

                if (!File.Exists(path)) return;

                using (var reader = new StreamReader(path))
                {
                    var loadedData = (List<WindowInfo>?)_windowInfoSerializer.Value.Deserialize(reader);

                    if (loadedData == null) return;

                    _savedWindows = new List<WindowInfo>(loadedData.Count);
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
