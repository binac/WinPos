using System.Diagnostics;

namespace WinPos
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //#if DEBUG
            //            Debugger.Launch();
            //#endif

            // Kill app if already running. Preserve the current instance.
            Process currentProcess = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(currentProcess.ProcessName))
                if (process.Id != currentProcess.Id)
                    process.Kill();

            ApplicationConfiguration.Initialize();

            if (StartupInstaller.IsRunningAsAdmin())
            {
                if (args.Length > 0 && args[0] == "--uninstall")
                {
                    // Remove scheduled task
                    StartupInstaller.DeleteScheduledTask();
                    Application.Exit();
                    return;
                }

                if (!StartupInstaller.QueryScheduledTask())
                {
                    // Configure scheduled task
                    StartupInstaller.CreateScheduledTask();
                    MessageBox.Show("Application installed successfully!");
                    Application.Exit();
                }

                string appPath = Application.ExecutablePath; // Path to the current executable
                string appName = Application.ProductName ?? "WinPos"; // Ensure appName is not null
                StartupInstaller.CreateStartMenuShortcut(appName, appPath, "Save and restore window positions.");
            }

            Application.Run(new MainForm());
        }
    }
}