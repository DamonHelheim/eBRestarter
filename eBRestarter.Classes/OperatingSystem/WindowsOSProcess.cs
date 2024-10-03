using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace eBRestarter.Classes.OperatingSystem
{
    public partial class WindowsOS
    {
        public Process? Process { get; set; }

        public void StartMSIFile(string installerMSIFilePath)
        {
            // Configure ProcessStartInfo
            ProcessStartInfo startInfo = new()
            {
                FileName = "msiexec.exe",
                Arguments = $"/i \"{installerMSIFilePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true, // Optional, to record the output
                RedirectStandardError = true   // Optional, to record errors
            };

            try
            {
                // Start process
                using Process process = Process.Start(startInfo)!;
                // Optional: Read output
                using (StreamReader output = process.StandardOutput)
                {
                    string outputText = output.ReadToEnd();
                    //Debug.WriteLine(outputText);
                }

                // Optional: Read error
                using (StreamReader error = process.StandardError)
                {
                    string errorText = error.ReadToEnd();
                    //Debug.WriteLine(errorText);
                }

                // Wait until the process is finished
                process.WaitForExit();
            }
            catch (Exception)
            {
                //Debug.WriteLine("Fehler beim Starten des Installationsprozesses: " + ex.Message);
            }
        }

        public void StartExecute(string installerEXEFilePath)
        {
            // Configure ProcessStartInfo
            ProcessStartInfo startInfo = new()
            {
                FileName = installerEXEFilePath,
                Arguments = string.Empty,       // Here you can specify arguments for the .exe file, if necessary
                UseShellExecute = true,         // Use True to let the operating system start the process
                RedirectStandardOutput = false, // Optional, set to true if required
                RedirectStandardError = false   // Optional, set to true if required
            };

            try
            {
                // Start process
                using Process process = Process.Start(startInfo)!;

                // Optional: Wait until the process is finished
                process.WaitForExit();
                //Debug.WriteLine("Process terminated with exit code:  " + process.ExitCode);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fehler beim Starten der Anwendung: " + ex.Message);
            }
        }

        public void OpenLinkWithStandardbrowser(string url)
        {
            try
            {
                // Opens the URL in the default browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An error occurred: " + ex.Message);
            }
        }

        public bool ProcessAlive(string name)
        {
            if (Process.GetProcessesByName(name).Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //This method terminate processes
        public void StopProcess(string nameOfProcess)
        {
            try
            {
                foreach (var process in Process.GetProcessesByName(nameOfProcess))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Win32Exception)
                    {
                        //It's nessecary to avoid an exception during the close Processes with the Google Chrome Browser
                    }
                }
            }
            catch (IOException e)
            {
                Log.Error(e, "Es ist eine Ausnahme im WindowsOSbereich aufgetreten. Logging...");
            }
        }


        public void CloseApplication(string nameOfApplication)
        {
            //CloseProcess(nameOfApplication);

            if (ProcessAlive(nameOfApplication) == true)
            {
                StopProcess(nameOfApplication);
            }
        }

        //This method closed processes
        //public void CloseProcess(string name)
        //{
        //    foreach (Process process in Process.GetProcessesByName(name))
        //    {
        //        if (process.MainWindowHandle == IntPtr.Zero) // some have no UI
        //        {
        //            continue;
        //        }

        //        AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);

        //        if (element != null)
        //        {

        //            ((WindowPattern)element.GetCurrentPattern(WindowPattern.Pattern)).Close();

        //        }
        //    }
        //}


        //Close all programs that are open in explorer
        public void CloseAllApplications()
        {
            Process me = Process.GetCurrentProcess();
            foreach (Process p in Process.GetProcesses())
            {
                if (p.Id != me.Id && IntPtr.Zero != p.MainWindowHandle && false == p.ProcessName.Equals("explorer", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Sends WM_CLOSE; less gentle methods available too.
                    p.CloseMainWindow();
                }
            }
        }


        // Import user32.dll to use the PostMessage function
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // Constants for the WM_CLOSE message
        private const uint WM_CLOSE = 0x0010;

        // Method to close all open programs
        public void CloseAllOpenPrograms()
        {
            // Get the list of all running processes
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                // Skip the system processes
                if (process.ProcessName == "System" || process.ProcessName == "Idle")
                    continue;

                try
                {
                    // Send the WM_CLOSE message to the main window of the process
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        PostMessage(process.MainWindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                        process.WaitForExit(5000); // Wait up to 5 seconds for the process to exit
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle the exception if needed
                    Console.WriteLine($"Error closing process {process.ProcessName}: {ex.Message}");
                }
            }

        }

        // Method to shut down the computer
        public void ShutdownComputer()
        {
            Process.Start("shutdown", "/r /f /t 0");
        }

        // Method to close all programs and shut down the computer
        public void CloseProgramsAndShutdown()
        {
            CloseAllOpenPrograms();
            ShutdownComputer();
        }

    }
}
