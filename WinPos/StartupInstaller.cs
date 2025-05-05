using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace WinPos;

class StartupInstaller
{
    // Add this method to check for admin privileges
    internal static bool IsRunningAsAdmin()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    // Add this method to create a scheduled task for elevated privileges
    internal static bool QueryScheduledTask()
    {
        try
        {
            string? appPath = Environment.ProcessPath;
            string taskName = "WindowPositionManager";

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = $"/QUERY /TN \"{taskName}\"",
                Verb = "runas",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            Process? process = Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd();
            
            return output.Contains("Ready") || output.Contains("Running");
        }
        catch (Exception ex)
        { return false; }
    }

    // Add this method to create a scheduled task for elevated privileges
    internal static void CreateScheduledTask()
    {
        try
        {
            string? appPath = Environment.ProcessPath;
            string taskName = "WindowPositionManager";

            string xmlPath = "WindowPositionManager.xml";

            string xml = File.ReadAllText(xmlPath);
            xml = xml.Replace("<UserId></UserId>", $"<UserId>{WindowsIdentity.GetCurrent().User.Value}</UserId>");
            xml = Regex.Replace(xml, $@"<Exec>[\r\n\s]*<Command>""WinPos.exe""</Command>[\r\n\s]*<Arguments>""-minimized""</Arguments>[\r\n\s]*<WorkingDirectory>/</WorkingDirectory>[\r\n\s]*</Exec>",
                $"<Exec>\r\n      <Command>\"{Environment.ProcessPath}\"</Command>\r\n      <Arguments>\"-minimized\"</Arguments>\r\n      <WorkingDirectory>{Environment.CurrentDirectory}</WorkingDirectory>\r\n    </Exec>");

            xmlPath = $"{Path.GetTempPath()}{xmlPath}";
            File.WriteAllText(xmlPath, xml);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = $"/Create /TN \"{taskName}\" /XML \"{xmlPath}\" /F",
                Verb = "runas",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(psi)?.WaitForExit();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create scheduled task: {ex.Message}");
        }
    }

    internal static void DeleteScheduledTask()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = "/Delete /TN \"WindowPositionManager\" /F",
                Verb = "runas",
                UseShellExecute = true,
                CreateNoWindow = true
            })?.WaitForExit();
        }
        catch { /* Ignore if task doesn't exist */ }
    }

    public static void CreateStartMenuShortcut(string appName, string appPath, string description = "")
    {
        // Get the Start Menu folder path
        string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        string shortcutPath = Path.Combine(startMenuPath, "Programs", $"{appName}.lnk");

        // Check if the shortcut already exists
        if (!File.Exists(shortcutPath))
        {
            // Create the shortcut
            IWshRuntimeLibrary.WshShell shell = new();
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = appPath; // Path to the application
            shortcut.WorkingDirectory = Path.GetDirectoryName(appPath);
            shortcut.Description = description; // Optional description
            shortcut.Save();
        }
    }
}

