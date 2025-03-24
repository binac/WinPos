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

            // Kill app if already running
            Process currentProcess = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(currentProcess.ProcessName))
                if (process.Id != currentProcess.Id)
                    process.Kill();

            ApplicationConfiguration.Initialize();

            if (StartupInstaller.IsRunningAsAdmin())
            {
                if (args.Length > 0 && args[0] == "--uninstall")
                {
                    // Remove startup and scheduled task
                    //StartupInstaller.ConfigureStartup(false);
                    StartupInstaller.DeleteScheduledTask();
                    //MessageBox.Show("Application uninstalled successfully!");
                    Application.Exit();

                    return;
                }

                if (!StartupInstaller.QueryScheduledTask())
                {
                    // Configure startup and scheduled task
                    //StartupInstaller.ConfigureStartup(true);
                    StartupInstaller.CreateScheduledTask();
                    MessageBox.Show("Application installed successfully!");
                    Application.Exit();
                    //return;
                }
            }

            Application.Run(new MainForm());
        }
    }
}